using System.Buffers;

namespace wan24.Core
{
    /// <summary>
    /// Array pool stream (stores in arrays from an <see cref="ArrayPool{T}"/>)
    /// </summary>
    public partial class ArrayPoolStream : StreamBase, IStatusProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public ArrayPoolStream(in ArrayPool<byte>? pool = null, in int? bufferSize = null) : base()
        {
            Pool = pool ?? ArrayPool<byte>.Shared;
            if (bufferSize.HasValue) BufferSize = bufferSize.Value;
            Buffers = [Pool.Rent(BufferSize)];
            HasUpdates = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Initial data (will be returned to the pool, if <c>returnData</c> is <see langword="true"/>; may be overwritten; initial position 
        /// will be <c>0</c>)</param>
        /// <param name="returnData">If to return the <c>data</c> to the pool when disposing (if <see langword="false"/>, <c>data</c> may not be a rented array)</param>
        /// <param name="len">Number of bytes to take from <c>data</c></param>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public ArrayPoolStream(in byte[] data, in bool returnData, in int len = -1, in ArrayPool<byte>? pool = null, in int? bufferSize = null) : base()
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(data.Length, other: 1, nameof(data));
            if (len >= 0) ArgumentOutOfRangeException.ThrowIfGreaterThan(len, data.Length, nameof(len));
            ReturnFirstBuffer = returnData;
            Pool = pool ?? ArrayPool<byte>.Shared;
            if (bufferSize.HasValue) BufferSize = bufferSize.Value;
            _Length = len < 0 ? data.Length : len;
            if (_Length == data.Length)
            {
                Buffers = [data, Pool.Rent(BufferSize)];
                BufferIndex = 1;
                LastBufferOffset = 0;
                HasUpdates = true;
            }
            else
            {
                Buffers = [data];
                LastBufferOffset = _Length;
                HasUpdates = _Length != 0;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Initial data (will be copied; initial position will be <c>0</c>)</param>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public ArrayPoolStream(in ReadOnlySpan<byte> data, in ArrayPool<byte>? pool = null, in int? bufferSize = null) : this(pool, bufferSize)
        {
            Write(data);
            Position = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Initial data (will be copied; initial position will be <c>0</c>)</param>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public ArrayPoolStream(in ReadOnlyMemory<byte> data, in ArrayPool<byte>? pool = null, in int? bufferSize = null) : this(data.Span, pool, bufferSize) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Initial data (will be copied; initial position will be <c>0</c>)</param>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public ArrayPoolStream(in byte[] data, in ArrayPool<byte>? pool = null, in int? bufferSize = null) : this(data.AsSpan(), pool, bufferSize) { }

        /// <summary>
        /// Get the written data as a new array
        /// </summary>
        /// <returns>Array</returns>
        public byte[] ToArray()
        {
            EnsureUndisposed();
            if (_Length == 0) return [];
            return HasUpdates
                ? UpdateBufferSequence().ToArray()
                : BufferSequence.ToArray();
        }

        /// <summary>
        /// Get the written data as read-only sequence
        /// </summary>
        /// <returns>Sequence</returns>
        public ReadOnlySequence<byte> ToReadOnlySequence()
        {
            EnsureUndisposed();
            if (_Length == 0) return ReadOnlySequence<byte>.Empty;
            return HasUpdates ? UpdateBufferSequence() : BufferSequence;
        }

        /// <inheritdoc/>
        public override void Flush() => EnsureUndisposed(allowDisposing: true);

        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed(allowDisposing: true);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            if (_Position == _Length) return 0;
            int len = buffer.Length;
            if (len == 0) return 0;
            int newPos = _Position + len;
            // Reduce the target buffer length, if required
            if (newPos > _Length)
            {
                len = _Length - _Position;
                buffer = buffer[..len];
                newPos = _Length;
            }
            // Copy the sequence to the buffer
            if (HasUpdates) UpdateBufferSequence();
            (len == _Length ? BufferSequence : BufferSequence.Slice(_Position, len)).CopyTo(buffer);
            // Update the buffer index and offset
            if (newPos == _Length)
            {
                // Red to the end
                _Position = newPos;
                BufferIndex = Buffers.Count - 1;
                BufferOffset = LastBufferOffset;
            }
            else
            {
                // Forward buffer index and offset
                newPos -= _Position;
                _Position += newPos;
                for (int i = 0, dataLen, forward; newPos != 0; newPos -= forward, BufferIndex++, BufferOffset = 0, i++)
                {
                    dataLen = i == 0
                        ? Buffers[BufferIndex].Length - BufferOffset
                        : Buffers[BufferIndex].Length;
                    forward = newPos > dataLen
                        ? dataLen
                        : newPos;
                    BufferOffset += forward;
                    if (BufferOffset != dataLen) break;
                }
                UpdateBufferOffsetAfterReading();
            }
            return len;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            if (_Position == _Length) return -1;
            int res = Buffers[BufferIndex][BufferOffset];
            BufferOffset++;
            _Position++;
            UpdateBufferOffsetAfterReading();
            return res;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(Read(buffer.Span));
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            return this.GenericSeek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value) => SetLength((int)value, clear: true);

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            int len = buffer.Length;
            if (len == 0) return;
            if (int.MaxValue - len < _Position) throw new OutOfMemoryException();
            int newLen = _Position + len;
            if (newLen > _Length) SetLength(newLen, clear: false);
            Span<byte> data;
            for (int written = 0, chunk, dataLen; written != len; written += chunk, BufferIndex++, BufferOffset = 0)
            {
                data = Buffers[BufferIndex].AsSpan();
                dataLen = data.Length;
                chunk = Math.Min(dataLen - BufferOffset, len - written);
                buffer.Slice(written, chunk).CopyTo(data[BufferOffset..]);
                _Position += chunk;
                BufferOffset += chunk;
                if (BufferOffset != dataLen) break;
            }
            UpdateBufferOffsetAfterWriting();
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            if (_Position == int.MaxValue) throw new OutOfMemoryException();
            if (++_Position > _Length) SetLength(_Position, clear: false);
            Buffers[BufferIndex][BufferOffset] = value;
            BufferOffset++;
            UpdateBufferOffsetAfterWriting();
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Write(buffer.Span);
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public override void Close()
        {
            if (IsClosed) return;
            if (SaveOnClose) SavedData ??= ToArray();
            base.Close();
        }
    }
}

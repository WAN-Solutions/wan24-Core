using System.Buffers;

namespace wan24.Core
{
    /// <summary>
    /// Freezable memory pool stream (stores in memory from a <see cref="MemoryPool{T}"/>)
    /// </summary>
    public partial class FreezableMemoryPoolStream : StreamBase, IStatusProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public FreezableMemoryPoolStream(MemoryPool<byte>? pool = null, in int? bufferSize = null) : base()
        {
            Pool = pool ??= MemoryPool<byte>.Shared;
            if (bufferSize.HasValue) BufferSize = bufferSize.Value;
            IMemoryOwner<byte> buffer = pool.Rent(bufferSize ?? BufferSize);
            Buffers = [buffer];
            TotalBufferLengths = [buffer.Memory.Length];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Initial data (will be returned to the pool, if <c>returnData</c> is <see langword="true"/>; may be overwritten; initial position 
        /// will be <c>0</c>)</param>
        /// <param name="returnData">If to return the <c>data</c> to the pool when disposing (if <see langword="false"/>, <c>data</c> may not be a rented array)</param>
        /// <param name="len">Number of bytes to take from <c>data</c>, or <c>-1</c> to take all</param>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public FreezableMemoryPoolStream(in IMemoryOwner<byte> data, in bool returnData, int len = -1, MemoryPool<byte>? pool = null, in int? bufferSize = null) : base()
        {
            int dataLen = data.Memory.Length;
            ArgumentOutOfRangeException.ThrowIfLessThan(dataLen, other: 1, nameof(data));
            if (len > 0) ArgumentOutOfRangeException.ThrowIfGreaterThan(len, dataLen, nameof(len));
            ReturnFirstBuffer = returnData;
            Pool = pool ??= MemoryPool<byte>.Shared;
            if (bufferSize.HasValue) BufferSize = bufferSize.Value;
            len = _Length = len < 0 ? dataLen : len;
            HasLengthChanged = true;
            if (len == dataLen)
            {
                // Use the whole given data and add a spare buffer
                IMemoryOwner<byte> buffer = pool.Rent(bufferSize ?? BufferSize);
                Buffers = [data, buffer];
                TotalBufferLengths = [len, len + buffer.Memory.Length];
                _LastBufferIndex = 1;
            }
            else
            {
                // Use the partial given data as buffer
                Buffers = [data];
                TotalBufferLengths = [dataLen];
                _LastBufferOffset = len;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Initial data (will be copied; initial position will be <c>0</c>)</param>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public FreezableMemoryPoolStream(in ReadOnlySpan<byte> data, in MemoryPool<byte>? pool = null, in int? bufferSize = null) : this(pool, bufferSize)
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
        public FreezableMemoryPoolStream(in ReadOnlyMemory<byte> data, in MemoryPool<byte>? pool = null, in int? bufferSize = null) : this(data.Span, pool, bufferSize) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Initial data (will be copied; initial position will be <c>0</c>)</param>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public FreezableMemoryPoolStream(in byte[] data, in MemoryPool<byte>? pool = null, in int? bufferSize = null) : this(data.AsSpan(), pool, bufferSize) { }

        /// <summary>
        /// Freeze
        /// </summary>
        public void Freeze()
        {
            EnsureUndisposed();
            if (!Buffers.IsFrozen)
            {
                Buffers.Freeze();
                TotalBufferLengths.Freeze();
            }
        }

        /// <summary>
        /// Unfreeze
        /// </summary>
        public void Unfreeze()
        {
            EnsureUndisposed();
            if (!Buffers.IsFrozen)
            {
                Buffers.Unfreeze();
                TotalBufferLengths.Unfreeze();
            }
        }

        /// <summary>
        /// Get the written data as a new array
        /// </summary>
        /// <returns>Array</returns>
        public byte[] ToArray()
        {
            EnsureUndisposed(allowDisposing: true);
            if (_Length == 0) return [];
            return HasLengthChanged
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
            return HasLengthChanged
                ? UpdateBufferSequence()
                : BufferSequence;
        }

        /// <summary>
        /// Optimize
        /// </summary>
        public void Optimize()
        {
            EnsureUndisposed();
            FreezableList<IMemoryOwner<byte>> buffers = Buffers;
            int len = buffers.Count,
                firstRemoved = _LastBufferIndex + 2;
            if (firstRemoved > len) return;
            if (CleanReturned)
            {
                IMemoryOwner<byte> buffer;
                for (int i = firstRemoved; i < len; buffer = buffers[i], buffer.Memory.Span.Clear(), buffer.Dispose(), i++) ;
            }
            else if(buffers.IsFrozen)
            {
                buffers.Frozen!.Value[firstRemoved..].DisposeAll();
            }
            else
            {
                for (int i = firstRemoved; i < len; buffers[i].Dispose(), i++) ;
            }
            buffers.RemoveRange(firstRemoved, len - firstRemoved);
            TotalBufferLengths.RemoveRange(firstRemoved, len - firstRemoved);
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
            EnsureReadable();
            int pos = _Position,
                length = _Length;
            if (pos == length) return 0;
            int len = buffer.Length;
            if (len == 0) return 0;
            long newPos = (long)pos + len;
            // Reduce the target buffer length, if required
            if (newPos > length)
            {
                len = length - pos;
                buffer = buffer[..len];
                newPos = length;
            }
            // Copy the sequence to the buffer
            if (HasLengthChanged) UpdateBufferSequence();
            (len == length ? BufferSequence : BufferSequence.Slice(pos, len)).CopyTo(buffer);
            Position = newPos;
            return len;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            EnsureReadable();
            int pos = _Position;
            if (pos == _Length) return -1;
            int bufferIndex = BufferIndex,
                bufferOffset = BufferOffset;
            IMemoryOwner<byte> buffer = Buffers[bufferIndex];
            int res = buffer.Memory.Span[bufferOffset];
            _Position = pos + 1;
            BufferOffset = ++bufferOffset;
            if (bufferOffset >= buffer.Memory.Length)
            {
                BufferIndex = bufferIndex + 1;
                BufferOffset = 0;
            }
            return res;
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Read(buffer.AsSpan(offset, count)));
        }

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(Read(buffer.Span));
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            EnsureSeekable();
            return this.GenericSeek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            EnsureWritable();
            SetLength((int)value, clear: true);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            int len = buffer.Length;
            if (len == 0) return;
            int pos = _Position;
            if (int.MaxValue - len < pos) throw new OutOfMemoryException();
            int newPos = pos + len,
                bufferIndex = BufferIndex,
                bufferOffset = BufferOffset;
            bool extended = newPos > _Length;
            if (extended) SetLength(newPos, clear: false);
            FreezableList<IMemoryOwner<byte>> buffers = Buffers;
            Span<byte> data = buffers[bufferIndex].Memory.Span;
            int chunk = data.Length - bufferOffset,
                written;
            // Fill up the current buffer
            if (chunk > 0)
            {
                written = Math.Min(chunk, len);
                buffer[..written].CopyTo(data[bufferOffset..]);
            }
            else
            {
                written = 0;
            }
            // Continue writing to the next buffers
            if (len != written)
            {
                for (
                    ;
                    len != written;
                    bufferIndex++,
                        data = buffers[bufferIndex].Memory.Span,
                        chunk = Math.Min(data.Length, len - written),
                        buffer.Slice(written, chunk).CopyTo(data),
                        written += chunk
                    ) ;
                (BufferIndex, BufferOffset) = (bufferIndex, chunk);
            }
            else
            {
                BufferOffset = bufferOffset + written;
            }
            _Position = newPos;
            if (!extended) UpdateBufferOffsetAfterWriting();
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            EnsureWritable();
            int pos = _Position;
            if (pos == int.MaxValue) throw new OutOfMemoryException();
            int newPos = pos + 1,
                bufferOffset = BufferOffset;
            bool extended = newPos > _Length;
            if (extended) SetLength(newPos, clear: false);
            Buffers[BufferIndex].Memory.Span[BufferOffset] = value;
            _Position = newPos;
            BufferOffset = bufferOffset + 1;
            if (!extended) UpdateBufferOffsetAfterWriting();
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureWritable();
            cancellationToken.ThrowIfCancellationRequested();
            Write(buffer.AsSpan(offset, count));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
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

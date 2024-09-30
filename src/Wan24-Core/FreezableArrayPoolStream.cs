using System.Buffers;
using System.Runtime;
using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Freezable array pool stream (stores in arrays from an <see cref="ArrayPool{T}"/>)
    /// </summary>
    public class FreezableArrayPoolStream : StreamBase, IStatusProvider
    {
        /// <summary>
        /// Default buffer size in bytes
        /// </summary>
        private static int _DefaultBufferSize = Settings.BufferSize;

        /// <summary>
        /// An object for static thread locking
        /// </summary>
        protected static readonly object StaticSyncObject = new();

        /// <summary>
        /// Buffers
        /// </summary>
        protected readonly FreezableList<byte[]> Buffers;
        /// <summary>
        /// Pool
        /// </summary>
        protected readonly ArrayPool<byte> Pool;
        /// <summary>
        /// Buffer sequence
        /// </summary>
        protected ReadOnlySequence<byte> BufferSequence = ReadOnlySequence<byte>.Empty;
        /// <summary>
        /// If having updates, and the <see cref="BufferSequence"/> is invalid
        /// </summary>
        protected bool HasUpdates = true;
        /// <summary>
        /// If writable
        /// </summary>
        protected bool _CanWrite = true;
        /// <summary>
        /// Buffer index
        /// </summary>
        protected int BufferIndex = 0;
        /// <summary>
        /// Buffer byte offset
        /// </summary>
        protected int BufferOffset = 0;
        /// <summary>
        /// Last buffer byte offset
        /// </summary>
        protected int LastBufferOffset = 0;
        /// <summary>
        /// Buffer size in bytes
        /// </summary>
        protected int _BufferSize = _DefaultBufferSize;
        /// <summary>
        /// Length in bytes
        /// </summary>
        protected int _Length = 0;
        /// <summary>
        /// Byte offset
        /// </summary>
        protected int _Position = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public FreezableArrayPoolStream(in ArrayPool<byte>? pool = null, in int? bufferSize = null) : base()
        {
            Pool = pool ?? ArrayPool<byte>.Shared;
            if (bufferSize.HasValue) BufferSize = bufferSize.Value;
            Buffers =
            [
                Pool.Rent(BufferSize)
            ];
            HasUpdates = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Initial data (will be copied; stream is writable; initial position will be <c>0</c>)</param>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public FreezableArrayPoolStream(in byte[] data, in ArrayPool<byte>? pool = null, in int? bufferSize = null) : this(pool, bufferSize)
        {
            Write(data);
            Position = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Initial data (will be copied; stream is writable; initial position will be <c>0</c>)</param>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public FreezableArrayPoolStream(in ReadOnlySpan<byte> data, in ArrayPool<byte>? pool = null, in int? bufferSize = null) : this(pool, bufferSize)
        {
            Write(data);
            Position = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Initial data (will be copied; stream is writable; initial position will be <c>0</c>)</param>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public FreezableArrayPoolStream(in ReadOnlyMemory<byte> data, in ArrayPool<byte>? pool = null, in int? bufferSize = null) : this(pool, bufferSize)
        {
            Write(data.Span);
            Position = 0;
        }

        /// <summary>
        /// Default buffer size in bytes
        /// </summary>
        public static int DefaultBufferSize
        {
            get => _DefaultBufferSize;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
                lock (StaticSyncObject) _DefaultBufferSize = value;
            }
        }

        /// <summary>
        /// Buffer size in bytes
        /// </summary>
        public int BufferSize
        {
            get => _BufferSize;
            set
            {
                EnsureUndisposed();
                EnsureWritable();
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
                _BufferSize = value;
            }
        }

        /// <summary>
        /// Total buffer length in bytes
        /// </summary>
        public long BufferLength => Buffers.Sum(b => (long)b.Length);

        /// <summary>
        /// Current number of buffers
        /// </summary>
        public int BufferCount => Buffers.Count;

        /// <summary>
        /// Clean returned buffers?
        /// </summary>
        public bool CleanReturned { get; set; } = Settings.ClearBuffers;

        /// <summary>
        /// Save the data on close?
        /// </summary>
        public bool SaveOnClose { get; set; }

        /// <summary>
        /// Saved data
        /// </summary>
        public byte[]? SavedData { get; protected set; }

        /// <inheritdoc/>
        public virtual IEnumerable<Status> State
        {
            get
            {
                yield return new(__("Name"), Name, __("Name of the stream"));
                yield return new(__("Type"), GetType().ToString(), __("Stream type"));
                if (StackInfo is not null)
                {
                    yield return new(__("Stack"), StackInfo.Stack, __("Instance creation stack"));
                    yield return new(__("Created"), StackInfo.Created, __("Instance creation time"));
                }
                yield return new(__("Size"), Length, __("Length in bytes"));
                yield return new(__("Buffers"), BufferCount, __("Number of buffers"));
                yield return new(__("Buffer"), BufferLength, __("All buffer length in bytes"));
                yield return new(__("Buffer size"), BufferSize, __("New buffer size in bytes"));
            }
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => _CanWrite;

        /// <inheritdoc/>
        public override long Length => _Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => _Position;
            set
            {
                EnsureUndisposed();
                ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(value));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other: _Length, nameof(value));
                int v = (int)value;
                _Position = v;
                // Find buffer index and byte offset
                if (v == 0)
                {
                    BufferIndex = 0;
                    BufferOffset = 0;
                }
                else
                {
                    BufferIndex = 0;
                    for (int pos = 0, add, len; ; BufferIndex++)
                    {
                        len = Buffers[BufferIndex].Length;
                        add = Math.Min(len, v - pos);
                        pos += add;
                        if (add == len && pos != v) continue;
                        BufferOffset = add;
                        break;
                    }
                    UpdateBufferOffsetAfterReading();
                }
            }
        }

        /// <summary>
        /// Freeze
        /// </summary>
        public void Freeze()
        {
            EnsureUndisposed();
            if (!_CanWrite) return;
            _CanWrite = false;
            Buffers.Freeze();
        }

        /// <summary>
        /// Unfreeze
        /// </summary>
        public void Unfreeze()
        {
            EnsureUndisposed(allowDisposing: true);
            if (_CanWrite) return;
            _CanWrite = true;
            Buffers.Unfreeze();
        }

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
            EnsureWritable();
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
            EnsureWritable();
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

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Unfreeze();
            foreach (byte[] buffer in Buffers)
            {
                if (CleanReturned) buffer.Clear();
                Pool.Return(buffer);
            }
            Buffers.Clear();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            Unfreeze();
            foreach (byte[] buffer in Buffers)
            {
                if (CleanReturned) buffer.Clear();
                Pool.Return(buffer);
            }
            Buffers.Clear();
            await base.DisposeCore().DynamicContext();
        }

        /// <summary>
        /// Update the buffer offset after reading
        /// </summary>
        protected void UpdateBufferOffsetAfterReading()
        {
            if (BufferIndex == Buffers.Count - 1 || BufferOffset != Buffers[BufferIndex].Length) return;
            BufferIndex++;
            BufferOffset = 0;
        }

        /// <summary>
        /// Update the buffer offset after writing
        /// </summary>
        protected void UpdateBufferOffsetAfterWriting()
        {
            // Inline
            if (BufferIndex != Buffers.Count - 1)
            {
                if (BufferOffset != Buffers[BufferIndex].Length) return;
                // Increase the buffer index
                BufferIndex++;
                BufferOffset = 0;
                return;
            }
            // Ensure a correct last buffer offset
            if (BufferOffset <= LastBufferOffset) return;
            LastBufferOffset = BufferOffset;
            HasUpdates = true;
            // Ensure the last buffer has space left (or add a new buffer)
            if (LastBufferOffset != Buffers[BufferIndex].Length) return;
            Buffers.Add(Pool.Rent(BufferSize));
            BufferIndex++;
            BufferOffset = LastBufferOffset = 0;
        }

        /// <summary>
        /// Set a new length
        /// </summary>
        /// <param name="value">New length in bytes</param>
        /// <param name="clear">Clear new buffers?</param>
        protected void SetLength(in int value, in bool clear)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfEqual(value, int.MaxValue, nameof(value));
            if (value == 0)
            {
                // Delete all data
                for (int i = 1, len = Buffers.Count; i < len; i++)
                {
                    if (CleanReturned) Buffers[i].Clear();
                    Pool.Return(Buffers[i]);
                }
                if (Buffers.Count > 1) Buffers.RemoveRange(1, Buffers.Count - 1);
                _Length = _Position = BufferIndex = BufferOffset = LastBufferOffset = 0;
                BufferSequence = ReadOnlySequence<byte>.Empty;
                HasUpdates = false;
            }
            else if (value < _Length)
            {
                // Delete some data
                BufferIndex = -1;
                int pos = 0,
                    add,
                    len;
                for (BufferIndex = 0; ; BufferIndex++)
                {
                    len = Buffers[BufferIndex].Length;
                    add = Math.Min(len, value - pos);
                    pos += add;
                    if (add == len && pos != value) continue;
                    BufferOffset = add;
                    break;
                }
                LastBufferOffset = BufferOffset;
                len = Buffers.Count;
                if (len > BufferIndex + 1)
                {
                    for (int i = BufferIndex + 1; i < len; i++)
                    {
                        if (CleanReturned) Buffers[i].Clear();
                        Pool.Return(Buffers[i]);
                    }
                    Buffers.RemoveRange(BufferIndex + 1, len - BufferIndex - 1);
                }
                _Length = value;
                if (_Position > value) _Position = value;
                HasUpdates = true;
            }
            else
            {
                // Add buffers
                int len = value - _Length;
                Span<byte> data = Buffers[^1].AsSpan();
                len -= Math.Min(data.Length - LastBufferOffset, len);
                if (clear) data[LastBufferOffset..].Clean();
                if (len == 0)
                {
                    LastBufferOffset += value - _Length;
                    if (LastBufferOffset == data.Length)
                    {
                        Buffers.Add(clear ? Pool.RentClean(BufferSize) : Pool.Rent(BufferSize));
                        LastBufferOffset = 0;
                    }
                }
                else
                {
                    for (; len > 0; len -= LastBufferOffset)
                    {
                        Buffers.Add(clear ? Pool.RentClean(BufferSize) : Pool.Rent(BufferSize));
                        LastBufferOffset = Math.Min(len, Buffers[^1].Length);
                    }
                }
                _Length = value;
                HasUpdates = true;
            }
        }

        /// <summary>
        /// Update the <see cref="BufferSequence"/> (<see cref="HasUpdates"/> signals an invalid <see cref="BufferSequence"/>)
        /// </summary>
        /// <returns>Sequence</returns>
        protected ReadOnlySequence<byte> UpdateBufferSequence()
        {
            if (_Length == 0)
            {
                BufferSequence = ReadOnlySequence<byte>.Empty;
                HasUpdates = false;
                return BufferSequence;
            }
            int len = Buffers.Count - 1;
            if (len == 0)
            {
                BufferSequence = new(Buffers[0].AsMemory(0, LastBufferOffset));
                HasUpdates = false;
                return BufferSequence;
            }
            MemorySequenceSegment<byte> first = new(Buffers[0]),
                last = first;
            for (int i = 1; i <= len; last = last.Append(i == len ? Buffers[i].AsMemory(0, LastBufferOffset) : Buffers[i]), i++) ;
            BufferSequence = new(first, startIndex: 0, last, endIndex: LastBufferOffset);
            HasUpdates = false;
            return BufferSequence;
        }

        /// <summary>
        /// Cast as new byte array
        /// </summary>
        /// <param name="ms"><see cref="MemoryPoolStream"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator byte[](in FreezableArrayPoolStream ms) => ms.ToArray();

        /// <summary>
        /// Cast as length
        /// </summary>
        /// <param name="ms"><see cref="MemoryPoolStream"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator long(in FreezableArrayPoolStream ms) => ms.Length;
    }
}

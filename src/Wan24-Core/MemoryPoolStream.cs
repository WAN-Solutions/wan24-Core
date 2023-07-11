using System.Buffers;
using System.Diagnostics;

namespace wan24.Core
{
    /// <summary>
    /// Memory pool stream (stores in arrays from an <see cref="ArrayPool{T}"/>)
    /// </summary>
    public class MemoryPoolStream : Stream
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
        /// An object for thread synchronization
        /// </summary>
        protected readonly object SyncObject = new();
        /// <summary>
        /// Buffers
        /// </summary>
        protected readonly List<byte[]> Buffers;
        /// <summary>
        /// Pool
        /// </summary>
        protected readonly ArrayPool<byte> Pool;
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
        /// Buffer site in bytes
        /// </summary>
        protected int _BufferSize = _DefaultBufferSize;
        /// <summary>
        /// Length in bytes
        /// </summary>
        protected long _Length = 0;
        /// <summary>
        /// Byte offset
        /// </summary>
        protected long _Position = 0;
        /// <summary>
        /// Is disposed?
        /// </summary>
        protected bool IsDisposed = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public MemoryPoolStream(ArrayPool<byte>? pool = null, int? bufferSize = null) : base()
        {
            Pool = pool ?? ArrayPool<byte>.Shared;
            if (bufferSize != null) BufferSize = bufferSize.Value;
            Buffers = new()
            {
                Pool.Rent(BufferSize)
            };
        }

        /// <summary>
        /// Default buffer size in bytes
        /// </summary>
        public static int DefaultBufferSize
        {
            get => _DefaultBufferSize;
            set
            {
                ArgumentValidationHelper.EnsureValidArgument(nameof(value), 1, int.MaxValue, value);
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
                this.EnsureValidArgument(nameof(value), 1, int.MaxValue, value);
                _BufferSize = value;
            }
        }

        /// <summary>
        /// Total buffer length in bytes
        /// </summary>
        public long BufferLength => Buffers.Sum(b => b.Length);

        /// <summary>
        /// Current number of buffers
        /// </summary>
        public int BufferCount => Buffers.Count;

        /// <summary>
        /// Clean returned buffers?
        /// </summary>
        public bool CleanReturned { get; set; }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Length => _Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => _Position;
            set
            {
                EnsureUndisposed();
                this.EnsureValidArgument(nameof(value), 0, _Length, value);
                _Position = value;
                // Find buffer index and byte offset
                if (value == 0)
                {
                    BufferIndex = 0;
                    BufferOffset = 0;
                }
                else
                {
                    long pos = 0,
                        add,
                        len;
                    for (BufferIndex = 0; ; BufferIndex++)
                    {
                        len = Buffers[BufferIndex].Length;
                        add = Math.Min(len, value - pos);
                        pos += add;
                        if (add == len && pos != value) continue;
                        BufferOffset = (int)add;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Get the written data as a new array
        /// </summary>
        /// <returns>Array</returns>
        public byte[] ToArray()
        {
            EnsureUndisposed();
            if (_Length == 0) return Array.Empty<byte>();
            byte[] res = new byte[_Length];
            long pos = _Position;
            try
            {
                Position = 0;
                Debug.Assert(Read(res) == res.Length);
            }
            finally
            {
                Position = pos;
            }
            return res;
        }

        /// <inheritdoc/>
        public override void Flush() => EnsureUndisposed();

        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            int len = buffer.Length;
            if (len == 0) return 0;
            int res = 0,
                red;
            byte[] data;
            while (res != len && _Position < _Length)
            {
                data = Buffers[BufferIndex];
                red = Math.Min((BufferIndex == Buffers.Count - 1 ? LastBufferOffset : data.Length) - BufferOffset, len - res);
                data.AsSpan(BufferOffset, red).CopyTo(buffer[res..]);
                _Position += red;
                BufferOffset += red;
                res += red;
                if (BufferOffset != data.Length) break;
                BufferIndex++;
                BufferOffset = 0;
            }
            return res;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            if (_Position == _Length) return -1;
            byte[] data = Buffers[BufferIndex];
            if (data.Length == BufferOffset)
            {
                BufferIndex++;
                BufferOffset = 0;
            }
            int res = data[BufferOffset];
            BufferOffset++;
            _Position++;
            if (data.Length == BufferOffset)
            {
                BufferIndex++;
                BufferOffset = 0;
            }
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
            return Position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _Position + offset,
                SeekOrigin.End => _Length + offset,
                _ => throw new ArgumentException($"Invalid origin {origin}", nameof(origin))
            };
        }

        /// <inheritdoc/>
        public override void SetLength(long value) => SetLength(value, clear: true);

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            int len = buffer.Length;
            if (len == 0) return;
            long newLen = _Position + len;
            if (newLen > _Length) SetLength(newLen, clear: false);
            int written = 0,
                chunk;
            byte[] data;
            while (written < len)
            {
                data = Buffers[BufferIndex];
                chunk = Math.Min(data.Length - BufferOffset, len - written);
                buffer.Slice(written, chunk).CopyTo(data.AsSpan(BufferOffset));
                _Position += chunk;
                BufferOffset += chunk;
                written += chunk;
                if (BufferOffset < data.Length) break;
                BufferIndex++;
                BufferOffset = 0;
            }
            Debug.Assert(written == len);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            _Position++;
            byte[] data = Buffers[BufferIndex];
            if (_Position > _Length) SetLength(_Position, clear: false);
            if (data.Length == BufferOffset)
            {
                BufferIndex++;
                BufferOffset = 0;
            }
            data[BufferOffset] = value;
            BufferOffset++;
            if (BufferIndex == Buffers.Count - 1)
            {
                if (BufferOffset > LastBufferOffset)
                {
                    LastBufferOffset++;
                    if (LastBufferOffset == data.Length)
                    {
                        Buffers.Add(Pool.Rent(BufferSize));
                        BufferIndex++;
                        BufferOffset = LastBufferOffset = 0;
                    }
                }
            }
            else if (BufferOffset == data.Length)
            {
                BufferIndex++;
                BufferOffset = 0;
            }
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
            lock (SyncObject)
            {
                if (IsDisposed) return;
                IsDisposed = true;
            }
            base.Close();
            foreach (byte[] buffer in Buffers)
            {
                if (CleanReturned) buffer.Clear();
                Pool.Return(buffer);
            }
            Buffers.Clear();
        }

        /// <inheritdoc/>
#pragma warning disable CA1816 // Suppress GC (will be supressed from the parent)
        public override async ValueTask DisposeAsync()
        {
            await Task.Yield();
            lock (SyncObject)
            {
                if (IsDisposed) return;
                IsDisposed = true;
            }
            await base.DisposeAsync().DynamicContext();
            foreach (byte[] buffer in Buffers)
            {
                if (CleanReturned) buffer.Clear();
                Pool.Return(buffer);
            }
            Buffers.Clear();
        }
#pragma warning restore CA1816 // Suppress GC (will be supressed from the parent)

        /// <summary>
        /// Ensure undisposed state
        /// </summary>
        protected void EnsureUndisposed()
        {
            if (!IsDisposed) return;
            throw new ObjectDisposedException(GetType().ToString());
        }

        /// <summary>
        /// Set a new length
        /// </summary>
        /// <param name="value">New length in bytes</param>
        /// <param name="clear">Clear new buffers?</param>
        protected void SetLength(long value, bool clear)
        {
            EnsureUndisposed();
            this.EnsureValidArgument(nameof(value), 0, long.MaxValue, value);
            if (value == _Length) return;
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
            }
            else if (value < _Length)
            {
                // Delete some data
                BufferIndex = -1;
                long pos = 0,
                    add,
                    len;
                for (BufferIndex = 0; ; BufferIndex++)
                {
                    len = Buffers[BufferIndex].Length;
                    add = Math.Min(len, value - pos);
                    pos += add;
                    if (add == len && pos != value) continue;
                    BufferOffset = (int)add;
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
                    Buffers.RemoveRange(BufferIndex + 1, (int)len - BufferIndex - 1);
                }
                _Length = value;
                if (_Position > value) _Position = value;
            }
            else
            {
                // Add buffers
                long len = value - _Length;
                byte[] data = Buffers[^1];
                len -= (int)Math.Min(data.Length - LastBufferOffset, len);
                if (clear) data.AsSpan(BufferOffset).Clear();
                if (len == 0)
                {
                    LastBufferOffset += (int)(value - _Length);
                    if (LastBufferOffset == data.Length)
                    {
                        Buffers.Add(clear ? Pool.RentClean(BufferSize) : Pool.Rent(BufferSize));
                        LastBufferOffset = 0;
                    }
                }
                else
                {
                    for (; len != 0; len -= LastBufferOffset)
                    {
                        data = clear ? Pool.RentClean(BufferSize) : Pool.Rent(BufferSize);
                        Buffers.Add(data);
                        LastBufferOffset = (int)Math.Min(len, data.Length);
                    }
                }
                _Length = value;
            }
        }

        /// <summary>
        /// Cast as new byte array
        /// </summary>
        /// <param name="ms"><see cref="MemoryPoolStream"/></param>
        public static implicit operator byte[](MemoryPoolStream ms) => ms.ToArray();

        /// <summary>
        /// Cast as length
        /// </summary>
        /// <param name="ms"><see cref="MemoryPoolStream"/></param>
        public static implicit operator long(MemoryPoolStream ms) => ms.Length;
    }
}

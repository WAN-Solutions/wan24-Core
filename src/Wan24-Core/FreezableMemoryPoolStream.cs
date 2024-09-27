﻿using System.Buffers;
using System.Runtime;
using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Freezable memory pool stream
    /// </summary>
    public class FreezableMemoryPoolStream : StreamBase, IStatusProvider
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
        protected readonly FreezableList<IMemoryOwner<byte>> Buffers;
        /// <summary>
        /// Pool
        /// </summary>
        protected readonly MemoryPool<byte> Pool;
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
        protected long _Length = 0;
        /// <summary>
        /// Byte offset
        /// </summary>
        protected long _Position = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public FreezableMemoryPoolStream(in MemoryPool<byte>? pool = null, in int? bufferSize = null) : base()
        {
            Pool = pool ?? MemoryPool<byte>.Shared;
            if (bufferSize.HasValue) BufferSize = bufferSize.Value;
            Buffers =
            [
                Pool.Rent(BufferSize)
            ];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Initial data (will be disposed; stream is writable; initial position will be <c>0</c>)</param>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="writable">If writable</param>
        public FreezableMemoryPoolStream(in IMemoryOwner<byte> data, in MemoryPool<byte>? pool = null, in int? bufferSize = null, in bool writable = true) : base()
        {
            _CanWrite = writable;
            Pool = pool ?? MemoryPool<byte>.Shared;
            if (bufferSize.HasValue) BufferSize = bufferSize.Value;
            Buffers =
            [
                data,
                writable
                    ?Pool.Rent(BufferSize)
                    :new MemoryOwner<byte>(Array.Empty<byte>())
            ];
            _Length = data.Memory.Length;
            if (!writable) Buffers.Freeze();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Initial data (will be copied, if writable; stream is writable; initial position will be <c>0</c>)</param>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="writable">If writable</param>
        public FreezableMemoryPoolStream(in byte[] data, in MemoryPool<byte>? pool = null, in int? bufferSize = null, in bool writable = true) : base()
        {
            _CanWrite = writable;
            Pool = pool ?? MemoryPool<byte>.Shared;
            if (bufferSize.HasValue) BufferSize = bufferSize.Value;
            if (writable)
            {
                Buffers =
                [
                    Pool.Rent(BufferSize)
                ];
                Write(data);
                Position = 0;
            }
            else
            {
                Buffers =
                [
                    new MemoryOwner<byte>(data),
                    new MemoryOwner<byte>(Array.Empty<byte>())
                ];
                _Length = data.Length;
                if (!writable) Buffers.Freeze();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Initial data (will be copied; stream is writable; initial position will be <c>0</c>)</param>
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
        /// <param name="data">Initial data (will be copied; stream is writable; initial position will be <c>0</c>)</param>
        /// <param name="pool">Pool</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public FreezableMemoryPoolStream(in ReadOnlyMemory<byte> data, in MemoryPool<byte>? pool = null, in int? bufferSize = null) : this(pool, bufferSize)
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
        public long BufferLength => Buffers.Sum(b => (long)b.Memory.Length);

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
                yield return new(__("Writable"), CanWrite, __("If writable"));
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
                _Position = value;
                // Find buffer index and byte offset
                if (value == 0)
                {
                    BufferIndex = 0;
                    BufferOffset = 0;
                }
                else
                {
                    BufferIndex = 0;
                    for (long pos = 0, add, len; ; BufferIndex++)
                    {
                        len = Buffers[BufferIndex].Memory.Length;
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
            byte[] res = new byte[_Length];
            long pos = _Position;
            try
            {
                Position = 0;
                ReadExactly(res);
                return res;
            }
            finally
            {
                Position = pos;
            }
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
            int len = buffer.Length;
            if (len == 0) return 0;
            int res = 0;
            Span<byte> data;
            for (int red, lastBuffer = Buffers.Count - 1, dataLen; res != len && _Position != _Length; BufferIndex++, BufferOffset = 0)
            {
                data = Buffers[BufferIndex].Memory.Span;
                dataLen = data.Length;
                red = Math.Min((BufferIndex == lastBuffer ? LastBufferOffset : dataLen) - BufferOffset, len - res);
                data.Slice(BufferOffset, red).CopyTo(buffer[res..]);
                _Position += red;
                BufferOffset += red;
                res += red;
                if (BufferIndex == lastBuffer || BufferOffset != dataLen) break;
            }
            UpdateBufferOffsetAfterReading();
            return res;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            if (_Position == _Length) return -1;
            int res = Buffers[BufferIndex].Memory.Span[BufferOffset];
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
        public override void SetLength(long value) => SetLength(value, clear: true);

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            int len = buffer.Length;
            if (len == 0) return;
            long newLen = _Position + len;
            if (newLen > _Length) SetLength(newLen, clear: false);
            Span<byte> data;
            for (int written = 0, chunk, dataLen; written != len; written += chunk, BufferIndex++, BufferOffset = 0)
            {
                data = Buffers[BufferIndex].Memory.Span;
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
            if (++_Position > _Length) SetLength(_Position, clear: false);
            Buffers[BufferIndex].Memory.Span[BufferOffset] = value;
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

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Unfreeze();
            foreach (IMemoryOwner<byte> buffer in Buffers)
            {
                if (CleanReturned) buffer.Memory.Span.Clean();
                buffer.Dispose();
            }
            Buffers.Clear();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            Unfreeze();
            foreach (IMemoryOwner<byte> buffer in Buffers)
            {
                if (CleanReturned) buffer.Memory.Span.Clean();
                buffer.Dispose();
            }
            Buffers.Clear();
            await base.DisposeCore().DynamicContext();
        }

        /// <summary>
        /// Update the buffer offset after reading
        /// </summary>
        protected void UpdateBufferOffsetAfterReading()
        {
            if (BufferIndex == Buffers.Count - 1 || BufferOffset != Buffers[BufferIndex].Memory.Length) return;
            BufferIndex++;
            BufferOffset = 0;
        }

        /// <summary>
        /// Update the buffer offset after writing
        /// </summary>
        protected void UpdateBufferOffsetAfterWriting()
        {
            if (BufferIndex != Buffers.Count - 1)
            {
                if (BufferOffset != Buffers[BufferIndex].Memory.Length) return;
                BufferIndex++;
                BufferOffset = 0;
                return;
            }
            if (BufferOffset < LastBufferOffset) return;
            LastBufferOffset = BufferOffset;
            if (LastBufferOffset != Buffers[BufferIndex].Memory.Length) return;
            Buffers.Add(Pool.Rent(BufferSize));
            BufferIndex++;
            BufferOffset = LastBufferOffset = 0;
        }

        /// <summary>
        /// Set a new length
        /// </summary>
        /// <param name="value">New length in bytes</param>
        /// <param name="clear">Clear new buffers?</param>
        protected void SetLength(in long value, in bool clear)
        {
            EnsureUndisposed();
            EnsureWritable();
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            if (value == _Length) return;
            if (value == 0)
            {
                // Delete all data
                for (int i = 1, len = Buffers.Count; i < len; i++)
                {
                    if (CleanReturned) Buffers[i].Memory.Span.Clean();
                    Buffers[i].Dispose();
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
                    len = Buffers[BufferIndex].Memory.Length;
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
                        if (CleanReturned) Buffers[i].Memory.Span.Clean();
                        Buffers[i].Dispose();
                    }
                    Buffers.RemoveRange(BufferIndex + 1, (int)len - BufferIndex - 1);
                }
                _Length = value;
                if (_Position > value) _Position = value;
                UpdateBufferOffsetAfterWriting();
            }
            else
            {
                // Add buffers
                long len = value - _Length;
                Memory<byte> data = Buffers[^1].Memory;
                len -= (int)Math.Min(data.Length - LastBufferOffset, len);
                if (clear) data.Span[LastBufferOffset..].Clean();
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
                    for (; len > 0; len -= LastBufferOffset)
                    {
                        Buffers.Add(clear ? Pool.RentClean(BufferSize) : Pool.Rent(BufferSize));
                        LastBufferOffset = (int)Math.Min(len, Buffers[^1].Memory.Length);
                    }
                }
                _Length = value;
                UpdateBufferOffsetAfterWriting();
            }
        }

        /// <summary>
        /// Cast as new byte array
        /// </summary>
        /// <param name="ms"><see cref="MemoryPoolStream"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator byte[](in FreezableMemoryPoolStream ms) => ms.ToArray();

        /// <summary>
        /// Cast as length
        /// </summary>
        /// <param name="ms"><see cref="MemoryPoolStream"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator long(in FreezableMemoryPoolStream ms) => ms.Length;
    }
}

﻿namespace wan24.Core
{
    /// <summary>
    /// Blocking buffer stream (reading blocks until (all, if aggressive) data is available, writing until the buffer was red completely; reading/writing is synchronized)
    /// </summary>
    public class BlockingBufferStream : StreamBase, IStatusProvider
    {
        /// <summary>
        /// Buffer
        /// </summary>
        protected readonly RentedArray<byte> Buffer;
        /// <summary>
        /// Thread synchronization for buffer access
        /// </summary>
        protected readonly SemaphoreSync BufferSync = new();
        /// <summary>
        /// Space event (raised when having space for writing)
        /// </summary>
        protected readonly ResetEvent SpaceEvent = new(initialState: true);
        /// <summary>
        /// Data event (raised when having data for reading)
        /// </summary>
        protected readonly ResetEvent DataEvent = new(initialState: false);
        /// <summary>
        /// Write byte offset
        /// </summary>
        protected int WriteOffset = 0;
        /// <summary>
        /// Read byte offset
        /// </summary>
        protected int ReadOffset = 0;
        /// <summary>
        /// Length in bytes
        /// </summary>
        protected long _Length = 0;
        /// <summary>
        /// Byte offset
        /// </summary>
        protected long _Position = 0;
        /// <summary>
        /// Is at the end of the file?
        /// </summary>
        protected bool _IsEndOfFile = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="clear">Clear the buffer when disposing?</param>
        public BlockingBufferStream(in int bufferSize, in bool clear = false) : base()
        {
            if (bufferSize < 1) throw new ArgumentOutOfRangeException(nameof(bufferSize));
            BufferSize = bufferSize;
            Buffer = new(bufferSize, clean: false)
            {
                Clear = clear
            };
        }

        /// <summary>
        /// Buffer size in bytes
        /// </summary>
        public int BufferSize { get; }

        /// <summary>
        /// Data in bytes available for reading unblocked
        /// </summary>
        public int Available => IfUndisposed(WriteOffset - ReadOffset);

        /// <summary>
        /// Space in bytes left in the buffer for writing unblocked
        /// </summary>
        public int SpaceLeft => IfUndisposed(BufferSize - WriteOffset);

        /// <summary>
        /// Block until the requested amount of data was red?
        /// </summary>
        public bool AggressiveReadBlocking { get; set; } = true;

        /// <summary>
        /// Is reading blocked?
        /// </summary>
        public bool IsReadBlocked => IfUndisposed(() => !DataEvent.IsSet);

        /// <summary>
        /// Is writing blocked?
        /// </summary>
        public bool IsWriteBlocked => IfUndisposed(() => !SpaceEvent.IsSet);

        /// <summary>
        /// Automatic reorganize the buffer?
        /// </summary>
        public bool AutoReorg { get; set; } = true;

        /// <summary>
        /// Is at the end of the file?
        /// </summary>
        public bool IsEndOfFile
        {
            get => IfUndisposed(_IsEndOfFile);
            set
            {
                EnsureUndisposed();
                if (_IsEndOfFile && !value) throw new InvalidOperationException();
                _IsEndOfFile = value;
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<Status> State
        {
            get
            {
                yield return new("Name", Name, "Name of the stream");
                yield return new("Type", GetType().ToString(), "Stream type");
                yield return new("Available", Available, "Available number of bytes for reading");
                yield return new("Space", SpaceLeft, "Space left in bytes for writing");
                yield return new("Reading blocks", IsReadBlocked, "Is reading blocked?");
                yield return new("Writing blocks", IsWriteBlocked, "Is writing blocked?");
            }
        }

        /// <inheritdoc/>
        public sealed override bool CanRead => true;

        /// <inheritdoc/>
        public sealed override bool CanSeek => false;

        /// <inheritdoc/>
        public sealed override bool CanWrite => true;

        /// <inheritdoc/>
        public sealed override long Length => IfUndisposed(_Length);

        /// <inheritdoc/>
        public sealed override long Position
        {
            get => IfUndisposed(_Position);
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Reorganize the buffer (may give more space for writing and unblock)
        /// </summary>
        /// <returns>If more space for writing is available now</returns>
        public bool ReorganizeBuffer()
        {
            EnsureUndisposed();
            bool hadSpace;
            using (SemaphoreSyncContext ssc = BufferSync.SyncContext())
            {
                EnsureUndisposed();
                hadSpace = !IsWriteBlocked;
                if (ReadOffset == 0) return false;
                Array.Copy(Buffer, ReadOffset, Buffer, 0, WriteOffset - ReadOffset);
                WriteOffset -= ReadOffset;
                ReadOffset = 0;
            }
            if (!hadSpace)
            {
                SpaceEvent.Set();
                RaiseOnSpaceAvailable();
            }
            return true;
        }

        /// <summary>
        /// Reorganize the buffer (may give more space for writing and unblock)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If more space for writing is available now</returns>
        public async Task<bool> ReorganizeBufferAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            bool hadSpace;
            using (SemaphoreSyncContext ssc = await BufferSync.SyncContextAsync(cancellationToken).DynamicContext())
            {
                EnsureUndisposed();
                hadSpace = !IsWriteBlocked;
                if (ReadOffset == 0) return false;
                Array.Copy(Buffer, ReadOffset, Buffer, 0, WriteOffset - ReadOffset);
                WriteOffset -= ReadOffset;
                ReadOffset = 0;
            }
            if (!hadSpace)
            {
                SpaceEvent.Set();
                RaiseOnSpaceAvailable();
            }
            return true;
        }

        /// <inheritdoc/>
        public sealed override void Flush() => EnsureUndisposed();

        /// <inheritdoc/>
        public override int ReadByte() => this.GenericReadByte();

        /// <inheritdoc/>
        public sealed override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            if (_IsEndOfFile && Available == 0) return 0;
            int res = 0;
            bool blocking = false;
            for (int read = 1; buffer.Length != 0 && read != 0 && EnsureUndisposed();)
            {
                DataEvent.Wait();
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = BufferSync.SyncContext();
                EnsureUndisposed();
                read = Math.Min(buffer.Length, Available);
                if (read == 0)
                {
                    if (!AggressiveReadBlocking || _IsEndOfFile)
                    {
                        blocking = true;
                        break;
                    }
                    DataEvent.Reset();
                    ssc.Dispose();
                    RaiseOnNeedData();
                    read = 1;
                    continue;
                }
                try
                {
                    Buffer.Span.Slice(ReadOffset, read).CopyTo(buffer);
                    buffer = buffer[read..];
                    res += read;
                    ReadOffset += read;
                    _Position += read;
                    ResetBuffer();
                }
                finally
                {
                    if (Available == 0)
                    {
                        blocking = true;
                        DataEvent.Reset();
                    }
                    else
                    {
                        blocking = false;
                    }
                }
            }
            if (blocking) RaiseOnNeedData();
            if (Available == 0 && AutoReorg) ReorganizeBuffer();
            return res;
        }

        /// <summary>
        /// Try reading as much data as available without blocking
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of bytes written into the buffer</returns>
        public virtual int TryRead(Span<byte> buffer)
        {
            EnsureUndisposed();
            if (_IsEndOfFile && Available == 0) return 0;
            int res = 0;
            bool blocking = false;
            for (int read = 1; buffer.Length != 0 && read != 0 && EnsureUndisposed();)
            {
                using SemaphoreSyncContext ssc = BufferSync.SyncContext();
                EnsureUndisposed();
                read = Math.Min(buffer.Length, Available);
                if (read == 0) break;
                try
                {
                    Buffer.Span.Slice(ReadOffset, read).CopyTo(buffer);
                    buffer = buffer[read..];
                    res += read;
                    ReadOffset += read;
                    _Position += read;
                    ResetBuffer();
                }
                finally
                {
                    if (Available == 0)
                    {
                        blocking = true;
                        DataEvent.Reset();
                    }
                    else
                    {
                        blocking = false;
                    }
                }
            }
            if (blocking) RaiseOnNeedData();
            if (Available == 0 && AutoReorg) ReorganizeBuffer();
            return res;
        }

        /// <inheritdoc/>
        public sealed override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (_IsEndOfFile && Available == 0) return 0;
            int res = 0;
            bool blocking = false;
            for (int read = 1; buffer.Length != 0 && read != 0 && EnsureUndisposed();)
            {
                await DataEvent.WaitAsync(cancellationToken).DynamicContext();
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = await BufferSync.SyncContextAsync(cancellationToken).DynamicContext();
                EnsureUndisposed();
                read = Math.Min(buffer.Length, Available);
                if (read == 0)
                {
                    if (!AggressiveReadBlocking || _IsEndOfFile)
                    {
                        blocking = true;
                        break;
                    }
                    DataEvent.Reset();
                    ssc.Dispose();
                    RaiseOnNeedData();
                    read = 1;
                    continue;
                }
                try
                {
                    Buffer.Span.Slice(ReadOffset, read).CopyTo(buffer.Span);
                    buffer = buffer[read..];
                    res += read;
                    ReadOffset += read;
                    _Position += read;
                    ResetBuffer();
                }
                finally
                {
                    if (Available == 0)
                    {
                        blocking = true;
                        DataEvent.Reset();
                    }
                    else
                    {
                        blocking = false;
                    }
                }
            }
            if (blocking) RaiseOnNeedData();
            if (Available == 0 && AutoReorg) await ReorganizeBufferAsync(cancellationToken).DynamicContext();
            return res;
        }

        /// <summary>
        /// Try reading as much data as available without blocking
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of bytes written into the buffer</returns>
        public virtual async ValueTask<int> TryReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (_IsEndOfFile && Available == 0) return 0;
            int res = 0;
            bool blocking = false;
            for (int read = 1; buffer.Length != 0 && read != 0 && EnsureUndisposed();)
            {
                using SemaphoreSyncContext ssc = await BufferSync.SyncContextAsync(cancellationToken).DynamicContext();
                EnsureUndisposed();
                read = Math.Min(buffer.Length, Available);
                if (read == 0) break;
                try
                {
                    Buffer.Span.Slice(ReadOffset, read).CopyTo(buffer.Span);
                    buffer = buffer[read..];
                    res += read;
                    ReadOffset += read;
                    _Position += read;
                    ResetBuffer();
                }
                finally
                {
                    if (Available == 0)
                    {
                        blocking = true;
                        DataEvent.Reset();
                    }
                    else
                    {
                        blocking = false;
                    }
                }
            }
            if (blocking) RaiseOnNeedData();
            if (Available == 0 && AutoReorg) await ReorganizeBufferAsync(cancellationToken).DynamicContext();
            return res;
        }

        /// <inheritdoc/>
        public sealed override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public sealed override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void WriteByte(byte value) => this.GenericWriteByte(value);

        /// <inheritdoc/>
        public sealed override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            if (_IsEndOfFile) throw new InvalidOperationException();
            bool blocking = false,
                haveData = false;
            for (int write; buffer.Length != 0 && EnsureUndisposed();)
            {
                SpaceEvent.Wait();
                EnsureUndisposed();
                using (SemaphoreSyncContext ssc = BufferSync.SyncContext())
                {
                    EnsureUndisposed();
                    write = Math.Min(SpaceLeft, buffer.Length);
                    buffer[..write].CopyTo(Buffer.Span[WriteOffset..]);
                    buffer = buffer[write..];
                    WriteOffset += write;
                    _Length += write;
                    if (SpaceLeft == 0)
                    {
                        blocking = true;
                        SpaceEvent.Reset();
                    }
                    haveData = !DataEvent.IsSet;
                    DataEvent.Set();
                }
                if (haveData)
                {
                    RaiseOnDataAvailable();
                    haveData = false;
                }
                if (blocking)
                {
                    RaiseOnNeedSpace();
                    blocking = false;
                }
                if (AutoReorg && SpaceLeft == 0) ReorganizeBuffer();
            }
        }

        /// <summary>
        /// Try writing a buffer without blocking
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of bytes written</returns>
        public virtual int TryWrite(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            if (_IsEndOfFile) throw new InvalidOperationException();
            int res = 0;
            bool blocking = false,
                haveData = false;
            for (int write; buffer.Length != 0 && EnsureUndisposed();)
            {
                using (SemaphoreSyncContext ssc = BufferSync.SyncContext())
                {
                    EnsureUndisposed();
                    write = Math.Min(SpaceLeft, buffer.Length);
                    if (write == 0) return res;
                    buffer[..write].CopyTo(Buffer.Span[WriteOffset..]);
                    buffer = buffer[write..];
                    WriteOffset += write;
                    _Length += write;
                    res += write;
                    if (SpaceLeft == 0)
                    {
                        blocking = true;
                        SpaceEvent.Reset();
                    }
                    haveData = !DataEvent.IsSet;
                    DataEvent.Set();
                }
                if (haveData)
                {
                    RaiseOnDataAvailable();
                    haveData = false;
                }
                if (blocking)
                {
                    RaiseOnNeedSpace();
                    blocking = false;
                }
                if (AutoReorg && SpaceLeft == 0) ReorganizeBuffer();
            }
            return res;
        }

        /// <inheritdoc/>
        public sealed override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (_IsEndOfFile) throw new InvalidOperationException();
            bool blocking = false,
                haveData = false;
            for (int write; buffer.Length != 0 && EnsureUndisposed();)
            {
                await SpaceEvent.WaitAsync(cancellationToken).DynamicContext();
                EnsureUndisposed();
                using (SemaphoreSyncContext ssc = await BufferSync.SyncContextAsync(cancellationToken).DynamicContext())
                {
                    EnsureUndisposed();
                    write = Math.Min(SpaceLeft, buffer.Length);
                    buffer.Span[..write].CopyTo(Buffer.Span[WriteOffset..]);
                    buffer = buffer[write..];
                    WriteOffset += write;
                    _Length += write;
                    if (SpaceLeft == 0)
                    {
                        blocking = true;
                        SpaceEvent.Reset();
                    }
                    haveData = !DataEvent.IsSet;
                    DataEvent.Set();
                }
                if (haveData)
                {
                    RaiseOnDataAvailable();
                    haveData = false;
                }
                if (blocking)
                {
                    RaiseOnNeedSpace();
                    blocking = false;
                }
                if (AutoReorg && SpaceLeft == 0) await ReorganizeBufferAsync(cancellationToken).DynamicContext();
            }
        }

        /// <summary>
        /// Try writing a buffer without blocking
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of bytes written</returns>
        public virtual async ValueTask<int> TryWriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (_IsEndOfFile) throw new InvalidOperationException();
            int res = 0;
            bool blocking = false,
                haveData = false;
            for (int write; buffer.Length != 0 && EnsureUndisposed();)
            {
                using (SemaphoreSyncContext ssc = await BufferSync.SyncContextAsync(cancellationToken).DynamicContext())
                {
                    EnsureUndisposed();
                    write = Math.Min(SpaceLeft, buffer.Length);
                    if (write == 0) return res;
                    buffer.Span[..write].CopyTo(Buffer.Span[WriteOffset..]);
                    buffer = buffer[write..];
                    WriteOffset += write;
                    _Length += write;
                    res += write;
                    if (SpaceLeft == 0)
                    {
                        blocking = true;
                        SpaceEvent.Reset();
                    }
                    haveData = !DataEvent.IsSet;
                    DataEvent.Set();
                }
                if (haveData)
                {
                    RaiseOnDataAvailable();
                    haveData = false;
                }
                if (blocking)
                {
                    RaiseOnNeedSpace();
                    blocking = false;
                }
                if (AutoReorg && SpaceLeft == 0) await ReorganizeBufferAsync(cancellationToken).DynamicContext();
            }
            return res;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            using SemaphoreSync bufferSync = BufferSync;
            using (SemaphoreSyncContext ssc = bufferSync.SyncContext())
            {
                base.Dispose(disposing);
                DataEvent.Dispose();
                SpaceEvent.Dispose();
            }
            Buffer.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            using SemaphoreSync bufferSync = BufferSync;
            using (SemaphoreSyncContext ssc = await bufferSync.SyncContextAsync().DynamicContext())
            {
                await base.DisposeCore().DynamicContext();
                await DataEvent.DisposeAsync().DynamicContext();
                await SpaceEvent.DisposeAsync().DynamicContext();
            }
            Buffer.Dispose();
        }

        /// <summary>
        /// Reset the buffer
        /// </summary>
        protected void ResetBuffer()
        {
            EnsureUndisposed();
            if (SpaceLeft != 0 || Available != 0) return;
            ReadOffset = 0;
            WriteOffset = 0;
            DataEvent.Reset();
            SpaceEvent.Set();
            RaiseOnSpaceAvailable();
        }

        /// <summary>
        /// Delegate for an event handler
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="e">Arguments</param>
        public delegate void Event_Delegate(BlockingBufferStream stream, EventArgs e);

        /// <summary>
        /// Raised when data is available for reading
        /// </summary>
        public event Event_Delegate? OnDataAvailable;
        /// <summary>
        /// Raise the <see cref="OnDataAvailable"/> event
        /// </summary>
        protected virtual void RaiseOnDataAvailable() => OnDataAvailable?.Invoke(this, new());

        /// <summary>
        /// Raised when reading is blocking
        /// </summary>
        public event Event_Delegate? OnNeedData;
        /// <summary>
        /// Raise the <see cref="OnNeedData"/> event
        /// </summary>
        protected virtual void RaiseOnNeedData() => OnNeedData?.Invoke(this, new());

        /// <summary>
        /// Raised when space for writing is available
        /// </summary>
        public event Event_Delegate? OnSpaceAvailable;
        /// <summary>
        /// Raise the <see cref="OnSpaceAvailable"/> event
        /// </summary>
        protected virtual void RaiseOnSpaceAvailable() => OnSpaceAvailable?.Invoke(this, new());

        /// <summary>
        /// Raised when writing is blocking
        /// </summary>
        public event Event_Delegate? OnNeedSpace;
        /// <summary>
        /// Raise the <see cref="OnNeedSpace"/> event
        /// </summary>
        protected virtual void RaiseOnNeedSpace() => OnNeedSpace?.Invoke(this, new());
    }
}

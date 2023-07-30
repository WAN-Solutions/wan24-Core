namespace wan24.Core
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
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSlim Sync = new(1, 1);
        /// <summary>
        /// Write synchronization
        /// </summary>
        protected readonly ResetEvent WriteSync = new(initialState: true);
        /// <summary>
        /// Read synchronization
        /// </summary>
        protected readonly ResetEvent ReadSync = new(initialState: false);
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
        /// Constructor
        /// </summary>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public BlockingBufferStream(int bufferSize) : base()
        {
            if (bufferSize < 1) throw new ArgumentOutOfRangeException(nameof(bufferSize));
            BufferSize = bufferSize;
            Buffer = new(bufferSize, clean: false);
        }

        /// <summary>
        /// Buffer size in bytes
        /// </summary>
        public int BufferSize { get; }

        /// <summary>
        /// Number of bytes available for reading unblocked
        /// </summary>
        public int Available => WriteOffset - ReadOffset;

        /// <summary>
        /// Number of bytes left in the buffer for writing unblocked
        /// </summary>
        public int SpaceLeft => BufferSize - WriteOffset;

        /// <summary>
        /// Block until the requested amount of data was red?
        /// </summary>
        public bool AggressiveReadBlocking { get; set; } = true;

        /// <summary>
        /// Is reading blocked?
        /// </summary>
        public bool IsReadBlocked => IfUndisposed(() => !ReadSync.IsSet);

        /// <summary>
        /// Is writing blocked?
        /// </summary>
        public bool IsWriteBlocked => IfUndisposed(() => !WriteSync.IsSet);

        /// <summary>
        /// Automatic reorganize the buffer during writing?
        /// </summary>
        public bool AutoReorg { get; set; } = true;

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
            Sync.Wait();
            EnsureUndisposed();
            bool readBlocked = IsReadBlocked,
                writeBlocked = IsWriteBlocked,
                hasSpace = writeBlocked;
            if (!readBlocked) ReadSync.Reset();
            if (!writeBlocked) WriteSync.Reset();
            try
            {
                if (ReadOffset == 0) return false;
                Array.Copy(Buffer, ReadOffset, Buffer, 0, WriteOffset - ReadOffset);
                WriteOffset -= ReadOffset;
                ReadOffset = 0;
                writeBlocked = false;
            }
            finally
            {
                if (!readBlocked) ReadSync.Set();
                if (!writeBlocked) WriteSync.Set();
                Sync.Release();
            }
            if (hasSpace) RaiseOnSpaceAvailable();
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
            await Sync.WaitAsync(cancellationToken).DynamicContext();
            EnsureUndisposed();
            bool readBlocked = IsReadBlocked,
                writeBlocked = IsWriteBlocked,
                hasSpace = writeBlocked;
            if (!readBlocked) ReadSync.Reset();
            if (!writeBlocked) WriteSync.Reset();
            try
            {
                if (ReadOffset == 0) return false;
                Array.Copy(Buffer, ReadOffset, Buffer, 0, WriteOffset - ReadOffset);
                WriteOffset -= ReadOffset;
                ReadOffset = 0;
                writeBlocked = false;
            }
            finally
            {
                if (!readBlocked) ReadSync.Set();
                if (!writeBlocked) WriteSync.Set();
                Sync.Release();
            }
            if (hasSpace) RaiseOnSpaceAvailable();
            return true;
        }

        /// <inheritdoc/>
        public sealed override void Flush() => EnsureUndisposed();

        /// <inheritdoc/>
        public sealed override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            int res = 0;
            bool blocking = false;
            for (int read = 1; buffer.Length != 0 && read != 0 && EnsureUndisposed();)
            {
                Sync.Wait();
                EnsureUndisposed();
                try
                {
                    read = Math.Min(buffer.Length, Available);
                    if (read == 0)
                    {
                        if (!AggressiveReadBlocking)
                        {
                            blocking = true;
                            break;
                        }
                        ReadSync.Reset();
                    }
                }
                finally
                {
                    Sync.Release();
                }
                if (read == 0)
                {
                    RaiseOnNeedData();
                    ReadSync.Wait();
                    EnsureUndisposed();
                    read = 1;
                    continue;
                }
                ReadSync.Wait();
                EnsureUndisposed();
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
                        ReadSync.Reset();
                    }
                    else
                    {
                        blocking = false;
                    }
                }
            }
            if (blocking) RaiseOnNeedData();
            return res;
        }

        /// <inheritdoc/>
        public sealed override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            int res = 0;
            bool blocking = false;
            for (int read = 1; buffer.Length != 0 && read != 0 && EnsureUndisposed();)
            {
                await Sync.WaitAsync(cancellationToken);
                EnsureUndisposed();
                try
                {
                    read = Math.Min(buffer.Length, Available);
                    if (read == 0)
                    {
                        if (!AggressiveReadBlocking)
                        {
                            blocking = true;
                            break;
                        }
                        ReadSync.Reset();
                    }
                }
                finally
                {
                    Sync.Release();
                }
                if (read == 0)
                {
                    RaiseOnNeedData();
                    await ReadSync.WaitAsync(cancellationToken).DynamicContext();
                    EnsureUndisposed();
                    read = 1;
                    continue;
                }
                await ReadSync.WaitAsync(cancellationToken).DynamicContext();
                EnsureUndisposed();
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
                        ReadSync.Reset();
                    }
                    else
                    {
                        blocking = false;
                    }
                }
            }
            if (blocking) RaiseOnNeedData();
            return res;
        }

        /// <inheritdoc/>
        public sealed override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public sealed override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public sealed override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            bool blocking = false,
                haveData = false;
            for (int write; buffer.Length != 0 && EnsureUndisposed();)
            {
                WriteSync.Wait();
                EnsureUndisposed();
                write = Math.Min(SpaceLeft, buffer.Length);
                buffer[..write].CopyTo(Buffer.Span[WriteOffset..]);
                buffer = buffer[write..];
                Sync.Wait();
                EnsureUndisposed();
                try
                {
                    WriteOffset += write;
                    _Length += write;
                    if (SpaceLeft == 0)
                    {
                        blocking = true;
                        WriteSync.Reset();
                    }
                    haveData = !ReadSync.IsSet;
                    ReadSync.Set();
                }
                finally
                {
                    Sync.Release();
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

        /// <inheritdoc/>
        public sealed override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            bool blocking = false,
                haveData = false;
            for (int write; buffer.Length != 0 && EnsureUndisposed();)
            {
                await WriteSync.WaitAsync(cancellationToken).DynamicContext();
                EnsureUndisposed();
                write = Math.Min(SpaceLeft, buffer.Length);
                buffer.Span[..write].CopyTo(Buffer.Span[WriteOffset..]);
                buffer = buffer[write..];
                await Sync.WaitAsync(cancellationToken).DynamicContext();
                EnsureUndisposed();
                try
                {
                    WriteOffset += write;
                    _Length += write;
                    if (SpaceLeft == 0)
                    {
                        blocking = true;
                        WriteSync.Reset();
                    }
                    ReadSync.Set();
                }
                finally
                {
                    Sync.Release();
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

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposing) return;
            Sync.Wait();
            try
            {
                base.Dispose(disposing);
                ReadSync.Dispose();
                WriteSync.Dispose();
            }
            finally
            {
                Sync.Dispose();
            }
            Buffer.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await Sync.WaitAsync().DynamicContext();
            try
            {
                await base.DisposeCore().DynamicContext();
                await ReadSync.DisposeAsync().DynamicContext();
                await WriteSync.DisposeAsync().DynamicContext();
            }
            finally
            {
                Sync.Release();
                Sync.Dispose();
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
            ReadSync.Reset();
            WriteSync.Set();
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

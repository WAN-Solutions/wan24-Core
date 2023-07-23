//TODO Write tests

namespace wan24.Core
{
    /// <summary>
    /// Synchronized stream (synchronizes reading/writing/seeking operations; the base stream should implement each single asynchronous reading/writing method! Any asynchronous 
    /// reading/writing method which adopts or calls to another asynchronous reading/writing or any seeking method will cause a dead-lock!)
    /// </summary>
    public class SynchronizedStream : SynchronizedStream<Stream>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public SynchronizedStream(Stream baseStream, bool leaveOpen = false) : base(baseStream, leaveOpen) { }
    }

    /// <summary>
    /// Synchronized stream (synchronizes reading/writing/seeking operations; the base stream should implement each single asynchronous reading/writing method! Any asynchronous 
    /// reading/writing method which adopts or calls to another asynchronous reading/writing or any seeking method will cause a dead-lock!)
    /// </summary>
    /// <typeparam name="T">Wrapped stream type</typeparam>
    public class SynchronizedStream<T> : WrapperStream<T> where T:Stream
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public SynchronizedStream(T baseStream, bool leaveOpen = false) : base(baseStream, leaveOpen) { }

        /// <summary>
        /// IO synchronization
        /// </summary>
        public SemaphoreSlim SyncIO { get; } = new(1, 1);

        /// <summary>
        /// Seeking synchronization
        /// </summary>
        public SemaphoreSlim SyncSeek { get; } = new(1, 1);

        /// <inheritdoc/>
        public override long Position
        {
            get => base.Position;
            set
            {
                EnsureUndisposed();
                SyncIO.Wait();
                SyncSeek.Wait();
                try
                {
                    base.Position = value;
                }
                finally
                {
                    SyncSeek.Release();
                    SyncIO.Release();
                }
            }
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            SyncIO.Wait();
            try
            {
                base.SetLength(value);
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            SyncIO.Wait();
            SyncSeek.Wait();
            try
            {
                return base.Position = origin switch
                {
                    SeekOrigin.Begin => offset,
                    SeekOrigin.Current => base.Position + offset,
                    SeekOrigin.End => base.Length + offset,
                    _ => throw new ArgumentException("Invalid seek origin", nameof(origin))
                };
            }
            finally
            {
                SyncSeek.Release();
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            SyncIO.Wait();
            try
            {
                return base.Read(buffer, offset, count);
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            SyncIO.Wait();
            try
            {
                return base.Read(buffer);
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            await SyncIO.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                return await base.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await SyncIO.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                return await base.ReadAsync(buffer, cancellationToken).DynamicContext();
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            SyncIO.Wait();
            try
            {
                return base.ReadByte();
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            SyncIO.Wait();
            try
            {
                base.Write(buffer, offset, count);
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            SyncIO.Wait();
            try
            {
                base.Write(buffer);
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            await SyncIO.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                await base.WriteAsync(buffer, offset, count, cancellationToken).DynamicContext();
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await SyncIO.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                await base.WriteAsync(buffer, cancellationToken).DynamicContext();
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            SyncIO.Wait();
            try
            {
                base.WriteByte(value);
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public override void Close()
        {
            if (IsClosed) return;
            SyncIO.Wait();
            SyncSeek.Wait();
            try
            {
                base.Close();
            }
            finally
            {
                SyncSeek.Release();
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            SyncIO.Wait();
            SyncSeek.Wait();
            try
            {
                base.Dispose(disposing);
            }
            finally
            {
                SyncSeek.Dispose();
                SyncIO.Dispose();
            }
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await SyncIO.WaitAsync().DynamicContext();
            await SyncSeek.WaitAsync().DynamicContext();
            try
            {
                await base.DisposeCore().DynamicContext();
            }
            finally
            {
                SyncSeek.Dispose();
                SyncIO.Dispose();
            }
        }
    }
}

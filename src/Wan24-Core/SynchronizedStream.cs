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
        /// I/O synchronization
        /// </summary>
        public SemaphoreSlim SyncIO { get; } = new(1, 1);

        /// <inheritdoc/>
        public sealed override long Position
        {
            get => base.Position;
            set
            {
                EnsureUndisposed();
                SyncIO.Wait();
                try
                {
                    base.Position = value;
                }
                finally
                {
                    SyncIO.Release();
                }
            }
        }

        /// <inheritdoc/>
        public sealed override void SetLength(long value)
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
        public sealed override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            SyncIO.Wait();
            try
            {
                return BaseStream.GenericSeek(offset, origin);
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public sealed override void Flush()
        {
            EnsureUndisposed();
            SyncIO.Wait();
            try
            {
                base.Flush();
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public sealed override async Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            await SyncIO.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                await base.FlushAsync(cancellationToken).DynamicContext();
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public sealed override int Read(byte[] buffer, int offset, int count)
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
        public sealed override int Read(Span<byte> buffer)
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
        public sealed override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
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
        public sealed override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
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
        public sealed override int ReadByte()
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
        public sealed override void Write(byte[] buffer, int offset, int count)
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
        public sealed override void Write(ReadOnlySpan<byte> buffer)
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
        public sealed override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
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
        public sealed override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
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
        public sealed override void WriteByte(byte value)
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
        public sealed override void CopyTo(Stream destination, int bufferSize)
        {
            EnsureUndisposed();
            SyncIO.Wait();
            try
            {
                BaseStream.CopyTo(destination, bufferSize);
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public sealed override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            await SyncIO.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                await BaseStream.CopyToAsync(destination, bufferSize, cancellationToken).DynamicContext();
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public sealed override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            EnsureUndisposed();
            SyncIO.Wait();
            try
            {
                return BaseStream.BeginRead(buffer, offset, count, callback, state);
            }
            catch
            {
                SyncIO.Release();
                throw;
            }
        }

        /// <inheritdoc/>
        public sealed override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            EnsureUndisposed();
            SyncIO.Wait();
            try
            {
                return BaseStream.BeginWrite(buffer, offset, count, callback, state);
            }
            catch
            {
                SyncIO.Release();
                throw;
            }
        }

        /// <inheritdoc/>
        public sealed override int EndRead(IAsyncResult asyncResult)
        {
            try
            {
                return BaseStream.EndRead(asyncResult);
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public sealed override void EndWrite(IAsyncResult asyncResult)
        {
            try
            {
                BaseStream.EndWrite(asyncResult);
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public sealed override void Close()
        {
            if (IsClosed) return;
            SyncIO.Wait();
            try
            {
                base.Close();
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        protected sealed override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            SyncIO.Wait();
            try
            {
                base.Dispose(disposing);
            }
            finally
            {
                SyncIO.Dispose();
            }
        }

        /// <inheritdoc/>
        protected sealed override async Task DisposeCore()
        {
            await SyncIO.WaitAsync().DynamicContext();
            try
            {
                await base.DisposeCore().DynamicContext();
            }
            finally
            {
                SyncIO.Dispose();
            }
        }
    }
}

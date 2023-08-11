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
    public class SynchronizedStream<T> : WrapperStream<T> where T : Stream
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
        public SemaphoreSync SyncIO { get; } = new();

        /// <inheritdoc/>
        public sealed override long Position
        {
            get => base.Position;
            set
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = SyncIO.Sync();
                base.Position = value;
            }
        }

        /// <inheritdoc/>
        public sealed override void SetLength(long value)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO.Sync();
            BaseStream.SetLength(value);
        }

        /// <inheritdoc/>
        public sealed override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO.Sync();
            return BaseStream.GenericSeek(offset, origin);
        }

        /// <inheritdoc/>
        public sealed override void Flush()
        {
            EnsureUndisposed(allowDisposing: true);
            using SemaphoreSyncContext ssc = SyncIO.Sync();
            BaseStream.Flush();
        }

        /// <inheritdoc/>
        public sealed override async Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed(allowDisposing: true);
            using SemaphoreSyncContext ssc = await SyncIO.SyncAsync(cancellationToken).DynamicContext();
            await BaseStream.FlushAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public sealed override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO.Sync();
            return BaseStream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public sealed override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO.Sync();
            return BaseStream.Read(buffer);
        }
        /// <summary>
        /// Read from a byte offset
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of bytes red</returns>
        public int ReadAt(long offset, Span<byte> buffer)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO.Sync();
            BaseStream.Position = offset;
            return BaseStream.Read(buffer);
        }

        /// <inheritdoc/>
        public sealed override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncAsync(cancellationToken).DynamicContext();
            return await BaseStream.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public sealed override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncAsync(cancellationToken).DynamicContext();
            return await BaseStream.ReadAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Read from a byte offset
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of bytes red</returns>
        public async Task<int> ReadAtAsync(long offset, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncAsync(cancellationToken).DynamicContext();
            base.Position = offset;
            return await BaseStream.ReadAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public sealed override int ReadByte()
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO.Sync();
            return BaseStream.ReadByte();
        }

        /// <summary>
        /// Read a byte from a byte offset
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <returns>Byte or <c>-1</c>, if reading failed</returns>
        public int ReadByteAt(long offset)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO.Sync();
            base.Position = offset;
            return BaseStream.ReadByte();
        }

        /// <inheritdoc/>
        public sealed override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO.Sync();
            BaseStream.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public sealed override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO.Sync();
            BaseStream.Write(buffer);
        }

        /// <summary>
        /// Write at a byte offset
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <param name="buffer">Buffer</param>
        public void WriteAt(long offset, ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO.Sync();
            base.Position = offset;
            BaseStream.Write(buffer);
        }

        /// <inheritdoc/>
        public sealed override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncAsync(cancellationToken).DynamicContext();
            await BaseStream.WriteAsync(buffer, offset, count, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public sealed override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncAsync(cancellationToken).DynamicContext();
            await BaseStream.WriteAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write at a byte offset
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task WriteAtAsync(long offset, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncAsync(cancellationToken).DynamicContext();
            base.Position = offset;
            await BaseStream.WriteAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public sealed override void WriteByte(byte value)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO.Sync();
            BaseStream.WriteByte(value);
        }

        /// <summary>
        /// Write a byte at a byte offset
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <param name="value">Value</param>
        public void WriteByteAt(long offset, byte value)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO.Sync();
            base.Position = offset;
            BaseStream.WriteByte(value);
        }

        /// <inheritdoc/>
        public sealed override void CopyTo(Stream destination, int bufferSize)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO.Sync();
            BaseStream.CopyTo(destination, bufferSize);
        }

        /// <inheritdoc/>
        public sealed override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncAsync(cancellationToken).DynamicContext();
            await BaseStream.CopyToAsync(destination, bufferSize, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public sealed override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            EnsureUndisposed();
            SyncIO.Semaphore.Wait();
            try
            {
                return BaseStream.BeginRead(buffer, offset, count, callback, state);
            }
            catch
            {
                SyncIO.Semaphore.Release();
                throw;
            }
        }

        /// <inheritdoc/>
        public sealed override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            EnsureUndisposed();
            SyncIO.Semaphore.Wait();
            try
            {
                return BaseStream.BeginWrite(buffer, offset, count, callback, state);
            }
            catch
            {
                SyncIO.Semaphore.Release();
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
                SyncIO.Semaphore.Release();
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
                SyncIO.Semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public sealed override void Close()
        {
            if (IsClosed) return;
            try
            {
                using SemaphoreSyncContext ssc = SyncIO.Sync();
                base.Close();
            }
            catch
            {
            }
        }

        /// <inheritdoc/>
        protected sealed override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            SyncIO.Dispose();
        }

        /// <inheritdoc/>
        protected sealed override Task DisposeCore()
        {
            SyncIO.Dispose();
            return Task.CompletedTask;
        }
    }
}

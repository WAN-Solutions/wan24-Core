namespace wan24.Core
{
    /// <summary>
    /// Synchronized stream (synchronizes reading/writing/seeking operations; the base stream should implement each single asynchronous reading/writing method! Any asynchronous 
    /// reading/writing method which adopts or calls to another asynchronous reading/writing or any seeking method will cause a dead-lock!)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class SynchronizedStream(in Stream baseStream, in bool leaveOpen = false) : SynchronizedStream<Stream>(baseStream, leaveOpen)
    {
    }

    /// <summary>
    /// Synchronized stream (synchronizes reading/writing/seeking operations; the base stream should implement each single asynchronous reading/writing method! Any asynchronous 
    /// reading/writing method which adopts or calls to another asynchronous reading/writing or any seeking method will cause a dead-lock!)
    /// </summary>
    /// <typeparam name="T">Wrapped stream type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class SynchronizedStream<T>(in T baseStream, in bool leaveOpen = false) : WrapperStream<T>(baseStream, leaveOpen) where T : Stream
    {

        /// <summary>
        /// I/O synchronization
        /// </summary>
        public SemaphoreSync SyncIO { get; } = new();

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                EnsureUndisposed();
                EnsureSeekable();
                return base.Position;
            }
            set
            {
                EnsureUndisposed();
                EnsureSeekable();
                using SemaphoreSyncContext ssc = SyncIO.SyncContext();
                base.Position = value;
            }
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            EnsureWritable();
            using SemaphoreSyncContext ssc = SyncIO.SyncContext();
            BaseStream.SetLength(value);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            EnsureSeekable();
            using SemaphoreSyncContext ssc = SyncIO.SyncContext();
            return BaseStream.GenericSeek(offset, origin);
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            EnsureUndisposed(allowDisposing: true);
            using SemaphoreSyncContext ssc = SyncIO.SyncContext();
            BaseStream.Flush();
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed(allowDisposing: true);
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            await BaseStream.FlushAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureReadable();
            using SemaphoreSyncContext ssc = SyncIO.SyncContext();
            return BaseStream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            using SemaphoreSyncContext ssc = SyncIO.SyncContext();
            return BaseStream.Read(buffer);
        }

        /// <summary>
        /// Read from a byte offset
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of bytes red</returns>
        public virtual int ReadAt(in long offset, in Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            EnsureSeekable();
            using SemaphoreSyncContext ssc = SyncIO.SyncContext();
            BaseStream.Position = offset;
            return BaseStream.Read(buffer);
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            return await BaseStream.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            return await BaseStream.ReadAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Read from a byte offset
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of bytes red</returns>
        public virtual async Task<int> ReadAtAsync(long offset, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            EnsureSeekable();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            base.Position = offset;
            return await BaseStream.ReadAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            EnsureReadable();
            using SemaphoreSyncContext ssc = SyncIO.SyncContext();
            return BaseStream.ReadByte();
        }

        /// <summary>
        /// Read a byte from a byte offset
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <returns>Byte or <c>-1</c>, if reading failed</returns>
        public virtual int ReadByteAt(in long offset)
        {
            EnsureUndisposed();
            EnsureReadable();
            EnsureSeekable();
            using SemaphoreSyncContext ssc = SyncIO.SyncContext();
            base.Position = offset;
            return BaseStream.ReadByte();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureWritable();
            using SemaphoreSyncContext ssc = SyncIO.SyncContext();
            BaseStream.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            using SemaphoreSyncContext ssc = SyncIO.SyncContext();
            BaseStream.Write(buffer);
        }

        /// <summary>
        /// Write at a byte offset
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <param name="buffer">Buffer</param>
        public virtual void WriteAt(in long offset, in ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            EnsureSeekable();
            using SemaphoreSyncContext ssc = SyncIO.SyncContext();
            base.Position = offset;
            BaseStream.Write(buffer);
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureWritable();
            EnsureSeekable();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            await BaseStream.WriteAsync(buffer, offset, count, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            await BaseStream.WriteAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write at a byte offset
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task WriteAtAsync(long offset, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            EnsureSeekable();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            base.Position = offset;
            await BaseStream.WriteAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            EnsureWritable();
            using SemaphoreSyncContext ssc = SyncIO.SyncContext();
            BaseStream.WriteByte(value);
        }

        /// <summary>
        /// Write a byte at a byte offset
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <param name="value">Value</param>
        public virtual void WriteByteAt(in long offset, in byte value)
        {
            EnsureUndisposed();
            EnsureWritable();
            EnsureSeekable();
            using SemaphoreSyncContext ssc = SyncIO.SyncContext();
            base.Position = offset;
            BaseStream.WriteByte(value);
        }

        /// <inheritdoc/>
        public override void CopyTo(Stream destination, int bufferSize)
        {
            EnsureUndisposed();
            EnsureReadable();
            using SemaphoreSyncContext ssc = SyncIO.SyncContext();
            BaseStream.CopyTo(destination, bufferSize);
        }

        /// <inheritdoc/>
        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            await BaseStream.CopyToAsync(destination, bufferSize, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            EnsureUndisposed();
            EnsureReadable();
            SyncIO.Sync();
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
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            EnsureUndisposed();
            EnsureWritable();
            SyncIO.Sync();
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
        public override int EndRead(IAsyncResult asyncResult)
        {
            try
            {
                EnsureReadable();
                return BaseStream.EndRead(asyncResult);
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            try
            {
                EnsureWritable();
                BaseStream.EndWrite(asyncResult);
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
            try
            {
                using SemaphoreSyncContext ssc = SyncIO.SyncContext();
                base.Close();
            }
            catch
            {
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            SyncIO.Dispose();
        }

        /// <inheritdoc/>
        protected override Task DisposeCore()
        {
            SyncIO.Dispose();
            return Task.CompletedTask;
        }
    }
}

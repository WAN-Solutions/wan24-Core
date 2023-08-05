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
                BaseStream.SetLength(value);
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
            EnsureUndisposed(allowDisposing: true);
            SyncIO.Wait();
            try
            {
                BaseStream.Flush();
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <inheritdoc/>
        public sealed override async Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed(allowDisposing: true);
            await SyncIO.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                await BaseStream.FlushAsync(cancellationToken).DynamicContext();
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
                return BaseStream.Read(buffer, offset, count);
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
                return BaseStream.Read(buffer);
            }
            finally
            {
                SyncIO.Release();
            }
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
            SyncIO.Wait();
            try
            {
                BaseStream.Position = offset;
                return BaseStream.Read(buffer);
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
                return await BaseStream.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
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
                return await BaseStream.ReadAsync(buffer, cancellationToken).DynamicContext();
            }
            finally
            {
                SyncIO.Release();
            }
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
            await SyncIO.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                base.Position = offset;
                return await BaseStream.ReadAsync(buffer, cancellationToken).DynamicContext();
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
                return BaseStream.ReadByte();
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <summary>
        /// Read a byte from a byte offset
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <returns>Byte or <c>-1</c>, if reading failed</returns>
        public int ReadByteAt(long offset)
        {
            EnsureUndisposed();
            SyncIO.Wait();
            try
            {
                base.Position = offset;
                return BaseStream.ReadByte();
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
                BaseStream.Write(buffer, offset, count);
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
                BaseStream.Write(buffer);
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <summary>
        /// Write at a byte offset
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <param name="buffer">Buffer</param>
        public void WriteAt(long offset, ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            SyncIO.Wait();
            try
            {
                base.Position = offset;
                BaseStream.Write(buffer);
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
                await BaseStream.WriteAsync(buffer, offset, count, cancellationToken).DynamicContext();
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
                await BaseStream.WriteAsync(buffer, cancellationToken).DynamicContext();
            }
            finally
            {
                SyncIO.Release();
            }
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
            await SyncIO.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                base.Position = offset;
                await BaseStream.WriteAsync(buffer, cancellationToken).DynamicContext();
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
                BaseStream.WriteByte(value);
            }
            finally
            {
                SyncIO.Release();
            }
        }

        /// <summary>
        /// Write a byte at a byte offset
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <param name="value">Value</param>
        public void WriteByteAt(long offset, byte value)
        {
            EnsureUndisposed();
            SyncIO.Wait();
            try
            {
                base.Position = offset;
                BaseStream.WriteByte(value);
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
            try
            {
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
            catch
            {
            }
            finally
            {
                try
                {
                    SyncIO.Release();
                }
                catch
                {
                }
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

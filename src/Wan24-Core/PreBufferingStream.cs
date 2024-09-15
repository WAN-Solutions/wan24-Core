namespace wan24.Core
{
    /// <summary>
    /// Pre-buffering stream (read-only)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="bufferSize">Buffer size in bytes</param>
    /// <param name="clearBuffer">If to clear unused buffers</param>
    /// <param name="leaveOpen">If to leave the base stream open when disposing</param>
    public class PreBufferingStream(in Stream baseStream, in int? bufferSize = null, in bool clearBuffer = false, in bool leaveOpen = false)
        : PreBufferingStream<Stream>(baseStream, bufferSize, clearBuffer, leaveOpen)
    {
    }

    /// <summary>
    /// Pre-buffering stream (read-only)
    /// </summary>
    /// <typeparam name="T">Buffered stream type</typeparam>
    public class PreBufferingStream<T> : WrapperStream<T> where T : Stream
    {
        /// <summary>
        /// Buffer
        /// </summary>
        protected readonly BlockingBufferStream Buffer;
        /// <summary>
        /// Buffer cancellation
        /// </summary>
        protected readonly CancellationTokenSource BufferCancellation = new();
        /// <summary>
        /// Buffer task
        /// </summary>
        protected readonly Task BufferTask;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="clearBuffer">If to clear unused buffers</param>
        /// <param name="leaveOpen">If to leave the base stream open when disposing</param>
        public PreBufferingStream(in T baseStream, in int? bufferSize = null, in bool clearBuffer = false, in bool leaveOpen = false)
            : base(baseStream, leaveOpen)
        {
            Buffer = new(bufferSize ?? Settings.BufferSize, clearBuffer);
            BufferTask = ((Func<Task>)BufferWorker).StartLongRunningTask();
        }

        /// <summary>
        /// Last exception of the buffer background worker task
        /// </summary>
        public Exception? LastException { get; protected set; }

        /// <summary>
        /// Buffer size in bytes
        /// </summary>
        public int BufferSize => Buffer.BufferSize;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            return Buffer.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            return Buffer.Read(buffer);
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            return await Buffer.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            return await Buffer.ReadAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            return Buffer.ReadByte();
        }

        /// <inheritdoc/>
        public override void CopyTo(Stream destination, int bufferSize)
        {
            EnsureUndisposed();
            Buffer.CopyTo(destination, bufferSize);
        }

        /// <inheritdoc/>
        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            await Buffer.CopyToAsync(destination, bufferSize, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            EnsureUndisposed();
            return Buffer.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdoc/>
        public override int EndRead(IAsyncResult asyncResult) => Buffer.EndRead(asyncResult);

        /// <summary>
        /// Buffer worker
        /// </summary>
        protected virtual async Task BufferWorker()
        {
            await Task.Yield();
            try
            {
                await BaseStream.CopyToAsync(Buffer, Buffer.BufferSize, BufferCancellation.Token).DynamicContext();
            }
            catch(ObjectDisposedException) when(Buffer.IsDisposing)
            {
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken.IsEqualTo(BufferCancellation.Token))
            {
            }
            catch (Exception ex)
            {
                ErrorHandling.Handle(new("Pre-buffering stream background buffer task failed", ex));
                LastException = ex;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            BufferCancellation.Cancel();
            base.Dispose(disposing);
            BufferCancellation.Dispose();
            Buffer.Dispose();
            BufferTask.GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            BufferCancellation.Cancel();
            await base.DisposeCore().DynamicContext();
            BufferCancellation.Dispose();
            await Buffer.DisposeAsync().DynamicContext();
            await BufferTask.DynamicContext();
        }
    }
}

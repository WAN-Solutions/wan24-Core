namespace wan24.Core
{
    /// <summary>
    /// Pre-buffering stream (read-only)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="bufferedStream">Buffered stream</param>
    /// <param name="bufferSize">Buffer size in bytes</param>
    /// <param name="clearBuffer">If to clear unused buffers</param>
    /// <param name="leaveOpen">If to leave the base stream open when disposing</param>
    public class PreBufferingStream(in Stream bufferedStream, in int? bufferSize = null, in bool clearBuffer = false, in bool leaveOpen = false)
        : PreBufferingStream<Stream>(bufferedStream, bufferSize, clearBuffer, leaveOpen)
    {
    }

    /// <summary>
    /// Pre-buffering stream (read-only)
    /// </summary>
    /// <typeparam name="T">Buffered stream type</typeparam>
    public class PreBufferingStream<T> : BlockingBufferStream where T : Stream
    {
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
        /// <param name="bufferedStream">Buffered stream</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="clearBuffer">If to clear unused buffers</param>
        /// <param name="leaveOpen">If to leave the base stream open when disposing</param>
        public PreBufferingStream(in T bufferedStream, in int? bufferSize = null, in bool clearBuffer = false, in bool leaveOpen = false)
            : base(bufferSize ?? Settings.BufferSize, clearBuffer)
        {
            BufferedStream = bufferedStream;
            LeaveOpen = leaveOpen;
            BufferTask = ((Func<Task>)BufferWorker).StartLongRunningTask();
        }

        /// <summary>
        /// Buffered stream
        /// </summary>
        public T BufferedStream { get; }

        /// <summary>
        /// If to leave the <see cref="BufferedStream"/> open when disposing
        /// </summary>
        public bool LeaveOpen { get; set; }

        /// <summary>
        /// Last exception of the buffer background worker task
        /// </summary>
        public Exception? LastException { get; protected set; }

        /// <summary>
        /// Buffer worker
        /// </summary>
        protected virtual async Task BufferWorker()
        {
            await Task.Yield();
            try
            {
                await BufferedStream.CopyToAsync(this, BufferSize, BufferCancellation.Token).DynamicContext();
            }
            catch(ObjectDisposedException) when(IsDisposing)
            {
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken.IsEqualTo(BufferCancellation.Token))
            {
            }
            catch (Exception ex)
            {
                ErrorHandling.Handle(new("Pre-buffering stream background buffer task failed", ex, tag: this));
                LastException = ex;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            BufferCancellation.Cancel();
            base.Dispose(disposing);
            BufferTask.GetAwaiter().GetResult();
            BufferCancellation.Dispose();
            BufferedStream.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            BufferCancellation.Cancel();
            await base.DisposeCore().DynamicContext();
            await BufferTask.DynamicContext();
            BufferCancellation.Dispose();
            await BufferedStream.DisposeAsync().DynamicContext();
        }
    }
}

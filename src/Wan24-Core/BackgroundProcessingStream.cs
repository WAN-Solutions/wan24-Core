namespace wan24.Core
{
    /// <summary>
    /// Base class for a background processing stream
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="bufferSize">Buffer size in bytes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [Obsolete("Use BackgroundProcessingStreamBase instead")]//TODO Remove in v3
    public abstract class BackgroundProcessingStream(in int bufferSize, in CancellationToken cancellationToken = default)
        : BackgroundProcessingStreamBase(bufferSize, cancellationToken)
    {
    }

    /// <summary>
    /// Base class for a background processing stream
    /// </summary>
    public abstract class BackgroundProcessingStreamBase : BlockingBufferStream
    {
        /// <summary>
        /// Cancellation
        /// </summary>
        protected readonly CancellationTokenSource Cancellation = new();
        /// <summary>
        /// Task scheduler to use for the processing background task
        /// </summary>
        protected TaskScheduler? Scheduler = null;
        /// <summary>
        /// Is a long running process?
        /// </summary>
        protected bool LongRunning = true;
        /// <summary>
        /// Did process already?
        /// </summary>
        protected bool DidProcess = false;
        /// <summary>
        /// Processor task
        /// </summary>
        protected Task? ProcessorTask = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected BackgroundProcessingStreamBase(in int bufferSize, in CancellationToken cancellationToken = default) : base(bufferSize)
            => Cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        /// <summary>
        /// Last processing exception (may be set during disposing)
        /// </summary>
        public Exception? LastException { get; protected set; }

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
            => throw new InvalidOperationException();

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException();

        /// <inheritdoc/>
        public override void WriteByte(byte value)
            => throw new InvalidOperationException();

        /// <inheritdoc/>
        public override int TryWrite(ReadOnlySpan<byte> buffer)
            => throw new InvalidOperationException();

        /// <inheritdoc/>
        public override ValueTask<int> TryWriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException();

        /// <inheritdoc/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
            => throw new InvalidOperationException();

        /// <summary>
        /// Process (must use <see cref="WriteIntAsync(ReadOnlyMemory{byte}, CancellationToken)"/> or <see cref="TryWriteIntAsync(ReadOnlyMemory{byte}, CancellationToken)"/> 
        /// for writing to the buffer!)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected abstract Task ProcessAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Internal write (uses the base <see cref="WriteAsync(ReadOnlyMemory{byte}, CancellationToken)"/>)
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual ValueTask WriteIntAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            => base.WriteAsync(buffer, cancellationToken);

        /// <summary>
        /// Internal try write (uses the base <see cref="TryWriteAsync(ReadOnlyMemory{byte}, CancellationToken)"/>)
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of written bytes</returns>
        protected virtual ValueTask<int> TryWriteIntAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            => base.TryWriteAsync(buffer, cancellationToken);

        /// <summary>
        /// Processor
        /// </summary>
        protected virtual async Task ProcessorAsync()
        {
            try
            {
                await ProcessAsync(Cancellation.Token).DynamicContext();
                await SetIsEndOfFileAsync(Cancellation.Token).DynamicContext();
            }
            catch(ObjectDisposedException) when (IsDisposing)
            {
            }
            catch(OperationCanceledException) when (Cancellation.IsCancellationRequested)
            {
            }
            catch(Exception ex)
            {
                SetLastException(ex);
            }
            finally
            {
                ProcessorTask = null;
            }
        }

        /// <summary>
        /// Start processing (can only be called once!)
        /// </summary>
        protected virtual void StartProcessing()
        {
            EnsureUndisposed();
            if (DidProcess) throw new InvalidOperationException();
            DidProcess = true;
            ProcessorTask = LongRunning
                ? ((Func<Task>)ProcessorAsync).StartLongRunningTask(Scheduler, Cancellation.Token)
                : ((Func<Task>)ProcessorAsync).StartFairTask(Scheduler, Cancellation.Token);
        }

        /// <summary>
        /// Start processing (can only be called once!)
        /// </summary>
        protected virtual Task StartProcessingAsync()
        {
            StartProcessing();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Set the last exception and start disposing
        /// </summary>
        /// <param name="ex">Exception</param>
        protected virtual void SetLastException(in Exception ex)
        {
            LastException = ex;
            RaiseOnError();
            if (!IsDisposing)
                _ = DisposeAsync().DynamicContext();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Cancellation.Cancel();
            ProcessorTask?.GetAwaiter().GetResult();
            base.Dispose(disposing);
            Cancellation.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await Cancellation.CancelAsync().DynamicContext();
            if (ProcessorTask is Task processor) await processor.DynamicContext();
            await base.DisposeCore().DynamicContext();
            Cancellation.Dispose();
        }

        /// <summary>
        /// Delegate for an <see cref="OnError"/> event handler
        /// </summary>
        /// <param name="stream">Sender</param>
        /// <param name="e">Arguments</param>
        public delegate void Error_Delegate(BackgroundProcessingStreamBase stream, EventArgs e);
        /// <summary>
        /// Raised on error (see <see cref="LastException"/>)
        /// </summary>
        public event Error_Delegate? OnError;
        /// <summary>
        /// Raise the <see cref="OnError"/> event
        /// </summary>
        protected virtual void RaiseOnError() => OnError?.Invoke(this, EventArgs.Empty);
    }
}

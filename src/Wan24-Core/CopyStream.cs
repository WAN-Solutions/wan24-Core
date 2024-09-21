namespace wan24.Core
{
    /// <summary>
    /// Copy stream (copies the wrapped stream to a target stream using a background task)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Readable base stream</param>
    /// <param name="copyTarget">Writable copy target stream</param>
    /// <param name="leaveBaseStreamOpen">If to leave the base stream open when disposing</param>
    /// <param name="leaveTargetStreamOpen">If to leave the copy target stream open when disposing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public class CopyStream(
        in Stream baseStream,
        in Stream copyTarget,
        in bool leaveBaseStreamOpen = false,
        in bool leaveTargetStreamOpen = false,
        in CancellationToken cancellationToken = default
        )
        : CopyStream<Stream, Stream>(baseStream, copyTarget, leaveBaseStreamOpen, leaveTargetStreamOpen, cancellationToken)
    {
    }

    /// <summary>
    /// Copy stream (copies the wrapped stream to a target stream using a background task)
    /// </summary>
    /// <typeparam name="T">Copy target stream type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Readable base stream</param>
    /// <param name="copyTarget">Writable copy target stream</param>
    /// <param name="leaveBaseStreamOpen">If to leave the base stream open when disposing</param>
    /// <param name="leaveTargetStreamOpen">If to leave the copy target stream open when disposing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public class CopyStream<T>(
        in Stream baseStream,
        in T copyTarget,
        in bool leaveBaseStreamOpen = false,
        in bool leaveTargetStreamOpen = false,
        in CancellationToken cancellationToken = default
        )
        : CopyStream<Stream, T>(baseStream, copyTarget, leaveBaseStreamOpen, leaveTargetStreamOpen, cancellationToken)
        where T : Stream
    {
    }

    /// <summary>
    /// Copy stream (copies the wrapped stream to a target stream using a background task)
    /// </summary>
    /// <typeparam name="tStream">Wrapped stream type</typeparam>
    /// <typeparam name="tTarget">Copy target stream type</typeparam>
    public class CopyStream<tStream, tTarget> : WrapperStream<tStream>, ICopyStream
        where tStream : Stream
        where tTarget : Stream
    {
        /// <summary>
        /// Cancellation
        /// </summary>
        protected readonly CancellationTokenSource Cancellation = new();
        /// <summary>
        /// Cancellations
        /// </summary>
        protected readonly Cancellations _Cancellations;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Readable base stream</param>
        /// <param name="copyTarget">Writable copy target stream</param>
        /// <param name="leaveBaseStreamOpen">If to leave the base stream open when disposing</param>
        /// <param name="leaveTargetStreamOpen">If to leave the copy target stream open when disposing</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public CopyStream(
            in tStream baseStream, 
            in tTarget copyTarget, 
            in bool leaveBaseStreamOpen = false, 
            in bool leaveTargetStreamOpen = false,
            in CancellationToken cancellationToken = default
            )
            : base(baseStream, leaveBaseStreamOpen)
        {
            if (!baseStream.CanRead)
                throw new ArgumentException("Readable base stream required", nameof(baseStream));
            if (!copyTarget.CanWrite)
                throw new ArgumentException("Writable copy target stream required", nameof(copyTarget));
            CopyTarget = copyTarget;
            LeaveTargetOpen = leaveTargetStreamOpen;
            _Cancellations = cancellationToken.IsEqualTo(default)
                ? new(Cancellation.Token) 
                : new(Cancellation.Token, cancellationToken);
            CopyTask = ((Func<Task>)StartCopyAsync).StartLongRunningTask(cancellationToken: CancellationToken.None);
        }

        /// <summary>
        /// Target stream
        /// </summary>
        public tTarget CopyTarget { get; }

        /// <inheritdoc/>
        Stream ICopyStream.CopyTarget => CopyTarget;

        /// <inheritdoc/>
        public bool LeaveTargetOpen { get; set; }

        /// <inheritdoc/>
        public bool AutoDispose { get; set; }

        /// <inheritdoc/>
        public Task CopyTask { get; }

        /// <inheritdoc/>
        public Exception? LastException { get; protected set; }

        /// <inheritdoc/>
        public CancellationToken CancelToken { get; }

        /// <inheritdoc/>
        public CancellationToken CopyCancellation => Cancellation.Token;

        /// <inheritdoc/>
        public CancellationToken Cancellations => _Cancellations;

        /// <inheritdoc/>
        public bool IsCopyCompleted { get; protected set; }

        /// <inheritdoc/>
        public virtual void CancelCopy() => Cancellation.Cancel();

        /// <inheritdoc/>
        public virtual Task CancelCopyAsync() => Cancellation.CancelAsync();

        /// <summary>
        /// Start the copy process
        /// </summary>
        protected virtual async Task StartCopyAsync()
        {
            await Task.Yield();
            try
            {
                await CopyToAsync(CopyTarget, _Cancellations).DynamicContext();
                IsCopyCompleted = true;
                RaiseOnComplete();
            }
            catch(Exception ex)
            {
                LastException = ex;
                RaiseOnError();
            }
            if (AutoDispose)
                await DisposeAsync().DynamicContext();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Cancellation.Cancel();
            base.Dispose(disposing);
            Cancellation.Dispose();
            _Cancellations.Dispose();
            if (!LeaveTargetOpen)
                CopyTarget.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await Cancellation.CancelAsync().DynamicContext();
            await base.DisposeCore().DynamicContext();
            Cancellation.Dispose();
            await _Cancellations.DisposeAsync().DynamicContext();
            if (!LeaveTargetOpen)
                await CopyTarget.DisposeAsync().DynamicContext();
        }

        /// <inheritdoc/>
        public event ICopyStream.CopyStream_Delegate? OnError;
        /// <summary>
        /// Raise the <see cref="OnError"/> event
        /// </summary>
        protected virtual void RaiseOnError() => OnError?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public event ICopyStream.CopyStream_Delegate? OnComplete;
        /// <summary>
        /// Raise the <see cref="OnComplete"/> event
        /// </summary>
        protected virtual void RaiseOnComplete() => OnComplete?.Invoke(this, EventArgs.Empty);
    }
}

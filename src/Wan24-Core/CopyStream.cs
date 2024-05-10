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
    public class CopyStream<tStream, tTarget> : WrapperStream<tStream>
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
            _Cancellations = Equals(cancellationToken, default) 
                ? new(Cancellation.Token) 
                : new(Cancellation.Token, cancellationToken);
            CopyTask = StartCopyAsync();
        }

        /// <summary>
        /// Target stream
        /// </summary>
        public tTarget CopyTarget { get; }

        /// <summary>
        /// If to leave the target stream open when disposing
        /// </summary>
        public bool LeaveTargetOpen { get; set; }

        /// <summary>
        /// If to dispose when the copy task did finish
        /// </summary>
        public bool AutoDispose { get; set; }

        /// <summary>
        /// Copy background task
        /// </summary>
        public Task CopyTask { get; }

        /// <summary>
        /// Last copy background task exception
        /// </summary>
        public Exception? LastException { get; protected set; }

        /// <summary>
        /// Cancellation token (as given to the constructor)
        /// </summary>
        public CancellationToken CancelToken { get; }

        /// <summary>
        /// The copy cancellation token (canceled using <see cref="CancelCopy"/>)
        /// </summary>
        public CancellationToken CopyCancellation => Cancellation.Token;

        /// <summary>
        /// Combined cancellation token (<see cref="CancelToken"/> and <see cref="CopyCancellation"/>)
        /// </summary>
        public CancellationToken Cancellations => _Cancellations;

        /// <summary>
        /// If the copy did complete
        /// </summary>
        public bool IsCopyCompleted { get; protected set; }

        /// <summary>
        /// Cancel the copy background task
        /// </summary>
        public virtual void CancelCopy() => Cancellation.Cancel();

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
            Cancellation.Cancel();
            await base.DisposeCore().DynamicContext();
            Cancellation.Dispose();
            await _Cancellations.DisposeAsync().DynamicContext();
            if (!LeaveTargetOpen)
                await CopyTarget.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Delegate for an event handler
        /// </summary>
        /// <param name="stream">Sender</param>
        /// <param name="e">Arguments</param>
        public delegate void CopyStream_Delegate(CopyStream<tStream, tTarget> stream, EventArgs e);

        /// <summary>
        /// Raised on error
        /// </summary>
        public event CopyStream_Delegate? OnError;
        /// <summary>
        /// Raise the <see cref="OnError"/> event
        /// </summary>
        protected virtual void RaiseOnError() => OnError?.Invoke(this, new());

        /// <summary>
        /// Raised on completed
        /// </summary>
        public event CopyStream_Delegate? OnComplete;
        /// <summary>
        /// Raise the <see cref="OnComplete"/> event
        /// </summary>
        protected virtual void RaiseOnComplete() => OnComplete?.Invoke(this, new());
    }
}

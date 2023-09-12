namespace wan24.Core
{
    /// <summary>
    /// Bound cancellation token source (canceled when a parent token was canceled)
    /// </summary>
    public class BoundCancellationTokenSource : CancellationTokenSource
    {
        /// <summary>
        /// Is disposed?
        /// </summary>
        private bool IsDisposed = false;
        /// <summary>
        /// Cancellation registration
        /// </summary>
        protected CancellationTokenRegistration? CancelRegistration = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public BoundCancellationTokenSource(in CancellationToken cancellationToken) : base() => Rebind(cancellationToken);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="delayMs">Cancellation delay in ms</param>
        public BoundCancellationTokenSource(in CancellationToken cancellationToken, in int delayMs) : base(delayMs) => Rebind(cancellationToken);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="delay">Cancellation delay</param>
        public BoundCancellationTokenSource(in CancellationToken cancellationToken, in TimeSpan delay) : base(delay) => Rebind(cancellationToken);

        /// <summary>
        /// Parent cancellation token
        /// </summary>
        public CancellationToken ParentToken { get; protected set; } = default;

        /// <summary>
        /// Is bound to the parent token?
        /// </summary>
        public bool IsBoundToParent { get; protected set; }

        /// <summary>
        /// Unbind
        /// </summary>
        public virtual void Unbind()
        {
            if (!CancelRegistration.HasValue) return;
            CancelRegistration.Value.Dispose();
            CancelRegistration = null;
            IsBoundToParent = false;
        }

        /// <summary>
        /// Re-bind to another cancellation token
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual void Rebind(in CancellationToken cancellationToken)
        {
            Unbind();
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            ParentToken = cancellationToken;
            CancelRegistration = cancellationToken.Register(() =>
            {
                if (!IsCancellationRequested) Cancel();
            });
            IsBoundToParent = true;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            IsDisposed = true;
            base.Dispose(disposing);
            Unbind();
        }
    }
}

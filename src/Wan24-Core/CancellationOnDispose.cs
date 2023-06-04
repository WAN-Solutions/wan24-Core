namespace wan24.Core
{
    /// <summary>
    /// <see cref="CancellationOnDispose"/> cancels a <see cref="CancellationTokenSource"/> when an object is disposing (or a <see cref="CancellationToken"/> was canceled)
    /// </summary>
    public sealed class CancellationOnDispose : DisposableBase
    {
        /// <summary>
        /// <see cref="CancellationTokenSource"/>
        /// </summary>
        private readonly CancellationTokenSource CancellationSource = new();
        /// <summary>
        /// <see cref="CancellationTokenRegistration"/>
        /// </summary>
        private readonly CancellationTokenRegistration? CancellationRegistration;
        /// <summary>
        /// Was initialized?
        /// </summary>
        private readonly bool WasInitialized = true;
        /// <summary>
        /// Monitored object (won't be disposed)
        /// </summary>
        public readonly IDisposableObject Object;
        /// <summary>
        /// Monitored <see cref="CancellationToken"/>
        /// </summary>
        public readonly CancellationToken? Token;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Monitored object (won't be disposed)</param>
        /// <param name="cancellationToken">Monitored <see cref="CancellationToken"/></param>
        public CancellationOnDispose(IDisposableObject obj, CancellationToken? cancellationToken = null) : base()
        {
            Cancellation = CancellationSource.Token;
            CancellationRegistration = null;
            Object = obj;
            Token = cancellationToken;
            lock (CancellationSource)
                if (obj.IsDisposing || (cancellationToken?.IsCancellationRequested ?? false))
                {
                    CancellationSource.Cancel();
                    CancellationSource.Dispose();
                    Dispose();
                }
                else
                {
                    obj.OnDisposing += HandleDispose;
                    if (cancellationToken != null) CancellationRegistration = cancellationToken.Value.Register(Dispose);
                    if (obj.IsDisposing || (cancellationToken?.IsCancellationRequested ?? false)) Dispose();
                }
        }

        /// <summary>
        /// The <see cref="CancellationToken"/> which will be canceled if the object is disposing (or the monitored <see cref="CancellationToken"/> was canceled; may be canceled 
        /// already; will be canceled when disposing)
        /// </summary>
        public CancellationToken Cancellation { get; }

        /// <summary>
        /// Handle the monitored object disposing
        /// </summary>
        /// <param name="obj">Sender</param>
        /// <param name="e"><see cref="EventArgs"/></param>
        private void HandleDispose(IDisposableObject obj, EventArgs e) => Dispose();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            lock (CancellationSource)
            {
                if (!WasInitialized) return;
                Object.OnDisposing -= HandleDispose;
                CancellationRegistration?.Dispose();
                if (!Cancellation.IsCancellationRequested) CancellationSource.Cancel();
                CancellationSource.Dispose();
            }
        }

        /// <summary>
        /// Cast as cancellation token
        /// </summary>
        /// <param name="cod"><see cref="CancellationOnDispose"/></param>
        public static implicit operator CancellationToken(CancellationOnDispose cod) => cod.Cancellation;

        /// <summary>
        /// Cast as disposed flag
        /// </summary>
        /// <param name="cod"><see cref="CancellationOnDispose"/></param>
        public static implicit operator bool(CancellationOnDispose cod) => !cod.IsDisposed;
    }
}

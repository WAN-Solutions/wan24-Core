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
        private readonly bool WasInitialized = false;
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
            try
            {
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
                        WasInitialized = true;
                        if (obj.IsDisposing || (cancellationToken?.IsCancellationRequested ?? false)) Dispose();
                    }
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// The <see cref="CancellationToken"/> which will be canceled if the object is disposing (or the monitored <see cref="CancellationToken"/> was canceled; may be canceled 
        /// already; will be canceled when disposing)
        /// </summary>
        public CancellationToken Cancellation { get; }

        /// <summary>
        /// Throw an exception, if canceled
        /// </summary>
        /// <param name="throwIfCancellationRequested">Throw an exception, if the monitored <see cref="CancellationToken"/> was canceled or this instance was disposed?</param>
        /// <returns><see langword="true"/>, if the monitored object wasn't disposed and the <see cref="CancellationToken"/> wasn't canceled, <see langword="false"/>, if the 
        /// <see cref="CancellationToken"/> was canceled</returns>
        /// <exception cref="OperationCanceledException">The monitored <see cref="CancellationToken"/> was canceled or this instance was disposed (and <c>throwIfCanceled</c> was 
        /// <see langword="true"/>)</exception>
        /// <exception cref="ObjectDisposedException">The monitored object was disposed (but the monitored <see cref="CancellationToken"/> wasn't canceled)</exception>
        public bool ThrowIfCanceled(bool throwIfCancellationRequested = true)
        {
            lock (CancellationSource)
                if (!(CancellationRegistration?.Token.IsCancellationRequested ?? false) && !Object.IsDisposing && !Cancellation.IsCancellationRequested)
                    return true;
            if (CancellationRegistration?.Token.IsCancellationRequested ?? false)
            {
                if (throwIfCancellationRequested) CancellationRegistration.Value.Token.ThrowIfCancellationRequested();
                return false;
            }
            if (!Object.IsDisposing)
            {
                if (throwIfCancellationRequested) Cancellation.ThrowIfCancellationRequested();
                return false;
            }
            throw new ObjectDisposedException(Object.GetType().ToString());
        }

        /// <summary>
        /// Throw an exception, if the monitored object was disposed
        /// </summary>
        /// <returns><see langword="true"/>, if the monitored object wasn't disposed and the <see cref="CancellationToken"/> wasn't canceled, <see langword="false"/>, if the 
        /// <see cref="CancellationToken"/> was canceled</returns>
        /// <exception cref="ObjectDisposedException">The monitored object was disposed (but the monitored <see cref="CancellationToken"/> wasn't canceled)</exception>
        public bool ThrowIfDisposed() => ThrowIfCanceled(throwIfCancellationRequested: false);

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
                if (!Cancellation.IsCancellationRequested)
                    try
                    {
                        CancellationSource.Cancel();
                    }
                    catch
                    {
                    }
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

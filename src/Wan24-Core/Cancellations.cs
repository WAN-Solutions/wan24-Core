namespace wan24.Core
{
    /// <summary>
    /// <see cref="Cancellations"/> combines multiple <see cref="CancellationToken"/> into one
    /// </summary>
    public sealed class Cancellations : DisposableBase
    {
        /// <summary>
        /// <see cref="CancellationTokenSource"/>
        /// </summary>
        private readonly CancellationTokenSource CancellationSource = new();
        /// <summary>
        /// <see cref="CancellationTokenRegistration"/>
        /// </summary>
        private readonly CancellationTokenRegistration[] CancellationRegistrations;
        /// <summary>
        /// Was initialized?
        /// </summary>
        private readonly bool WasInitialized = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cts">Monitored <see cref="CancellationToken"/></param>
        public Cancellations(params CancellationToken[] cts) : base()
        {
            CancellationRegistrations = new CancellationTokenRegistration[cts.Length];
            Cancellation = CancellationSource.Token;
            try
            {
                lock (CancellationSource)
                    for (int i = 0; i < cts.Length; i++)
                    {
                        if (cts[i].IsCancellationRequested)
                        {
                            WasInitialized = false;
                            for (int j = 0; j < i; CancellationRegistrations[j].Dispose(), j++) ;
                            CancellationSource.Cancel();
                            CancellationSource.Dispose();
                            Dispose();
                            break;
                        }
                        CancellationRegistrations[i] = cts[i].Register(Dispose);
                    }
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// The <see cref="CancellationToken"/> which will be canceled if any of the monitored cancellation tokens was canceled
        /// </summary>
        public CancellationToken Cancellation { get; }

        /// <summary>
        /// Determine if any <see cref="CancellationToken"/> was canceled
        /// </summary>
        public bool IsAnyCanceled => CancellationRegistrations.Any(r => r != default && r.Token.IsCancellationRequested);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            lock (CancellationSource)
            {
                if (!WasInitialized) return;
                for (int i = 0; i < CancellationRegistrations.Length; i++)
                    if (CancellationRegistrations[i] != default)
                        CancellationRegistrations[i].Dispose();
                if (!CancellationSource.IsCancellationRequested)
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
        /// <param name="cancellations"><see cref="Cancellations"/></param>
        public static implicit operator CancellationToken(in Cancellations cancellations) => cancellations.Cancellation;

        /// <summary>
        /// Cast as disposed flag
        /// </summary>
        /// <param name="cancellations"><see cref="Cancellations"/></param>
        public static implicit operator bool(in Cancellations cancellations) => !cancellations.IsDisposing;
    }
}

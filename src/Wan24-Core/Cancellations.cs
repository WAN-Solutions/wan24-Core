namespace wan24.Core
{
    /// <summary>
    /// <see cref="Cancellations"/> combines multiple cancellation tokens into one
    /// </summary>
    public sealed class Cancellations : DisposableBase
    {
        /// <summary>
        /// Cancellation source
        /// </summary>
        private readonly CancellationTokenSource CancellationSource = new();
        /// <summary>
        /// Cancellation registration
        /// </summary>
        private readonly CancellationTokenRegistration[] CancellationRegistrations;
        /// <summary>
        /// Was initialized?
        /// </summary>
        private readonly bool WasInitialized = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cts">Cancellation tokens</param>
        public Cancellations(params CancellationToken[] cts) : base()
        {
            CancellationRegistrations = new CancellationTokenRegistration[cts.Length];
            Cancellation = CancellationSource.Token;
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

        /// <summary>
        /// The <see cref="CancellationToken"/> which will be canceled if any of the monitored cancellation tokens are canceled
        /// </summary>
        public CancellationToken Cancellation { get; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            lock (CancellationSource)
            {
                if (!WasInitialized) return;
                for (int i = 0; i < CancellationRegistrations.Length; CancellationRegistrations[i].Dispose(), i++) ;
                if (!CancellationSource.IsCancellationRequested) CancellationSource.Cancel();
                CancellationSource.Dispose();
            }
        }

        /// <summary>
        /// Cast as cancellation token
        /// </summary>
        /// <param name="cancellations"><see cref="Cancellations"/></param>
        public static implicit operator CancellationToken(Cancellations cancellations) => cancellations.Cancellation;

        /// <summary>
        /// Cast as disposed flag
        /// </summary>
        /// <param name="cancellations"><see cref="Cancellations"/></param>
        public static implicit operator bool(Cancellations cancellations) => !cancellations.IsDisposed;
    }
}

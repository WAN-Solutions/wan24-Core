using System.Collections.Immutable;

namespace wan24.Core
{
    /// <summary>
    /// Bound cancellation token source (canceled when a parent token was canceled)
    /// </summary>
    public class BoundCancellationTokenSource : CancellationTokenSource
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Bound cancellation tokens
        /// </summary>
        protected readonly List<CancellationToken> _BoundTokens = [];
        /// <summary>
        /// Cancellation registrations
        /// </summary>
        protected readonly List<CancellationTokenRegistration> CancelRegistrations = [];
        /// <summary>
        /// Is disposed?
        /// </summary>
        private bool IsDisposed = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cancellationTokens">Cancellation tokens</param>
        public BoundCancellationTokenSource(params CancellationToken[] cancellationTokens) : base() => AddTokens(cancellationTokens);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="delayMs">Cancellation delay in ms</param>
        /// <param name="cancellationTokens">Cancellation tokens</param>
        public BoundCancellationTokenSource(in int delayMs, params CancellationToken[] cancellationTokens) : base(delayMs) => AddTokens(cancellationTokens);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="delay">Cancellation delay</param>
        /// <param name="cancellationTokens">Cancellation tokens</param>
        public BoundCancellationTokenSource(in TimeSpan delay, params CancellationToken[] cancellationTokens) : base(delay) => AddTokens(cancellationTokens);

        /// <summary>
        /// Bound cancellation token
        /// </summary>
        public ImmutableArray<CancellationToken> BoundTokens => [.. _BoundTokens];

        /// <summary>
        /// Add tokens
        /// </summary>
        /// <param name="cancellationTokens">Cancellation tokens</param>
        public virtual void AddTokens(params CancellationToken[] cancellationTokens)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            if (IsCancellationRequested) throw new InvalidOperationException("Cancelled");
            using SemaphoreSyncContext ssc = Sync;
            foreach (CancellationToken cancellationToken in cancellationTokens)
            {

                if (_BoundTokens.Any(t => t.IsEqualTo(cancellationToken))) continue;
                CancelRegistrations.Add(cancellationToken.Register(() =>
                {
                    try
                    {
                        using SemaphoreSyncContext ssc = Sync;
                        if (IsDisposed) return;
                        if (!IsCancellationRequested) Cancel();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }));
                _BoundTokens.Add(cancellationToken);
            }
        }

        /// <summary>
        /// Remove a token
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual void RemoveToken(in CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            using SemaphoreSyncContext ssc = Sync;
            for (int i = 0, len = _BoundTokens.Count; i < len; i++)
            {
                if (!_BoundTokens[i].IsEqualTo(cancellationToken)) continue;
                CancelRegistrations[i].Dispose();
                CancelRegistrations.RemoveAt(i);
                _BoundTokens.RemoveAt(i);
            }
        }

        /// <summary>
        /// Remove all tokens
        /// </summary>
        public virtual void Clear()
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            foreach (CancellationToken cancellationToken in _BoundTokens.ToArray())
                RemoveToken(cancellationToken);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            IsDisposed = true;
            using (SemaphoreSyncContext ssc = Sync)
                foreach (CancellationTokenRegistration ctr in CancelRegistrations)
                    ctr.Dispose();
            Sync.Dispose();
            base.Dispose(disposing);
        }
    }
}

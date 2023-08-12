namespace wan24.Core
{
    /// <summary>
    /// Disposing asynchronous enumerator (disposes tagged objects when disposing)
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public sealed class DisposingAsyncEnumerator<T> : DisposableBase, IAsyncEnumerator<T>, IAsyncEnumerable<T>
    {
        /// <summary>
        /// Enumerable
        /// </summary>
        private readonly IAsyncEnumerable<T>? Enumerable;
        /// <summary>
        /// Disposables
        /// </summary>
        private readonly object?[] Disposables;
        /// <summary>
        /// Cancellation
        /// </summary>
        private CancellationToken Cancellation;
        /// <summary>
        /// Enumerator
        /// </summary>
        private IAsyncEnumerator<T>? Enumerator;
        /// <summary>
        /// Started enumerating?
        /// </summary>
        private bool Started = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="disposables">Disposables</param>
        public DisposingAsyncEnumerator(IAsyncEnumerable<T> enumerable, params object?[] disposables) : base()
        {
            Enumerable = enumerable;
            Enumerator = null;
            Disposables = disposables;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="disposables">Disposables</param>
        public DisposingAsyncEnumerator(IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken, params object?[] disposables) : this(enumerable, disposables)
            => Cancellation = cancellationToken;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerator">Enumerator (will be disposed)</param>
        /// <param name="disposables">Disposables</param>
        public DisposingAsyncEnumerator(IAsyncEnumerator<T> enumerator, params object?[] disposables) : base()
        {
            Enumerable = null;
            Enumerator = enumerator;
            Disposables = disposables;
            Started = true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerator">Enumerator (will be disposed)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="disposables">Disposables</param>
        public DisposingAsyncEnumerator(IAsyncEnumerator<T> enumerator, CancellationToken cancellationToken, params object?[] disposables) : this(enumerator, disposables)
            => Cancellation = cancellationToken;

        /// <inheritdoc/>
        public T Current
        {
            get
            {
                EnsureUndisposed();
                if (!Started) throw new InvalidOperationException();
                return Enumerator!.Current;
            }
        }

        /// <inheritdoc/>
        public async ValueTask<bool> MoveNextAsync()
        {
            EnsureUndisposed();
            if (!Started)
            {
                Enumerator = Enumerable!.GetAsyncEnumerator(Cancellation);
                Started = true;
            }
            if (await Enumerator!.MoveNextAsync().DynamicContext()) return true;
            await DisposeAsync().DynamicContext();
            return false;
        }

        /// <inheritdoc/>
        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            if (Started) throw new InvalidOperationException();
            Cancellation = cancellationToken;
            return this;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Enumerator?.DisposeAsync().AsTask().Wait();
            foreach (object? obj in Disposables)
                if (obj is not null)
                    if (obj is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    else if (obj is IAsyncDisposable asyncDisposable)
                    {
                        asyncDisposable.DisposeAsync().AsTask().Wait();
                    }
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (Enumerator is not null) await Enumerator.DisposeAsync().DynamicContext();
            foreach (object? obj in Disposables)
                if (obj is not null)
                    if (obj is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync().DynamicContext();
                    }
                    else if (obj is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
        }
    }
}

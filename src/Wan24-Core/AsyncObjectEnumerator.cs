namespace wan24.Core
{
    /// <summary>
    /// Asynchronous object enumerator
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public sealed class AsyncObjectEnumerator<T> : BasicAllDisposableBase, IAsyncEnumerator<object>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerator">Enumerator</param>
        public AsyncObjectEnumerator(in IAsyncEnumerator<T> enumerator) : base() => BaseEnumerator = enumerator;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public AsyncObjectEnumerator(in IAsyncEnumerable<T> enumerable, in CancellationToken cancellationToken = default) : base()
            => BaseEnumerator = enumerable.GetAsyncEnumerator(cancellationToken);

        /// <summary>
        /// Enumerator
        /// </summary>
        public IAsyncEnumerator<T> BaseEnumerator { get; }

        /// <inheritdoc/>
        public object Current => BaseEnumerator.Current!;

        /// <inheritdoc/>
        public ValueTask<bool> MoveNextAsync() => BaseEnumerator.MoveNextAsync();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => BaseEnumerator.TryDispose();

        /// <inheritdoc/>
        protected override async Task DisposeCore() => await BaseEnumerator.DisposeAsync().DynamicContext();
    }
}

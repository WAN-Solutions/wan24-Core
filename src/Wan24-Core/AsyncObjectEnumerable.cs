namespace wan24.Core
{
    /// <summary>
    /// Asynchronous object enumerable
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public sealed class AsyncObjectEnumerable<T> : DisposableBase, IAsyncEnumerable<object>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerable">Enumerable</param>
        public AsyncObjectEnumerable(in IAsyncEnumerable<T> enumerable) : base() => BaseEnumerable = enumerable;

        /// <summary>
        /// Enumerable
        /// </summary>
        public IAsyncEnumerable<T> BaseEnumerable { get; }

        /// <inheritdoc/>
        public IAsyncEnumerator<object> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new AsyncObjectEnumerator<T>(BaseEnumerable);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => BaseEnumerable.TryDispose();

        /// <inheritdoc/>
        protected override async Task DisposeCore() => await BaseEnumerable.TryDisposeAsync().DynamicContext();
    }
}

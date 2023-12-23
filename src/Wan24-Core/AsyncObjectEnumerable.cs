namespace wan24.Core
{
    /// <summary>
    /// Asynchronous object enumerable
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="enumerable">Enumerable</param>
    public sealed class AsyncObjectEnumerable<T>(in IAsyncEnumerable<T> enumerable) : DisposableBase(), IAsyncEnumerable<object>
    {
        /// <summary>
        /// Enumerable
        /// </summary>
        public IAsyncEnumerable<T> BaseEnumerable { get; } = enumerable;

        /// <inheritdoc/>
        public IAsyncEnumerator<object> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new AsyncObjectEnumerator<T>(BaseEnumerable);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => BaseEnumerable.TryDispose();

        /// <inheritdoc/>
        protected override async Task DisposeCore() => await BaseEnumerable.TryDisposeAsync().DynamicContext();
    }
}

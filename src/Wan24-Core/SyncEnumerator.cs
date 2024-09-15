using System.Collections;

namespace wan24.Core
{
    /// <summary>
    /// Synchronous enumerator/enumerable for an asynchronous enumerator (don't forget to dispose!)
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="enumerator">Asynchronous enumerator (will be disposed)</param>
    public sealed class SyncEnumerator<T>(in IAsyncEnumerator<T> enumerator) : DisposableBase(), IEnumerable<T>, IEnumerator<T>
    {
        /// <summary>
        /// Asynchronous enumerator (will be disposed)
        /// </summary>
        private readonly IAsyncEnumerator<T> Enumerator = enumerator;

        /// <inheritdoc/>
        public T Current => IfUndisposed(() => Enumerator.Current);

        /// <inheritdoc/>
        object IEnumerator.Current => Current!;

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => IfUndisposed(this);

        /// <inheritdoc/>
        public bool MoveNext() => EnsureUndisposed(throwException: false) && Enumerator.MoveNextAsync().AsTask().GetAwaiter().GetResult();

        /// <inheritdoc/>
        public void Reset() => IfUndisposed(() => throw new NotSupportedException());

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => Enumerator.DisposeAsync().AsTask().GetAwaiter().GetResult();

        /// <inheritdoc/>
        protected override async Task DisposeCore() => await Enumerator.DisposeAsync().DynamicContext();
    }
}

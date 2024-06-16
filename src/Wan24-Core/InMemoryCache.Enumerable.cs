using System.Collections;

namespace wan24.Core
{
    // Enumerable
    public partial class InMemoryCache<T> : IEnumerable<KeyValuePair<string, InMemoryCacheEntry<T>>>, IEnumerable<T>
    {
        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, InMemoryCacheEntry<T>>> GetEnumerator()
        {
            EnsureUndisposed();
            return Cache.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            EnsureUndisposed();
            return Items.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            EnsureUndisposed();
            return Cache.GetEnumerator();
        }
    }
}

namespace wan24.Core
{
    /// <summary>
    /// Asynchronous concurrent dictionary
    /// </summary>
    /// <typeparam name="tKey">Key type</typeparam>
    /// <typeparam name="tValue">Value type</typeparam>
    public partial class AsyncConcurrentDictionary<tKey, tValue> : BasicDisposableBase where tKey : notnull
    {
        /// <summary>
        /// Dictionary
        /// </summary>
        protected readonly Dictionary<tKey, tValue> Dict;
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();

        /// <summary>
        /// Constructor
        /// </summary>
        public AsyncConcurrentDictionary() : base() => Dict = [];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comparer">Key comparer</param>
        public AsyncConcurrentDictionary(in EqualityComparer<tKey>? comparer) => Dict = new(comparer);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public AsyncConcurrentDictionary(in int capacity) => Dict = new(capacity);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="comparer">Key comparer</param>
        public AsyncConcurrentDictionary(in int capacity, in EqualityComparer<tKey>? comparer) => Dict = new(capacity, comparer);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Initial items</param>
        public AsyncConcurrentDictionary(in IEnumerable<KeyValuePair<tKey, tValue>> items) => Dict = new(items);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Initial items</param>
        /// <param name="comparer">Key comparer</param>
        public AsyncConcurrentDictionary(in IEnumerable<KeyValuePair<tKey, tValue>> items, in EqualityComparer<tKey>? comparer) => Dict = new(items, comparer);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dict">Dictionary to copy</param>
        public AsyncConcurrentDictionary(in IDictionary<tKey, tValue> dict) => Dict = new(dict);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dict">Dictionary to copy</param>
        /// <param name="comparer">Key comparer</param>
        public AsyncConcurrentDictionary(in IDictionary<tKey, tValue> dict, in EqualityComparer<tKey>? comparer) => Dict = new(dict, comparer);

        /// <summary>
        /// Add a key/value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="InvalidOperationException">Key exists</exception>
        public virtual async Task AddAsync(tKey key, tValue value, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            if (Dict.ContainsKey(key)) throw new InvalidOperationException("Key exists");
            Dict[key] = value;
        }

        /// <summary>
        /// Set a key/value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task SetAsync(tKey key, tValue value, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            Dict[key] = value;
        }

        /// <summary>
        /// Get a value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public virtual async Task<tValue> GetAsync(tKey key, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            return Dict[key];
        }

        /// <summary>
        /// Try getting a value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public virtual async Task<TryAsyncResult<tValue>> TryGetAsync(tKey key, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            return Dict.TryGetValue(key, out tValue? value)
                ? value
                : false;
        }

        /// <summary>
        /// Determine if a key is contained
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If the key is contained</returns>
        public virtual async Task<bool> ContainsKeyAsync(tKey key, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            return Dict.ContainsKey(key);
        }

        /// <summary>
        /// Remove a key/value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If the key/value was removed</returns>
        public virtual async Task<bool> RemoveAsync(tKey key, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            return Dict.Remove(key);
        }

        /// <summary>
        /// Clear all keys/values
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            Dict.Clear();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Sync.Dispose();
            Dict.Clear();
        }
    }
}

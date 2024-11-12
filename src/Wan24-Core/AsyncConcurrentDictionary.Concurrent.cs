using System.Collections.Concurrent;

namespace wan24.Core
{
    // Concurrent dictionary specific method implementation
    public partial class AsyncConcurrentDictionary<tKey, tValue>
    {
        /// <summary>
        /// Attempts to add the specified key and value to the <see cref="ConcurrentChangeTokenDictionary{tKey, tValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be a null reference (Nothing
        /// in Visual Basic) for reference types.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        /// true if the key/value pair was added to the <see cref="ConcurrentChangeTokenDictionary{tKey, tValue}"/> successfully; otherwise, false.
        /// </returns>
        public virtual async Task<bool> TryAddAsync(tKey key, tValue value, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            return Dict.TryAdd(key, value);
        }

        /// <summary>
        /// Updates the value associated with <paramref name="key"/> to <paramref name="newValue"/> if the existing value is equal
        /// to <paramref name="comparisonValue"/>.
        /// </summary>
        /// <param name="key">The key whose value is compared with <paramref name="comparisonValue"/> and
        /// possibly replaced.</param>
        /// <param name="newValue">The value that replaces the value of the element with <paramref
        /// name="key"/> if the comparison results in equality.</param>
        /// <param name="comparisonValue">The value that is compared to the value of the element with
        /// <paramref name="key"/>.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        /// true if the value with <paramref name="key"/> was equal to <paramref name="comparisonValue"/> and
        /// replaced with <paramref name="newValue"/>; otherwise, false.
        /// </returns>
        public virtual async Task<bool> TryUpdateAsync(tKey key, tValue newValue, tValue comparisonValue, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            if (!Dict.TryGetValue(key, out tValue? existing) || !GenericHelper.Equals(comparisonValue, newValue)) return false;
            Dict[key] = newValue;
            return true;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/> if the key does not already
        /// exist, or updates a key/value pair in the <see cref="ConcurrentDictionary{TKey,TValue}"/> if the key
        /// already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key
        /// based on the key's existing value</param>
        /// <param name="factoryArgument">An argument to pass into <paramref name="addValueFactory"/> and <paramref name="updateValueFactory"/>.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The new value for the key.  This will be either be the result of addValueFactory (if the key was
        /// absent) or the result of updateValueFactory (if the key was present).</returns>
        public virtual async Task<tValue> AddOrUpdateAsync<tArg>(
            tKey key, 
            Func<tKey, tArg, tValue> addValueFactory, 
            Func<tKey, tValue, tArg, tValue> updateValueFactory, 
            tArg factoryArgument, 
            CancellationToken cancellationToken = default
            )
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            tValue res = Dict.TryGetValue(key, out tValue? existing)
                ? updateValueFactory(key, existing, factoryArgument)
                : addValueFactory(key, factoryArgument);
            Dict[key] = res;
            return res;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/> if the key does not already
        /// exist, or updates a key/value pair in the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/> if the key
        /// already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key
        /// based on the key's existing value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The new value for the key.  This will be either the result of addValueFactory (if the key was
        /// absent) or the result of updateValueFactory (if the key was present).</returns>
        public virtual async Task<tValue> AddOrUpdateAsync(
            tKey key, 
            Func<tKey, tValue> addValueFactory, 
            Func<tKey, tValue, tValue> updateValueFactory, 
            CancellationToken cancellationToken = default
            )
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            tValue res = Dict.TryGetValue(key, out tValue? existing)
                ? updateValueFactory(key, existing)
                : addValueFactory(key);
            Dict[key] = res;
            return res;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/> if the key does not already
        /// exist, or updates a key/value pair in the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/> if the key
        /// already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on
        /// the key's existing value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The new value for the key.  This will be either the value of addValue (if the key was
        /// absent) or the result of updateValueFactory (if the key was present).</returns>
        public virtual async Task<tValue> AddOrUpdateAsync(
            tKey key, 
            tValue addValue, 
            Func<tKey, tValue, tValue> updateValueFactory, 
            CancellationToken cancellationToken = default
            )
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            tValue res = Dict.TryGetValue(key, out tValue? existing)
                ? updateValueFactory(key, existing)
                : addValue;
            Dict[key] = res;
            return res;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/>
        /// if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        public virtual async Task<tValue> GetOrAddAsync(tKey key, Func<tKey, tValue> valueFactory, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            if (Dict.TryGetValue(key, out tValue? res)) return res;
            res = valueFactory(key);
            Dict[key] = res;
            return res;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/>
        /// if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <param name="factoryArgument">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        public virtual async Task<tValue> GetOrAddAsync<tArg>(
            tKey key, 
            Func<tKey, tArg, tValue> valueFactory, 
            tArg factoryArgument, 
            CancellationToken cancellationToken = default
            )
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            if (Dict.TryGetValue(key, out tValue? res)) return res;
            res = valueFactory(key, factoryArgument);
            Dict[key] = res;
            return res;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/>
        /// if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value if the key was not in the dictionary.</returns>
        public virtual async Task<tValue> GetOrAddAsync(tKey key, tValue value, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            if (Dict.TryGetValue(key, out tValue? res)) return res;
            Dict[key] = value;
            return value;
        }

        /// <summary>
        /// Attempts to remove and return the value with the specified key from the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Asynchronous try-result</returns>
        public virtual async Task<TryAsyncResult<tValue>> TryRemoveAsync(tKey key, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            return Dict.Remove(key, out tValue? value)
                ? value
                : false;
        }
    }
}

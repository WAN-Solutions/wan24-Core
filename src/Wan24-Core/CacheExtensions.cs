using System.Collections.Immutable;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="ICache{T}"/> extensions
    /// </summary>
    public static class CacheExtensions
    {
        /// <summary>
        /// Get a cache entry
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="caches">Caches</param>
        /// <param name="key">Unique cache entry key</param>
        /// <param name="cachesSync">Caches synchronization</param>
        /// <param name="entryFactory">Cache entry factory (used to create a new item from the given <c>key</c>, if no existing item was found)</param>
        /// <param name="options">Options</param>
        /// <param name="removeExisting">Remove an existing entry?</param>
        /// <param name="disposeUnused">Dispose a found item, if a newer item was found?</param>
        /// <returns>Cache entry</returns>
        public static ICacheEntry<T>? Get<T>(
            this ImmutableArray<ICache<T>> caches,
            in string key,
            in SemaphoreSync cachesSync,
            in ICache<T>.CacheEntryFactory_Delegate? entryFactory = null,
            in ICacheEntryOptions? options = null,
            in bool removeExisting = true,
            in bool disposeUnused = true
            )
        {
            // Try to find the item in any cache
            int len = caches.Length;
            if (len < 1) return null;
            for (int i = 0; i < len; i++)
            {
                if (caches[i].Get(key) is not ICacheEntry<T> entry || !entry.CanUse) continue;
                // Return the entry from the first cache
                if (i < 1) return entry;
                // Move the entry up in the cache hierarchy
                using SemaphoreSyncContext ssc = cachesSync;
                (_, ICacheEntry<T>? res) = TryStore(caches, i, key, entry.Item, entry.Size, options, removeExisting, disposeUnused);
                if (res is not null && caches[i].TryRemove(key) is ICacheEntry<T> removed && !removed.Item!.Equals(res.Item) && !removed.Item!.Equals(entry.Item))
                    removed.Item.TryDispose();
                return res ?? entry;
            }
            // Use the entry factory
            using SemaphoreSyncContext ssc2 = cachesSync;
            return caches[0].Get(key, entryFactory, options);
        }

        /// <summary>
        /// Get a cache entry
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="caches">Caches (should be synchronized)</param>
        /// <param name="key">Unique cache entry key</param>
        /// <param name="cachesSync">Caches synchronization</param>
        /// <param name="entryFactory">Cache entry factory (used to create a new item from the given <c>key</c>, if no existing item was found)</param>
        /// <param name="options">Options</param>
        /// <param name="removeExisting">Remove an existing entry?</param>
        /// <param name="disposeUnused">Dispose a found item, if a newer item was found?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cache entry</returns>
        public static async Task<ICacheEntry<T>?> GetAsync<T>(
            this ImmutableArray<ICache<T>> caches,
            string key,
            SemaphoreSync cachesSync,
            ICache<T>.CacheEntryFactory_Delegate? entryFactory = null,
            ICacheEntryOptions? options = null,
            bool removeExisting = true,
            bool disposeUnused = true,
            CancellationToken cancellationToken = default
            )
        {
            // Try to find the item in any cache
            int len = caches.Length;
            if (len < 1) return null;
            for (int i = 0; i < len; i++)
            {
                if (await caches[i].GetAsync(key, cancellationToken: cancellationToken).DynamicContext() is not ICacheEntry<T> entry || !entry.CanUse) continue;
                // Return the entry from the first cache
                if (i < 1) return entry;
                // Move the entry up in the cache hierarchy
                entry = await caches[0].AddAsync(key, entry.Item, options, cancellationToken: cancellationToken).DynamicContext();
                using SemaphoreSyncContext ssc = await cachesSync.SyncContextAsync(cancellationToken).DynamicContext();
                (_, ICacheEntry<T>? res) = await TryStoreAsync(caches, i, key, entry.Item, entry.Size, options, removeExisting, disposeUnused).DynamicContext();
                if (res is not null && caches[i].TryRemove(key) is ICacheEntry<T> removed && !removed.Item!.Equals(res.Item) && !removed.Item!.Equals(entry.Item))
                    await removed.Item.TryDisposeAsync().DynamicContext();
                return res ?? entry;
            }
            // Use the entry factory
            using SemaphoreSyncContext ssc2 = await cachesSync.SyncContextAsync(cancellationToken).DynamicContext();
            return caches[0].Get(key, entryFactory, options);
        }

        /// <summary>
        /// Try removing an entry by its key (items will be disposed)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="caches">Caches (should be synchronized)</param>
        /// <param name="key">Unique cache entry key</param>
        /// <returns>Number of removed entries</returns>
        public static int TryRemove<T>(this ImmutableArray<ICache<T>> caches, in string key)
        {
            int res = 0,
                len = caches.Length;
            if (len < 1) return res;
            for (int i = 0; i < len; i++)
            {
                if (caches[i].TryRemove(key) is not ICacheEntry<T> entry) continue;
                entry.Item.TryDispose();
                res++;
            }
            return res;
        }

        /// <summary>
        /// Clear the cache (items will be disposed)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="caches">Caches (should be synchronized)</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Clear<T>(this ImmutableArray<ICache<T>> caches)
        {
            for (int i = 0, len = caches.Length; i < len; caches[i].Clear(disposeItems: true).TryDisposeAll(), i++) ;
        }

        /// <summary>
        /// Clear the cache (items will be disposed)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="caches">Caches (should be synchronized)</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task ClearAsync<T>(this ImmutableArray<ICache<T>> caches)
        {
            for (int i = 0, len = caches.Length; i < len; (await caches[i].ClearAsync(disposeItems: true).DynamicContext()).TryDisposeAll(), i++) ;
        }

        /// <summary>
        /// Try to store the item in the first suitable cache
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="caches">Caches (should be synchronized)</param>
        /// <param name="stopIndex">Cache index to stop before</param>
        /// <param name="key">Item key</param>
        /// <param name="item">Item</param>
        /// <param name="size">Item size</param>
        /// <param name="options">Options</param>
        /// <param name="removeExisting">Remove an existing entry?</param>
        /// <param name="disposeUnused">Dispose the given <c>item</c>, if a newer item was found?</param>
        /// <param name="startIndex">Cache index to start at</param>
        /// <returns>Index of the cache which has this item now (or <c>-1</c>, if not cached), and the cache entry</returns>
        public static (int CacheIndex, ICacheEntry<T>? Entry) TryStore<T>(
            this ImmutableArray<ICache<T>> caches,
            in int stopIndex,
            in string key,
            in T item,
            in int size,
            in ICacheEntryOptions? options = null,
            in bool removeExisting = true,
            in bool disposeUnused = true,
            in int startIndex = 0
            )
        {
            for (int i = startIndex; i < stopIndex; i++)
            {
                if (caches[i].Options.MaxItemSize < size) continue;
                ICacheEntry<T> entry = caches[i].Add(key, item, options, removeExisting, disposeUnused);
                return (i, entry);
            }
            return (-1, Entry: null);
        }

        /// <summary>
        /// Try to store the item in the first suitable cache
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="caches">Caches (should be synchronized)</param>
        /// <param name="stopIndex">Cache index to stop before</param>
        /// <param name="key">Item key</param>
        /// <param name="item">Item</param>
        /// <param name="size">Item size</param>
        /// <param name="options">Options</param>
        /// <param name="removeExisting">Remove an existing entry?</param>
        /// <param name="disposeUnused">Dispose the given <c>item</c>, if a newer item was found?</param>
        /// <param name="startIndex">Cache index to start at</param>
        /// <returns>Index of the cache which has this item now (or <c>-1</c>, if not cached), and the cache entry</returns>
        public static async Task<(int CacheIndex, ICacheEntry<T>? Entry)> TryStoreAsync<T>(
            this ImmutableArray<ICache<T>> caches,
            int stopIndex,
            string key,
            T item,
            int size,
            ICacheEntryOptions? options = null,
            bool removeExisting = true,
            bool disposeUnused = true,
            int startIndex = 0
            )
        {
            for (int i = startIndex; i < stopIndex; i++)
            {
                if (caches[i].Options.MaxItemSize < size) continue;
                ICacheEntry<T> entry = await caches[i].AddAsync(key, item, options, removeExisting, disposeUnused).DynamicContext();
                return (i, entry);
            }
            return (-1, Entry: null);
        }

        /// <summary>
        /// Apply automatic down-moving of cache items which were removed during a cache tidy action
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="caches">Caches</param>
        /// <param name="cachesSync">Caches synchronization</param>
        /// <param name="disposeRemoved">If to dispose finally removed items</param>
        public static void ApplyAutoItemMoving<T>(this ImmutableArray<ICache<T>> caches, SemaphoreSync cachesSync, bool disposeRemoved = true)
        {
            for (int i = 0, len = caches.Length - 1; i < len; i++)
            {
                void HandleRemovedEntry(ICache<T> s, ICache<T>.CacheEntryEventArgs e)
                {
                    try
                    {
                        if (e.Reason != CacheEventReasons.Tidy || (s is IDisposableObject disposable && disposable.IsDisposing)) return;
                        using SemaphoreSyncContext ssc = cachesSync;
                        (int index, ICacheEntry<T>? entry) = TryStore(
                            caches,
                            caches.Length,
                            e.Entry.Key,
                            e.Entry.Item,
                            e.Entry.Size,
                            e.Entry.Options,
                            removeExisting: true,
                            disposeUnused: true,
                            startIndex: i + 1
                            );
                        if (disposeRemoved && entry is null) e.Entry.TryDispose();
                    }
                    catch(Exception ex)
                    {
                        ErrorHandling.Handle(new("Failed to auto-move a removed cache entry down in the caches hierarchy", ex, tag: s));
                    }
                }
                void HandleCacheDisposing(IDisposableObject disposable, EventArgs e)
                {
                    disposable.OnDisposing -= HandleCacheDisposing;
                    ((ICache<T>)disposable).OnCacheEntryRemoved -= HandleRemovedEntry;
                }
                caches[i].OnCacheEntryRemoved += HandleRemovedEntry;
                if (caches[i] is IDisposableObject disposable) disposable.OnDisposing += HandleCacheDisposing;
            }
        }
    }
}

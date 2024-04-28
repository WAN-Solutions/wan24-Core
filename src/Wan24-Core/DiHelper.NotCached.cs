using System.Collections.ObjectModel;

namespace wan24.Core
{
    // Not cached
    public partial class DiHelper
    {
        /// <summary>
        /// Not cached types
        /// </summary>
        private static readonly HashSet<Type> _NotCachedTypes = [];

        /// <summary>
        /// Not cached types
        /// </summary>
        public static ReadOnlyCollection<Type> NotCachedTypes
        {
            get
            {
                using SemaphoreSyncContext ssc = StaticSync.SyncContext();
                return _NotCachedTypes.AsReadOnly();
            }
        }

        /// <summary>
        /// Add not cached types
        /// </summary>
        /// <param name="types">Types</param>
        public static void AddNotCachedTypes(params Type[] types)
        {
            using SemaphoreSyncContext ssc = StaticSync.SyncContext();
            foreach (Type type in types)
            {
                _NotCachedTypes.Add(type);
                if (Objects.TryRemove(type, out object? obj)) obj.TryDispose();
            }
        }

        /// <summary>
        /// Add not cached types
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="types">Types</param>
        public static async Task AddNotCachedTypesAsync(CancellationToken cancellationToken = default, params Type[] types)
        {
            using SemaphoreSyncContext ssc = await StaticSync.SyncContextAsync(cancellationToken).DynamicContext();
            foreach (Type type in types)
            {
                _NotCachedTypes.Add(type);
                if (Objects.TryRemove(type, out object? obj)) await obj.TryDisposeAsync().DynamicContext();
            }
        }

        /// <summary>
        /// Remove a not cached type
        /// </summary>
        /// <param name="type">Type</param>
        public static void RemoveNotCachedType(Type type)
        {
            using SemaphoreSyncContext ssc = StaticSync.SyncContext();
            _NotCachedTypes.Remove(type);
        }

        /// <summary>
        /// Remove a not cached type
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task RemoveNotCachedTypeAsync(Type type, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await StaticSync.SyncContextAsync(cancellationToken).DynamicContext();
            _NotCachedTypes.Remove(type);
        }

        /// <summary>
        /// Determine if an object of the given type will be cached
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Will be cached?</returns>
        public static bool IsTypeCached(Type type)
        {
            using SemaphoreSyncContext ssc = StaticSync.SyncContext();
            return !_NotCachedTypes.Contains(type);
        }

        /// <summary>
        /// Determine if an object of the given type will be cached
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Will be cached?</returns>
        public static async Task<bool> IsTypeCachedAsync(Type type, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await StaticSync.SyncContextAsync(cancellationToken).DynamicContext();
            return !_NotCachedTypes.Contains(type);
        }
    }
}

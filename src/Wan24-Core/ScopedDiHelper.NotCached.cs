﻿using System.Collections.ObjectModel;

namespace wan24.Core
{
    // Not cached
    public partial class ScopedDiHelper
    {
        /// <summary>
        /// Not cached types
        /// </summary>
        protected readonly HashSet<Type> _ScopeNotCachedTypes = [];

        /// <summary>
        /// Not cached types
        /// </summary>
        public ReadOnlyCollection<Type> ScopeNotCachedTypes
        {
            get
            {
                using SemaphoreSyncContext ssc = Sync.SyncContext();
                return _ScopeNotCachedTypes.AsReadOnly();
            }
        }

        /// <summary>
        /// Add not cached types
        /// </summary>
        /// <param name="types">Types</param>
        new public void AddNotCachedTypes(params Type[] types)
        {
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            foreach (Type type in types)
            {
                _ScopeNotCachedTypes.Add(type);
                if (Objects.TryRemove(type, out object? obj)) obj.TryDispose();
            }
        }

        /// <summary>
        /// Add not cached types
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="types">Types</param>
        new public async Task AddNotCachedTypesAsync(CancellationToken cancellationToken = default, params Type[] types)
        {
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            foreach (Type type in types)
            {
                _ScopeNotCachedTypes.Add(type);
                if (Objects.TryRemove(type, out object? obj)) await obj.TryDisposeAsync().DynamicContext();
            }
        }

        /// <summary>
        /// Remove a not cached type
        /// </summary>
        /// <param name="type">Type</param>
        new public void RemoveNotCachedType(Type type)
        {
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            _ScopeNotCachedTypes.Remove(type);
        }

        /// <summary>
        /// Remove a not cached type
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        new public async Task RemoveNotCachedTypeAsync(Type type, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            _ScopeNotCachedTypes.Remove(type);
        }

        /// <summary>
        /// Determine if an object of the given type will be cached
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Will be cached?</returns>
        new public bool IsTypeCached(Type type)
        {
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            return !_ScopeNotCachedTypes.Contains(type) && !DiHelper.IsTypeCached(type);
        }

        /// <summary>
        /// Determine if an object of the given type will be cached
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Will be cached?</returns>
        new public async Task<bool> IsTypeCachedAsync(Type type, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            return !_ScopeNotCachedTypes.Contains(type) && !await DiHelper.IsTypeCachedAsync(type, cancellationToken).DynamicContext();
        }
    }
}

using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Runtime;

//TODO Add support for keyed services

namespace wan24.Core
{
    /// <summary>
    /// Scoped DI helper (will dispose created objects, if possible; don't forget to dispose!)
    /// </summary>
    public partial class ScopedDiHelper : DiHelper
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Objects
        /// </summary>
        protected readonly ConcurrentDictionary<Type, object> ScopeObjects = new();
        /// <summary>
        /// Keyed objects
        /// </summary>
        protected readonly ConcurrentDictionary<object, ConcurrentDictionary<Type, object>> KeyedScopeObjects = new();

        /// <summary>
        /// Constructor
        /// </summary>
        public ScopedDiHelper() : base()
        {
            DisposableAdapter = new(
                (disposing) =>
                {
                    ServiceProvider?.TryDispose();
                    ScopeObjects.Values.TryDisposeAll();
                    ScopeObjects.Clear();
                    foreach (var dict in KeyedScopeObjects.Values)
                    {
                        dict.Values.TryDisposeAll();
                        dict.Clear();
                    }
                    KeyedScopeObjects.Clear();
                    ScopeObjectFactories.Clear();
                    KeyedScopeObjectFactories.Clear();
                    ScopeAsyncObjectFactories.Clear();
                    KeyedScopeAsyncObjectFactories.Clear();
                    Sync.Dispose();
                },
                async () =>
                {
                    if (ServiceProvider is not null) await ServiceProvider.TryDisposeAsync().DynamicContext();
                    await ScopeObjects.Values.TryDisposeAllAsync().DynamicContext();
                    ScopeObjects.Clear();
                    foreach (var dict in KeyedScopeObjects.Values)
                    {
                        await dict.Values.TryDisposeAllAsync().DynamicContext();
                        dict.Clear();
                    }
                    KeyedScopeObjects.Clear();
                    ScopeObjectFactories.Clear();
                    KeyedScopeObjectFactories.Clear();
                    ScopeAsyncObjectFactories.Clear();
                    KeyedScopeAsyncObjectFactories.Clear();
                    await Sync.DisposeAsync().DynamicContext();
                });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceProvider">Service provider (will be disposed, if possible!)</param>
        public ScopedDiHelper(IServiceProvider serviceProvider) : this() => ServiceProvider = serviceProvider;

        /// <summary>
        /// DI object factories
        /// </summary>
        public readonly ConcurrentDictionary<Type, Di_Delegate> ScopeObjectFactories = new();
        /// <summary>
        /// Keyed DI object factories
        /// </summary>
        public readonly ConcurrentDictionary<object, ConcurrentDictionary<Type, Di_Delegate>> KeyedScopeObjectFactories = new();
        /// <summary>
        /// Asynchronous DI object factories
        /// </summary>
        public readonly ConcurrentDictionary<Type, DiAsync_Delegate> ScopeAsyncObjectFactories = new();
        /// <summary>
        /// Keyed asynchronous DI object factories
        /// </summary>
        public readonly ConcurrentDictionary<object, ConcurrentDictionary<Type, DiAsync_Delegate>> KeyedScopeAsyncObjectFactories = new();

        /// <inheritdoc/>
        new public void AddNotCachedTypes(params Type[] types)
        {
            DisposableAdapter.EnsureNotDisposed();
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            foreach (Type type in types)
            {
                DiHelper.AddNotCachedTypes(type);
                if (ScopeObjects.TryRemove(type, out object? obj)) obj.TryDispose();
            }
        }

        /// <inheritdoc/>
        new public async Task AddNotCachedTypesAsync(CancellationToken cancellationToken = default, params Type[] types)
        {
            DisposableAdapter.EnsureNotDisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            foreach (Type type in types)
            {
                await DiHelper.AddNotCachedTypesAsync(cancellationToken, type).DynamicContext();
                if (ScopeObjects.TryRemove(type, out object? obj)) await obj.TryDisposeAsync().DynamicContext();
            }
        }

        /// <inheritdoc/>
        new public T AddDiObject<T>(T obj)
        {
            DisposableAdapter.EnsureNotDisposed();
            Contract.Assert(obj is not null);
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            Type type = obj.GetType();
            if (!IsTypeCached(type)) return obj;
            ScopeObjects.GetOrAdd(type, key => obj);
            return obj;
        }

        /// <inheritdoc/>
        new public async Task<T> AddDiObjectAsync<T>(T obj, CancellationToken cancellationToken = default)
        {
            DisposableAdapter.EnsureNotDisposed();
            Contract.Assert(obj is not null);
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            Type type = obj.GetType();
            if (!await IsTypeCachedAsync(type, cancellationToken).DynamicContext()) return obj;
            ScopeObjects.GetOrAdd(type, key => obj);
            return obj;
        }

        /// <inheritdoc/>
        new public object? RemoveDiObject(Type type)
        {
            DisposableAdapter.EnsureNotDisposed();
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            ScopeObjects.TryGetValue(type, out object? res);
            object? baseObj = DiHelper.RemoveDiObject(type);
            if (res is not null && baseObj is not null) baseObj.TryDispose();
            return res ?? baseObj;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        new public T? RemoveDiObject<T>() => (T?)RemoveDiObject(typeof(T));

        /// <inheritdoc/>
        new public async Task<object?> RemoveDiObjectAsync(Type type, CancellationToken cancellationToken = default)
        {
            DisposableAdapter.EnsureNotDisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            ScopeObjects.TryGetValue(type, out object? res);
            object? baseObj = await DiHelper.RemoveDiObjectAsync(type, cancellationToken).DynamicContext();
            if (res is not null && baseObj is not null) await baseObj.TryDisposeAsync().DynamicContext();
            return res ?? baseObj;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        new public async Task<T?> RemoveDiObjectAsync<T>(CancellationToken cancellationToken = default)
            => (T?)await RemoveDiObjectAsync(typeof(T), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        new public void ClearObjectCache()
        {
            DisposableAdapter.EnsureNotDisposed();
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            object[] objects = [.. ScopeObjects.Values];
            ScopeObjects.Clear();
            objects.TryDisposeAll();
        }

        /// <inheritdoc/>
        new public void ClearKeyedObjectCache(object key)
        {
            DisposableAdapter.EnsureNotDisposed();
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            if (!KeyedScopeObjects.TryGetValue(key, out var dict)) return;
            object[] objects = [.. dict.Values];
            dict.Clear();
            KeyedScopeObjects.TryRemove(key, out _);
            objects.TryDisposeAll();
        }

        /// <inheritdoc/>
        new public async Task ClearObjectCacheAsync(CancellationToken cancellationToken = default)
        {
            DisposableAdapter.EnsureNotDisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            object[] objects = [.. ScopeObjects.Values];
            ScopeObjects.Clear();
            await objects.TryDisposeAllAsync().DynamicContext();
        }

        /// <inheritdoc/>
        new public async Task ClearKeyedObjectCacheAsync(object key, CancellationToken cancellationToken = default)
        {
            DisposableAdapter.EnsureNotDisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            if (!KeyedScopeObjects.TryGetValue(key, out var dict)) return;
            object[] objects = [.. dict.Values];
            dict.Clear();
            KeyedScopeObjects.TryRemove(key, out _);
            await objects.TryDisposeAllAsync().DynamicContext();
        }
    }
}

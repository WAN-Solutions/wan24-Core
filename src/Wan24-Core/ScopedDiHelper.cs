using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime;

//TODO Add support for keyed services

namespace wan24.Core
{
    /// <summary>
    /// Scoped DI helper (will dispose created objects, if possible; don't forget to dispose!)
    /// </summary>
    public class ScopedDiHelper : DiHelper, IDisposableObject
    {
        /// <summary>
        /// Disposable adapter
        /// </summary>
        protected readonly DisposableAdapter DisposableAdapter;
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Objects (key is the type hash code)
        /// </summary>
        protected readonly ConcurrentDictionary<int, object> ScopeObjects = new();

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
                    ScopeObjectFactories.Clear();
                    ScopeAsyncObjectFactories.Clear();
                    Sync.Dispose();
                },
                async () =>
                {
                    if (ServiceProvider is not null) await ServiceProvider.TryDisposeAsync().DynamicContext();
                    await ScopeObjects.Values.TryDisposeAllAsync().DynamicContext();
                    ScopeObjects.Clear();
                    ScopeObjectFactories.Clear();
                    ScopeAsyncObjectFactories.Clear();
                    await Sync.DisposeAsync().DynamicContext();
                });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceProvider">Service provider (will be disposed, if possible!)</param>
        public ScopedDiHelper(IServiceProvider serviceProvider) : this() => ServiceProvider = serviceProvider;

        /// <summary>
        /// Destructor
        /// </summary>
        ~ScopedDiHelper() => DisposableAdapter.DisposeFromDestructor();

        /// <summary>
        /// DI object factories
        /// </summary>
        public readonly ConcurrentDictionary<Type, Di_Delegate> ScopeObjectFactories = new();
        /// <summary>
        /// Asynchronous DI object factories
        /// </summary>
        public readonly ConcurrentDictionary<Type, DiAsync_Delegate> ScopeAsyncObjectFactories = new();

        /// <summary>
        /// DI service provider (will be disposed!)
        /// </summary>
        new public IServiceProvider? ServiceProvider { get; set; }

        /// <inheritdoc/>
        public bool IsDisposing => DisposableAdapter.IsDisposing;

        /// <inheritdoc/>
        public bool IsDisposed => DisposableAdapter.IsDisposed;

        /// <inheritdoc/>
        public override object? GetService(Type serviceType)
            => DisposableAdapter.IsDisposing
                ? throw new ObjectDisposedException(GetType().ToString())
                : GetDiObject(serviceType, out object? res) ? res : null;

        /// <inheritdoc/>
        public override async Task<object?> GetServiceAsync(Type serviceType, CancellationToken cancellationToken = default)
            => DisposableAdapter.IsDisposing
                ? throw new ObjectDisposedException(GetType().ToString())
                : (await GetDiObjectAsync(serviceType, serviceProvider: null, cancellationToken).DynamicContext()).Object;

        /// <inheritdoc/>
        public override bool IsService(Type serviceType)
            => DisposableAdapter.IsDisposing
                ? throw new ObjectDisposedException(GetType().ToString())
                : ((ServiceProvider as IServiceProviderIsService)?.IsService(serviceType) ?? false) ||
                GetFactory(serviceType) is not null ||
                GetAsyncFactory(serviceType) is not null;

        /// <inheritdoc/>
        new public void AddNotCachedTypes(params Type[] types)
        {
            ObjectDisposedException.ThrowIf(DisposableAdapter.IsDisposing, this);
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            foreach (Type type in types)
            {
                DiHelper.AddNotCachedTypes(type);
                if (ScopeObjects.TryRemove(type.GetHashCode(), out object? obj)) obj.TryDispose();
            }
        }

        /// <inheritdoc/>
        new public async Task AddNotCachedTypesAsync(CancellationToken cancellationToken = default, params Type[] types)
        {
            ObjectDisposedException.ThrowIf(DisposableAdapter.IsDisposing, this);
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            foreach (Type type in types)
            {
                await DiHelper.AddNotCachedTypesAsync(cancellationToken, type).DynamicContext();
                if (ScopeObjects.TryRemove(type.GetHashCode(), out object? obj)) await obj.TryDisposeAsync().DynamicContext();
            }
        }

        /// <inheritdoc/>
        new public T AddDiObject<T>(T obj)
        {
            ObjectDisposedException.ThrowIf(DisposableAdapter.IsDisposing, this);
            Contract.Assert(obj is not null);
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            Type type = obj.GetType();
            if (!IsTypeCached(type)) return obj;
            ScopeObjects.GetOrAdd(type.GetHashCode(), key => obj);
            return obj;
        }

        /// <inheritdoc/>
        new public async Task<T> AddDiObjectAsync<T>(T obj, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(DisposableAdapter.IsDisposing, this);
            Contract.Assert(obj is not null);
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            Type type = obj.GetType();
            if (!await IsTypeCachedAsync(type, cancellationToken).DynamicContext()) return obj;
            ScopeObjects.GetOrAdd(type.GetHashCode(), key => obj);
            return obj;
        }

        /// <inheritdoc/>
        new public object? RemoveDiObject(int type)
        {
            ObjectDisposedException.ThrowIf(DisposableAdapter.IsDisposing, this);
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            ScopeObjects.TryGetValue(type, out object? res);
            object? baseObj = DiHelper.RemoveDiObject(type);
            if (res is not null && baseObj is not null) baseObj.TryDispose();
            return res ?? baseObj;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        new public object? RemoveDiObject(Type type) => RemoveDiObject(type.GetHashCode());

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        new public T? RemoveDiObject<T>() => (T?)RemoveDiObject(typeof(T).GetHashCode());

        /// <inheritdoc/>
        new public async Task<object?> RemoveDiObjectAsync(int type, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(DisposableAdapter.IsDisposing, this);
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            ScopeObjects.TryGetValue(type, out object? res);
            object? baseObj = await DiHelper.RemoveDiObjectAsync(type, cancellationToken).DynamicContext();
            if (res is not null && baseObj is not null) await baseObj.TryDisposeAsync().DynamicContext();
            return res ?? baseObj;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        new public Task<object?> RemoveDiObjectAsync(Type type, CancellationToken cancellationToken = default)
            => RemoveDiObjectAsync(type.GetHashCode(), cancellationToken);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        new public async Task<T?> RemoveDiObjectAsync<T>(CancellationToken cancellationToken = default)
            => (T?)await RemoveDiObjectAsync(typeof(T).GetHashCode(), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        new public void ClearObjectCache()
        {
            ObjectDisposedException.ThrowIf(DisposableAdapter.IsDisposing, this);
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            object[] objects = [.. ScopeObjects.Values];
            ScopeObjects.Clear();
            objects.TryDisposeAll();
        }

        /// <inheritdoc/>
        new public async Task ClearObjectCacheAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(DisposableAdapter.IsDisposing, this);
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            object[] objects = [.. ScopeObjects.Values];
            ScopeObjects.Clear();
            await objects.TryDisposeAllAsync().DynamicContext();
        }

        /// <inheritdoc/>
        new public bool GetDiObject(in Type type, [NotNullWhen(returnValue: true)] out object? obj, IServiceProvider? serviceProvider = null)
        {
            ObjectDisposedException.ThrowIf(DisposableAdapter.IsDisposing, this);
            int typeHashCode = type.GetHashCode();
            if (ScopeObjects.TryGetValue(typeHashCode, out obj)) return true;
            obj = serviceProvider?.GetService(type) ?? ServiceProvider?.GetService(type);
            if (obj is not null)
            {
                if(IsTypeCached(type)) ScopeObjects.TryAdd(typeHashCode, obj);
                return true;
            }
            obj = DiHelper.ServiceProvider?.GetService(type);
            if (obj is not null) return true;
            if (GetFactoryInt(type) is Di_Delegate factory && factory(type, out obj))
            {
                if (IsTypeCached(type)) ScopeObjects.TryAdd(typeHashCode, obj);
                return true;
            }
            if (DiHelper.GetFactory(type) is Di_Delegate baseFactory && baseFactory(type, out obj)) return true;
            return false;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        new public bool GetDiObject<T>([NotNullWhen(returnValue: true)] out T? obj, IServiceProvider? serviceProvider = null)
        {
            obj = GetDiObject(typeof(T), out object? res, serviceProvider) ? (T)res : default;
            return obj is not null;
        }

        /// <inheritdoc/>
        new public async Task<AsyncResult> GetDiObjectAsync(Type type, IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            if (GetDiObject(type, out object? res, serviceProvider)) return new(res, true);
            if (GetAsyncFactoryInt(type) is DiAsync_Delegate factory)
            {
                AsyncResult result = await factory(type, cancellationToken).DynamicContext();
                if (result.Use)
                {
                    if (await IsTypeCachedAsync(type, cancellationToken).DynamicContext()) ScopeObjects.TryAdd(type.GetHashCode(), result.Object);
                    return result;
                }
            }
            if (DiHelper.GetAsyncFactory(type) is DiAsync_Delegate baseFactory)
            {
                AsyncResult result = await baseFactory(type, cancellationToken).DynamicContext();
                if (result.Use) return result;
            }
            return new(res, false);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        new public async Task<AsyncResult<T>> GetDiObjectAsync<T>(IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default)
            => (await GetDiObjectAsync(typeof(T), serviceProvider, cancellationToken).DynamicContext()).GetGeneric<T>();

        /// <inheritdoc/>
        new public Di_Delegate? GetFactory(Type type)
            => DisposableAdapter.IsDisposing
                ? throw new ObjectDisposedException(GetType().ToString())
                : GetFactoryInt(type) is Di_Delegate res
                    ? res
                    : DiHelper.GetFactory(type);

        /// <inheritdoc/>
        new public DiAsync_Delegate? GetAsyncFactory(Type type)
            => DisposableAdapter.IsDisposing
                ? throw new ObjectDisposedException(GetType().ToString())
                : GetAsyncFactoryInt(type) is DiAsync_Delegate res
                    ? res
                    : DiHelper.GetAsyncFactory(type);

        /// <inheritdoc/>
        public void Dispose()
        {
            DisposableAdapter.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await DisposableAdapter.DisposeAsync().DynamicContext();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Get an object factory
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Factory</returns>
        protected virtual Di_Delegate? GetFactoryInt(Type type)
        {
            ObjectDisposedException.ThrowIf(DisposableAdapter.IsDisposing, this);
            if (ScopeObjectFactories.TryGetValue(type, out Di_Delegate? res)) return res;
            res = (from kvp in ScopeObjectFactories
                   where type.IsAssignableFrom(kvp.Key)
                   select kvp.Value)
                    .FirstOrDefault();
            if (res is not null || !type.IsGenericType || type.IsGenericTypeDefinition) return res;
            type = type.GetGenericTypeDefinition();
            return (from kvp in ScopeObjectFactories
                    where kvp.Key.IsGenericTypeDefinition &&
                        type.IsAssignableFrom(kvp.Key)
                    select kvp.Value)
                    .FirstOrDefault();
        }

        /// <summary>
        /// Get an object factory
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Factory</returns>
        protected virtual DiAsync_Delegate? GetAsyncFactoryInt(Type type)
        {
            ObjectDisposedException.ThrowIf(DisposableAdapter.IsDisposing, this);
            if (ScopeAsyncObjectFactories.TryGetValue(type, out DiAsync_Delegate? res)) return res;
            res = (from kvp in ScopeAsyncObjectFactories
                   where type.IsAssignableFrom(kvp.Key)
                   select kvp.Value)
                    .FirstOrDefault();
            if (res is not null || !type.IsGenericType || type.IsGenericTypeDefinition) return res;
            type = type.GetGenericTypeDefinition();
            return (from kvp in ScopeAsyncObjectFactories
                    where kvp.Key.IsGenericTypeDefinition &&
                        type.IsAssignableFrom(kvp.Key)
                    select kvp.Value)
                    .FirstOrDefault();
        }

        /// <inheritdoc/>
        public event IDisposableObject.Dispose_Delegate? OnDisposing
        {
            add => DisposableAdapter.OnDisposing += value;
            remove => DisposableAdapter.OnDisposing -= value;
        }

        /// <inheritdoc/>
        public event IDisposableObject.Dispose_Delegate? OnDisposed
        {
            add => DisposableAdapter.OnDisposed += value;
            remove => DisposableAdapter.OnDisposed -= value;
        }
    }
}

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime;

namespace wan24.Core
{
    // Static
    public partial class DiHelper
    {
        /// <summary>
        /// Static thread synchronization
        /// </summary>
        protected static readonly SemaphoreSync StaticSync = new();
        /// <summary>
        /// Objects (key is the type hashcode)
        /// </summary>
        protected static readonly ConcurrentDictionary<int, object> Objects = new();
        /// <summary>
        /// Not cached types
        /// </summary>
        private static readonly HashSet<Type> _NotCachedTypes = [];
        /// <summary>
        /// Default instance
        /// </summary>
        private static DiHelper? _Instance;
        /// <summary>
        /// Custom instance
        /// </summary>
        private static DiHelper? CustomInstance = null;
        /// <summary>
        /// DI object factories
        /// </summary>
        public static readonly ConcurrentDictionary<Type, Di_Delegate> ObjectFactories = new();
        /// <summary>
        /// Asynchronous DI object factories
        /// </summary>
        public static readonly ConcurrentDictionary<Type, DiAsync_Delegate> AsyncObjectFactories = new();

        /// <summary>
        /// Global singleton instance
        /// </summary>
        public static DiHelper Instance
        {
            get => CustomInstance ?? (_Instance ??= new());
            set => CustomInstance = value;
        }

        /// <summary>
        /// DI service provider
        /// </summary>
        public static IServiceProvider? ServiceProvider { get; set; }

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
                if (Objects.TryRemove(type.GetHashCode(), out object? obj)) obj.TryDispose();
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
                if (Objects.TryRemove(type.GetHashCode(), out object? obj)) await obj.TryDisposeAsync().DynamicContext();
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

        /// <summary>
        /// Add a DI object to the cache
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Object</returns>
        public static T AddDiObject<T>(T obj)
        {
            Contract.Assert(obj is not null);
            using SemaphoreSyncContext ssc = StaticSync.SyncContext();
            Type type = obj.GetType();
            if (_NotCachedTypes.Contains(type)) return obj;
            Objects.GetOrAdd(type.GetHashCode(), key => obj);
            return obj;
        }

        /// <summary>
        /// Add a DI object to the cache
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        public static async Task<T> AddDiObjectAsync<T>(T obj, CancellationToken cancellationToken = default)
        {
            Contract.Assert(obj is not null);
            using SemaphoreSyncContext ssc = await StaticSync.SyncContextAsync(cancellationToken).DynamicContext();
            Type type = obj.GetType();
            if (_NotCachedTypes.Contains(type)) return obj;
            Objects.GetOrAdd(type.GetHashCode(), key => obj);
            return obj;
        }

        /// <summary>
        /// Remove a DI object from the cache
        /// </summary>
        /// <param name="type">Type hash code</param>
        /// <returns>Object</returns>
        public static object? RemoveDiObject(int type)
        {
            using SemaphoreSyncContext ssc = StaticSync.SyncContext();
            return Objects.TryGetValue(type, out object? res) ? res : null;
        }

        /// <summary>
        /// Remove a DI object from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object? RemoveDiObject(Type type) => RemoveDiObject(type.GetHashCode());

        /// <summary>
        /// Remove a DI object from the cache
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static T? RemoveDiObject<T>() => (T?)RemoveDiObject(typeof(T).GetHashCode());

        /// <summary>
        /// Remove a DI object from the cache
        /// </summary>
        /// <param name="type">Type hash code</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        public static async Task<object?> RemoveDiObjectAsync(int type, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await StaticSync.SyncContextAsync(cancellationToken).DynamicContext();
            return Objects.TryGetValue(type, out object? res) ? res : null;
        }

        /// <summary>
        /// Remove a DI object from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static Task<object?> RemoveDiObjectAsync(Type type, CancellationToken cancellationToken = default)
            => RemoveDiObjectAsync(type.GetHashCode(), cancellationToken);

        /// <summary>
        /// Remove a DI object from the cache
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static async Task<T?> RemoveDiObjectAsync<T>(CancellationToken cancellationToken = default)
            => (T?)await RemoveDiObjectAsync(typeof(T).GetHashCode(), cancellationToken).DynamicContext();

        /// <summary>
        /// Clear the object cache (will dispose cached objects, if possible)
        /// </summary>
        public static void ClearObjectCache()
        {
            using SemaphoreSyncContext ssc = StaticSync.SyncContext();
            object[] objects = [.. Objects.Values];
            Objects.Clear();
            objects.TryDisposeAll();
        }

        /// <summary>
        /// Clear the object cache (will dispose cached objects, if possible)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task ClearObjectCacheAsync(CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await StaticSync.SyncContextAsync(cancellationToken).DynamicContext();
            object[] objects = [.. Objects.Values];
            Objects.Clear();
            await objects.TryDisposeAllAsync().DynamicContext();
        }

        /// <summary>
        /// Get a DI object
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="obj">Result</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <returns>Use the result?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool GetDiObject(in Type type, [NotNullWhen(returnValue: true)] out object? obj, IServiceProvider? serviceProvider = null)
        {
            if (Objects.TryGetValue(type.GetHashCode(), out obj)) return true;
            obj = serviceProvider?.GetService(type) ?? ServiceProvider?.GetService(type);
            if (obj is not null)
            {
                AddDiObject(obj);
                return true;
            }
            if (GetFactory(type) is Di_Delegate factory && factory(type, out obj))
            {
                AddDiObject(type);
                return true;
            }
            if (GetAsyncFactory(type) is DiAsync_Delegate asyncFactory)
            {
                obj = asyncFactory(type, default).GetAwaiter().GetResult().Object;
                if (obj is not null)
                {
                    AddDiObject(obj);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get a DI object
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="obj">Result</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <returns>Use the result?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool GetDiObject<T>([NotNullWhen(returnValue: true)] out T? obj, IServiceProvider? serviceProvider = null)
        {
            obj = GetDiObject(typeof(T), out object? res, serviceProvider) ? (T)res : default;
            return obj is not null;
        }

        /// <summary>
        /// Get a DI object
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<AsyncResult> GetDiObjectAsync(Type type, IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default)
        {
            if (GetDiObject(type, out object? res, serviceProvider)) return new(res, true);
            if (GetAsyncFactory(type) is DiAsync_Delegate factory)
            {
                AsyncResult result = await factory(type, cancellationToken).DynamicContext();
                if (result.Use)
                {
                    await AddDiObjectAsync(result.Object, cancellationToken).DynamicContext();
                    return result;
                }
            }
            return new(res, false);
        }

        /// <summary>
        /// Get a DI object
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<AsyncResult<T>> GetDiObjectAsync<T>(IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default)
            => (await GetDiObjectAsync(typeof(T), serviceProvider, cancellationToken).DynamicContext()).GetGeneric<T>();

        /// <summary>
        /// Get an object factory
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Factory</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static Di_Delegate? GetFactory(Type type)
        {
            if (ObjectFactories.TryGetValue(type, out Di_Delegate? res)) return res;
            res = (from kvp in ObjectFactories
                   where type.IsAssignableFrom(kvp.Key)
                   select kvp.Value)
                    .FirstOrDefault();
            if (res is not null || !type.IsGenericType || type.IsGenericTypeDefinition) return res;
            type = type.GetGenericTypeDefinition();
            return (from kvp in ObjectFactories
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
        [TargetedPatchingOptOut("Tiny method")]
        public static DiAsync_Delegate? GetAsyncFactory(Type type)
        {
            if (AsyncObjectFactories.TryGetValue(type, out DiAsync_Delegate? res)) return res;
            res = (from kvp in AsyncObjectFactories
                   where type.IsAssignableFrom(kvp.Key)
                   select kvp.Value)
                    .FirstOrDefault();
            if (res is not null || !type.IsGenericType || type.IsGenericTypeDefinition) return res;
            type = type.GetGenericTypeDefinition();
            return (from kvp in AsyncObjectFactories
                    where kvp.Key.IsGenericTypeDefinition &&
                        type.IsAssignableFrom(kvp.Key)
                    select kvp.Value)
                    .FirstOrDefault();
        }
    }
}

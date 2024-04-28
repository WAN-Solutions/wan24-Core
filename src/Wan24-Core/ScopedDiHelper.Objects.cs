using System.Diagnostics.Contracts;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Objects
    public partial class ScopedDiHelper
    {
        /// <summary>
        /// Add a DI object to the cache
        /// </summary>
        /// <typeparam name="T">DI type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public T AddDiObject<T>(T obj)
        {
            Contract.Assert(obj is not null);
            AddDiObject(obj, typeof(T));
            return obj;
        }

        /// <summary>
        /// Add a DI object to the cache
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="type">DI type</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public object AddDiObject(object obj, Type type)
        {
            Contract.Assert(obj is not null);
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            Type objType = obj.GetType();
            if (_ScopeNotCachedTypes.Contains(objType) || !IsTypeCached(objType)) return obj;
            ScopeObjects.GetOrAdd(type, key => obj);
            return obj;
        }

        /// <summary>
        /// Add a keyed DI object to the cache
        /// </summary>
        /// <typeparam name="T">DI type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="obj">Object</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public T AddKeyedDiObject<T>(object key, T obj)
        {
            Contract.Assert(obj is not null);
            AddKeyedDiObject(key, obj, typeof(T));
            return obj;
        }

        /// <summary>
        /// Add a keyed DI object to the cache
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="obj">Object</param>
        /// <param name="type">DI type</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public object AddKeyedDiObject(object key, object obj, Type type)
        {
            Contract.Assert(obj is not null);
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            Type objType = obj.GetType();
            if (_ScopeNotCachedTypes.Contains(objType) || !IsTypeCached(objType)) return obj;
            var dict = KeyedScopeObjects.GetOrAdd(key, _ => []);
            dict.GetOrAdd(type, key => obj);
            return obj;
        }

        /// <summary>
        /// Add a DI object to the cache
        /// </summary>
        /// <typeparam name="T">DI type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<T> AddDiObjectAsync<T>(T obj, CancellationToken cancellationToken = default)
        {
            Contract.Assert(obj is not null);
            await AddDiObjectAsync(obj, typeof(T), cancellationToken).DynamicContext();
            return obj;
        }

        /// <summary>
        /// Add a DI object to the cache
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="type">DI type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<object> AddDiObjectAsync(object obj, Type type, CancellationToken cancellationToken = default)
        {
            Contract.Assert(obj is not null);
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            Type objType = obj.GetType();
            if (_ScopeNotCachedTypes.Contains(objType) || !IsTypeCached(objType)) return obj;
            ScopeObjects.GetOrAdd(type, key => obj);
            return obj;
        }

        /// <summary>
        /// Add a keyed DI object to the cache
        /// </summary>
        /// <typeparam name="T">DI type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="obj">Object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<T> AddKeyedDiObjectAsync<T>(object key, T obj, CancellationToken cancellationToken = default)
        {
            Contract.Assert(obj is not null);
            await AddKeyedDiObjectAsync(key, obj, typeof(T), cancellationToken).DynamicContext();
            return obj;
        }

        /// <summary>
        /// Add a keyed DI object to the cache
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="obj">Object</param>
        /// <param name="type">DI type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<object> AddKeyedDiObjectAsync(object key, object obj, Type type, CancellationToken cancellationToken = default)
        {
            Contract.Assert(obj is not null);
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            Type objType = obj.GetType();
            if (_ScopeNotCachedTypes.Contains(objType) || !IsTypeCached(objType)) return obj;
            var dict = KeyedScopeObjects.GetOrAdd(key, _ => []);
            dict.GetOrAdd(type, key => obj);
            return obj;
        }

        /// <summary>
        /// Remove a DI object from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Object (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public object? RemoveDiObject(Type type)
        {
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            return ScopeObjects.TryRemove(type, out object? res) ? res : null;
        }

        /// <summary>
        /// Remove a DI object from the cache
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Type</param>
        /// <returns>Object (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public object? RemoveKeyedDiObject(object key, Type type)
        {
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            return KeyedScopeObjects.TryGetValue(key, out var dict) && dict.TryRemove(type, out object? res) ? res : null;
        }

        /// <summary>
        /// Remove a DI object from the cache
        /// </summary>
        /// <typeparam name="T">DI type</typeparam>
        /// <returns>Object (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public T? RemoveDiObject<T>() => (T?)RemoveDiObject(typeof(T));

        /// <summary>
        /// Remove a keyed DI object from the cache
        /// </summary>
        /// <typeparam name="T">DI type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Object (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public T? RemoveKeyedDiObject<T>(object key) => (T?)RemoveKeyedDiObject(key, typeof(T));

        /// <summary>
        /// Remove a DI object from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<object?> RemoveDiObjectAsync(Type type, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            return ScopeObjects.TryGetValue(type, out object? res) ? res : null;
        }

        /// <summary>
        /// Remove a DI object from the cache
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<object?> RemoveKeyedDiObjectAsync(object key, Type type, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            return KeyedScopeObjects.TryGetValue(key, out var dict) && dict.TryRemove(type, out object? res) ? res : null;
        }

        /// <summary>
        /// Remove a DI object from the cache
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<T?> RemoveDiObjectAsync<T>(CancellationToken cancellationToken = default)
            => (T?)await RemoveDiObjectAsync(typeof(T), cancellationToken).DynamicContext();

        /// <summary>
        /// Remove a DI object from the cache
        /// </summary>
        /// <typeparam name="T">DI type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<T?> RemoveKeyedDiObjectAsync<T>(object key, CancellationToken cancellationToken = default)
            => (T?)await RemoveKeyedDiObjectAsync(key, typeof(T), cancellationToken).DynamicContext();

        /// <summary>
        /// Remove all DI objects from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Objects (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public object[] RemoveDiObjects(Type type)
        {
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            List<object> res = [];
            foreach (Type t in ScopeObjects.Keys)
                if (type.IsAssignableFromExt(t) && ScopeObjects.TryRemove(t, out object? obj))
                    res.Add(obj);
            return [.. res];
        }

        /// <summary>
        /// Remove all DI objects from the cache
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Type</param>
        /// <returns>Objects (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public object[] RemoveKeyedDiObjects(object key, Type type)
        {
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            if (!KeyedScopeObjects.TryGetValue(key, out var dict)) return [];
            List<object> res = [];
            foreach (Type t in dict.Keys)
                if (type.IsAssignableFromExt(t) && dict.TryRemove(t, out object? obj))
                    res.Add(obj);
            return [.. res];
        }

        /// <summary>
        /// Remove a DI object from the cache
        /// </summary>
        /// <typeparam name="T">DI type</typeparam>
        /// <returns>Object (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public T[] RemoveDiObjects<T>() => [.. RemoveDiObjects(typeof(T)).Select(o => (T)o)];

        /// <summary>
        /// Remove a keyed DI object from the cache
        /// </summary>
        /// <typeparam name="T">DI type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Object (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public T[] RemoveKeyedDiObjects<T>(object key) => [.. RemoveKeyedDiObjects(key, typeof(T)).Select(o => (T)o)];

        /// <summary>
        /// Remove all DI objects from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<object[]> RemoveDiObjectsAsync(Type type, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            List<object> res = [];
            foreach (Type t in ScopeObjects.Keys)
                if (type.IsAssignableFromExt(t) && Objects.TryRemove(t, out object? obj))
                    res.Add(obj);
            return [.. res];
        }

        /// <summary>
        /// Remove all DI objects from the cache
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<object[]> RemoveKeyedDiObjectsAsync(object key, Type type, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            if (!KeyedScopeObjects.TryGetValue(key, out var dict)) return [];
            List<object> res = [];
            foreach (Type t in dict.Keys)
                if (type.IsAssignableFromExt(t) && dict.TryRemove(t, out object? obj))
                    res.Add(obj);
            return [.. res];
        }

        /// <summary>
        /// Remove all DI objects from the cache
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Objects (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<T[]> RemoveDiObjectsAsync<T>(CancellationToken cancellationToken = default)
            => [.. (await RemoveDiObjectsAsync(typeof(T), cancellationToken).DynamicContext()).Select(o => (T)o)];

        /// <summary>
        /// Remove all DI objects from the cache
        /// </summary>
        /// <typeparam name="T">DI type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Objects (needs to be disposed, if applicable!)</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<T[]> RemoveKeyedDiObjectsAsync<T>(object key, CancellationToken cancellationToken = default)
            => [.. (await RemoveKeyedDiObjectsAsync(key, typeof(T), cancellationToken).DynamicContext()).Select(o => (T)o)];

        /// <summary>
        /// Clear the object cache (will dispose cached objects, if possible)
        /// </summary>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public void ClearObjectCache()
        {
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            object[] objects = [.. ScopeObjects.Values];
            Objects.Clear();
            objects.TryDisposeAll();
        }

        /// <summary>
        /// Clear the keyed object cache (will dispose cached objects, if possible)
        /// </summary>
        /// <param name="key">Key</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public void ClearKeyedObjectCache(object key)
        {
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            if (!KeyedScopeObjects.TryGetValue(key, out var dict)) return;
            object[] objects = [.. dict.Values];
            dict.Clear();
            KeyedScopeObjects.TryRemove(key, out _);
            objects.TryDisposeAll();
        }

        /// <summary>
        /// Clear the object cache (will dispose cached objects, if possible)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task ClearObjectCacheAsync(CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            object[] objects = [.. ScopeObjects.Values];
            ScopeObjects.Clear();
            await objects.TryDisposeAllAsync().DynamicContext();
        }

        /// <summary>
        /// Clear the object cache (will dispose cached objects, if possible)
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task ClearKeyedObjectCacheAsync(object key, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            if (!KeyedScopeObjects.TryGetValue(key, out var dict)) return;
            object[] objects = [.. dict.Values];
            dict.Clear();
            KeyedScopeObjects.TryRemove(key, out _);
            await objects.TryDisposeAllAsync().DynamicContext();
        }
    }
}

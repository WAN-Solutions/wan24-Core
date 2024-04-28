using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Getter
    public partial class ScopedDiHelper
    {
        /// <summary>
        /// Get a DI object
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="obj">Result</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <returns>Use the result?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        new public bool GetDiObject(in Type type, [NotNullWhen(returnValue: true)] out object? obj, in IServiceProvider? serviceProvider = null)
        {
            obj = GetObject(type) ?? serviceProvider?.GetService(type) ?? ServiceProvider?.GetService(type);
            if (obj is not null) return true;
            if (GetFactory(type) is Di_Delegate factory && factory(type, out obj))
            {
                AddDiObject(type);
                return true;
            }
            if (GetAsyncFactory(type) is DiAsync_Delegate asyncFactory)
            {
                ITryAsyncResult result = asyncFactory(type, default).GetAwaiter().GetResult();
                if (result.Succeed)
                {
                    obj = result.Result;
                    AddDiObject(obj);
                    return true;
                }
            }
            return DiHelper.GetDiObject(type, out obj, serviceProvider);
        }

        /// <summary>
        /// Get a DI object
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Type</param>
        /// <param name="obj">Result</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="tryAll">Try all sources (also unkeyed)?</param>
        /// <returns>Use the result?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        new public bool GetKeyedDiObject(
            in object key,
            in Type type,
            [NotNullWhen(returnValue: true)] out object? obj,
            in IServiceProvider? serviceProvider = null,
            in bool tryAll = true
            )
        {
            if (!KeyedObjects.TryGetValue(key, out var dict))
                return DiHelper.GetKeyedDiObject(key, type, out obj, serviceProvider, tryAll);
            obj = GetKeyedObject(key, type);
            if (obj is not null) return true;
            if (serviceProvider is IKeyedServiceProvider keyedServiceProvider)
            {
                obj = keyedServiceProvider.GetKeyedService(type, key);
                if (obj is not null) return true;
            }
            if (ServiceProvider is IKeyedServiceProvider keyedServiceProvider2)
            {
                obj = keyedServiceProvider2.GetKeyedService(type, key);
                if (obj is not null) return true;
            }
            if (tryAll)
            {
                obj = GetObject(type) ?? serviceProvider?.GetService(type) ?? ServiceProvider?.GetService(type);
                if (obj is not null) return true;
            }
            if (GetKeyedFactory(key, type) is Di_Delegate keyedFactory && keyedFactory(type, out obj))
            {
                AddKeyedDiObject(key, obj);
                return true;
            }
            if (GetKeyedAsyncFactory(key, type) is DiAsync_Delegate keyedAsyncFactory)
            {
                ITryAsyncResult result = keyedAsyncFactory(type, default).GetAwaiter().GetResult();
                if (result.Succeed)
                {
                    obj = result.Result;
                    AddKeyedDiObject(key, obj);
                    return true;
                }
            }
            if (tryAll)
            {
                if (GetFactory(type) is Di_Delegate factory && factory(type, out obj))
                {
                    AddDiObject(type);
                    return true;
                }
                if (GetAsyncFactory(type) is DiAsync_Delegate asyncFactory)
                {
                    ITryAsyncResult result = asyncFactory(type, default).GetAwaiter().GetResult();
                    if (result.Succeed)
                    {
                        obj = result.Result;
                        AddDiObject(obj);
                        return true;
                    }
                }
            }
            return DiHelper.GetKeyedDiObject(key, type, out obj, serviceProvider, tryAll);
        }

        /// <summary>
        /// Get a DI object
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="obj">Result</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <returns>Use the result?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public bool GetDiObject<T>([NotNullWhen(returnValue: true)] out T? obj, in IServiceProvider? serviceProvider = null)
        {
            if (!GetDiObject(typeof(T), out object? res, serviceProvider))
            {
                obj = default;
                return false;
            }
            obj = (T)res;
            return true;
        }

        /// <summary>
        /// Get a DI object
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="obj">Result</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="tryAll">Try all sources (also unkeyed)?</param>
        /// <returns>Use the result?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public bool GetKeyedDiObject<T>(
            in object key,
            [NotNullWhen(returnValue: true)] out T? obj,
            in IServiceProvider? serviceProvider = null,
            in bool tryAll = true
            )
        {
            if (!GetKeyedDiObject(key, typeof(T), out object? res, serviceProvider, tryAll))
            {
                obj = default;
                return false;
            }
            obj = (T)res;
            return true;
        }

        /// <summary>
        /// Get a DI object
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<ITryAsyncResult> GetDiObjectAsync(Type type, IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default)
        {
            object? res = GetObject(type) ?? serviceProvider?.GetService(type) ?? ServiceProvider?.GetService(type);
            if (res is not null) return new TryAsyncResult<object>(res, succeed: true);
            if (GetAsyncFactory(type) is DiAsync_Delegate asyncFactory)
            {
                ITryAsyncResult result = await asyncFactory(type, cancellationToken).DynamicContext();
                if (result.Succeed)
                {
                    await AddDiObjectAsync(result.Result!, type, cancellationToken).DynamicContext();
                    return result;
                }
            }
            if (GetFactory(type) is Di_Delegate factory && factory(type, out res))
            {
                await AddDiObjectAsync(res, type, cancellationToken).DynamicContext();
                return new TryAsyncResult<object>(res, succeed: true);
            }
            return await DiHelper.GetDiObjectAsync(type, serviceProvider, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Get a DI object
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Type</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="tryAll">Try all sources (also unkeyed)?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<ITryAsyncResult> GetKeyedDiObjectAsync(
            object key,
            Type type,
            IServiceProvider? serviceProvider = null,
            bool tryAll = true,
            CancellationToken cancellationToken = default
            )
        {
            if (!KeyedObjects.TryGetValue(key, out var dict))
                return await DiHelper.GetKeyedDiObjectAsync(key, type, serviceProvider, tryAll, cancellationToken).DynamicContext();
            object? obj = GetKeyedObject(key, type);
            if (obj is not null) return new TryAsyncResult<object>(obj, succeed: true);
            if (serviceProvider is IKeyedServiceProvider keyedServiceProvider)
            {
                obj = keyedServiceProvider.GetKeyedService(type, key);
                if (obj is not null) return new TryAsyncResult<object>(obj, succeed: true);
            }
            if (ServiceProvider is IKeyedServiceProvider keyedServiceProvider2)
            {
                obj = keyedServiceProvider2.GetKeyedService(type, key);
                if (obj is not null) return new TryAsyncResult<object>(obj, succeed: true);
            }
            if (tryAll)
            {
                obj = GetObject(type) ?? serviceProvider?.GetService(type) ?? ServiceProvider?.GetService(type);
                if (obj is not null) return new TryAsyncResult<object>(obj, succeed: true);
            }
            if (GetKeyedAsyncFactory(key, type) is DiAsync_Delegate keyedAsyncFactory)
            {
                ITryAsyncResult result = await keyedAsyncFactory(type, cancellationToken).DynamicContext();
                if (result.Succeed)
                {
                    AddKeyedDiObject(key, result.Result);
                    return result;
                }
            }
            if (GetKeyedFactory(key, type) is Di_Delegate keyedFactory && keyedFactory(type, out obj))
            {
                AddKeyedDiObject(key, obj);
                return new TryAsyncResult<object>(obj, succeed: true);
            }
            if (tryAll)
            {
                if (GetAsyncFactory(type) is DiAsync_Delegate asyncFactory)
                {
                    ITryAsyncResult result = await asyncFactory(type, cancellationToken).DynamicContext();
                    if (result.Succeed)
                    {
                        AddDiObject(result.Result);
                        return result;
                    }
                }
                if (GetFactory(type) is Di_Delegate factory && factory(type, out obj))
                {
                    AddDiObject(type);
                    return new TryAsyncResult<object>(obj, succeed: true);
                }
            }
            return await DiHelper.GetKeyedDiObjectAsync(key, type, serviceProvider, tryAll, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Get a DI object
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<TryAsyncResult<T>> GetDiObjectAsync<T>(IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default)
            => (TryAsyncResult<T>)await GetDiObjectAsync(typeof(T), serviceProvider, cancellationToken).DynamicContext();

        /// <summary>
        /// Get a DI object
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="tryAll">Try all sources (also unkeyed)?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public async Task<TryAsyncResult<T>> GetKeyedDiObjectAsync<T>(
            object key,
            IServiceProvider? serviceProvider = null,
            bool tryAll = true,
            CancellationToken cancellationToken = default
            )
            => (TryAsyncResult<T>)await GetKeyedDiObjectAsync(key, typeof(T), serviceProvider, tryAll, cancellationToken).DynamicContext();

        /// <summary>
        /// Get an object
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Object</returns>
        new public object? GetObject(in Type type)
            => ScopeObjects.Keys.GetClosestType(type) is Type t && ScopeObjects.TryGetValue(t, out object? res) ? res : DiHelper.GetObject(type);

        /// <summary>
        /// Get a keyed object
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Type</param>
        /// <returns>Object</returns>
        new public object? GetKeyedObject(in object key, in Type type)
        {
            if (!KeyedScopeObjects.TryGetValue(key, out var dict)) return null;
            return dict.Keys.GetClosestType(type) is Type t && dict.TryGetValue(t, out object? res) ? res : DiHelper.GetKeyedObject(key, type);
        }

        /// <summary>
        /// Get an object factory
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Factory</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public Di_Delegate? GetFactory(Type type)
            => ScopeObjectFactories.Keys.GetClosestType(type) is Type t && ScopeObjectFactories.TryGetValue(t, out Di_Delegate? res) ? res : DiHelper.GetFactory(type);

        /// <summary>
        /// Get an object factory
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Type</param>
        /// <returns>Factory</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public Di_Delegate? GetKeyedFactory(object key, Type type)
        {
            if (!KeyedScopeObjectFactories.TryGetValue(key, out var dict)) return null;
            return dict.Keys.GetClosestType(type) is Type t && dict.TryGetValue(t, out Di_Delegate? res) ? res : DiHelper.GetKeyedFactory(key, type);
        }

        /// <summary>
        /// Get an object factory
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Factory</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public DiAsync_Delegate? GetAsyncFactory(Type type)
            => ScopeAsyncObjectFactories.Keys.GetClosestType(type) is Type t && ScopeAsyncObjectFactories.TryGetValue(t, out DiAsync_Delegate? res) 
                ? res 
                : DiHelper.GetAsyncFactory(type);

        /// <summary>
        /// Get an object factory
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Type</param>
        /// <returns>Factory</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        new public DiAsync_Delegate? GetKeyedAsyncFactory(object key, Type type)
        {
            if (!KeyedScopeAsyncObjectFactories.TryGetValue(key, out var dict)) return null;
            return dict.Keys.GetClosestType(type) is Type t && dict.TryGetValue(t, out DiAsync_Delegate? res) ? res : DiHelper.GetKeyedAsyncFactory(key, type);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// DI helper
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public partial class DiHelper() : IAsyncKeyedServiceProvider, IServiceProviderIsService, IServiceProviderIsKeyedService
    {
        /// <inheritdoc/>
        public virtual object? GetService(Type serviceType) => GetDiObject(serviceType, out object? res) ? res : null;

        /// <inheritdoc/>
        public virtual async Task<object?> GetServiceAsync(Type serviceType, CancellationToken cancellationToken = default)
            => (await GetDiObjectAsync(serviceType, serviceProvider: null, cancellationToken).DynamicContext()).Result;

        /// <inheritdoc/>
        public virtual bool IsService(Type serviceType)
        {
            if (ServiceProvider is IServiceProviderIsService serviceProvider && serviceProvider.IsService(serviceType))
                return true;
            if (Objects.Keys.GetClosestType(serviceType) is not null)
                return true;
            if (ObjectFactories.Keys.GetClosestType(serviceType) is not null)
                return true;
            if (AsyncObjectFactories.Keys.GetClosestType(serviceType) is not null)
                return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual object? GetKeyedService(Type serviceType, object? serviceKey)
        {
            if(serviceKey is null)
            {
                return GetDiObject(serviceType, out object? res) ? res : null;
            }
            else if(GetKeyedDiObject(serviceKey,serviceType, out object? res))
            {
                return res;
            }
            return null;
        }

        /// <inheritdoc/>
        public virtual async Task<object?> GetKeyedServiceAsync(Type serviceType, object? serviceKey, CancellationToken cancellationToken = default)
        {
            if (serviceKey is null)
            {
                ITryAsyncResult res = await GetDiObjectAsync(serviceType, cancellationToken: cancellationToken).DynamicContext();
                return res.Succeed ? res.Result : null;
            }
            {
                ITryAsyncResult res = await GetKeyedDiObjectAsync(serviceKey, serviceType, cancellationToken: cancellationToken).DynamicContext();
                return res.Succeed ? res.Result : null;
            }
        }

        /// <inheritdoc/>
        public virtual object GetRequiredKeyedService(Type serviceType, object? serviceKey)
            => GetKeyedService(serviceType, serviceKey) is object res 
                ? res 
                : throw new InvalidOperationException($"Service {serviceType} (key \"{serviceKey}\") not found");

        /// <inheritdoc/>
        public virtual async Task<object> GetRequiredKeyedServiceAsync(Type serviceType, object? serviceKey, CancellationToken cancellationToken = default)
            => await GetKeyedServiceAsync(serviceType, serviceKey, cancellationToken).DynamicContext() is object res
                ? res
                : throw new InvalidOperationException($"Service {serviceType} (key \"{serviceKey}\") not found");

        /// <inheritdoc/>
        public virtual bool IsKeyedService(Type serviceType, object? serviceKey)
        {
            if (ServiceProvider is IServiceProviderIsKeyedService keyedServiceProvider && keyedServiceProvider.IsKeyedService(serviceType, serviceKey))
                return true;
            if (serviceKey is null)
                return false;
            if (KeyedObjects.TryGetValue(serviceKey, out var dict) && dict.Keys.GetClosestType(serviceType) is not null)
                return true;
            if (KeyedObjectFactories.TryGetValue(serviceKey, out var factoryDict) && factoryDict.Keys.GetClosestType(serviceType) is not null)
                return true;
            return false;
        }

        /// <summary>
        /// DI delegate
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="obj">Object</param>
        /// <returns>Use the object?</returns>
        public delegate bool Di_Delegate(Type type, [NotNullWhen(returnValue: true)] out object? obj);

        /// <summary>
        /// Asynchronous DI delegate
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The result</returns>
        public delegate Task<ITryAsyncResult> DiAsync_Delegate(Type type, CancellationToken cancellationToken);
    }
}

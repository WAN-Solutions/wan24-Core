using Microsoft.Extensions.DependencyInjection;

namespace wan24.Core
{
    /// <summary>
    /// Interface for an asynchronous keyed service provider
    /// </summary>
    public interface IAsyncKeyedServiceProvider : IAsyncServiceProvider, IKeyedServiceProvider
    {
        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <param name="serviceKey">An object that specifies the key of service object to get.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns> A service object of type serviceType. -or- null if there is no service object of type serviceType.</returns>
        Task<object?> GetKeyedServiceAsync(Type serviceType, object? serviceKey, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets service of type <paramref name="serviceType"/> from the <see cref="IServiceProvider"/> implementing
        /// this interface.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the service.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A service object of type <paramref name="serviceType"/>.
        /// Throws an exception if the <see cref="IServiceProvider"/> cannot create the object.</returns>
        Task<object> GetRequiredKeyedServiceAsync(Type serviceType, object? serviceKey, CancellationToken cancellationToken = default);
    }
}

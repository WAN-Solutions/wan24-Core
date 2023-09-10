namespace wan24.Core
{
    /// <summary>
    /// Interface for an asynchronous service provider
    /// </summary>
    public interface IAsyncServiceProvider : IServiceProvider
    {
        /// <summary>
        /// Get a service
        /// </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Service</returns>
        Task<object?> GetServiceAsync(Type serviceType, CancellationToken cancellationToken = default);
    }
}

using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// DI helper
    /// </summary>
    public partial class DiHelper : IAsyncServiceProvider, IServiceProviderIsService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DiHelper() { }

        /// <inheritdoc/>
        public virtual object? GetService(Type serviceType) => GetDiObject(serviceType, out object? res) ? res : null;

        /// <inheritdoc/>
        public virtual async Task<object?> GetServiceAsync(Type serviceType, CancellationToken cancellationToken = default)
            => (await GetDiObjectAsync(serviceType, serviceProvider: null, cancellationToken).DynamicContext()).Object;

        /// <inheritdoc/>
        public virtual bool IsService(Type serviceType)
            => ((ServiceProvider as IServiceProviderIsService)?.IsService(serviceType) ?? false) ||
                GetFactory(serviceType) is not null ||
                GetAsyncFactory(serviceType) is not null;

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
        /// <returns>The object and if to use the result</returns>
        public delegate Task<AsyncResult> DiAsync_Delegate(Type type, CancellationToken cancellationToken);
    }
}

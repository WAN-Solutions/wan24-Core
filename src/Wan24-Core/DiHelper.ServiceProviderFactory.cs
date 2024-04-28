using Microsoft.Extensions.DependencyInjection;

namespace wan24.Core
{
    // Service provider factory
    public partial class DiHelper
    {
        /// <summary>
        /// DI helper service provider factory
        /// </summary>
        public sealed class DiServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="options">Options</param>
            /// <param name="scoped">Create scoped service providers?</param>
            /// <param name="defaultFactory">Default service provider factory</param>
            public DiServiceProviderFactory(
                in ServiceProviderOptions? options = null, 
                in bool scoped = true, 
                in IServiceProviderFactory<IServiceCollection>? defaultFactory = null
                )
            {
                DefaultFactory = defaultFactory ?? new DefaultServiceProviderFactory(options ?? new()
                {
                    ValidateOnBuild = false,
                    ValidateScopes = false
                });
                Scoped = scoped;
            }

            /// <summary>
            /// Default service provider factory
            /// </summary>
            public IServiceProviderFactory<IServiceCollection> DefaultFactory { get; }

            /// <summary>
            /// Create scoped service providers?
            /// </summary>
            public bool Scoped { get; }

            /// <inheritdoc/>
            public IServiceCollection CreateBuilder(IServiceCollection services) => DefaultFactory.CreateBuilder(services);

            /// <inheritdoc/>
            public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
            {
                if (Scoped)
                {
                    ScopedDiHelper res = new();
                    containerBuilder.AddScoped(sp => res);
                    res.ServiceProvider = DefaultFactory.CreateServiceProvider(containerBuilder);
                    return res;
                }
                containerBuilder.AddSingleton(sp => Instance);
                ServiceProvider = DefaultFactory.CreateServiceProvider(containerBuilder);
                return Instance;
            }
        }
    }
}

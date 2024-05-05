namespace wan24.Core
{
    // Service provider
    public partial class ScopedDiHelper
    {
        /// <summary>
        /// DI service provider (will be disposed, if possible!)
        /// </summary>
        new public IServiceProvider? ServiceProvider { get; set; }

        /// <summary>
        /// Dispose the <see cref="ServiceProvider"/> when disposing?
        /// </summary>
        public bool DisposeServiceProvider { get; set; } = true;

        /// <inheritdoc/>
        public override object? GetService(Type serviceType) => GetDiObject(serviceType, out object? res) ? res : null;

        /// <inheritdoc/>
        public override async Task<object?> GetServiceAsync(Type serviceType, CancellationToken cancellationToken = default)
            => (await GetDiObjectAsync(serviceType, serviceProvider: null, cancellationToken).DynamicContext()).Result;

        /// <inheritdoc/>
        public override bool IsService(Type serviceType)
        {
            if (ScopeObjects.Keys.GetClosestType(serviceType) is not null)
                return true;
            if (ScopeObjectFactories.Keys.GetClosestType(serviceType) is not null)
                return true;
            if (ScopeAsyncObjectFactories.Keys.GetClosestType(serviceType) is not null)
                return true;
            return base.IsService(serviceType);
        }

        /// <inheritdoc/>
        public override bool IsKeyedService(Type serviceType, object? serviceKey)
        {
            if (serviceKey is null)
                return base.IsKeyedService(serviceType, serviceKey);
            if (KeyedScopeObjects.TryGetValue(serviceKey, out var dict) && dict.Keys.GetClosestType(serviceType) is not null)
                return true;
            if (KeyedScopeObjectFactories.TryGetValue(serviceKey, out var factoryDict) && factoryDict.Keys.GetClosestType(serviceType) is not null)
                return true;
            return base.IsKeyedService(serviceType, serviceKey);
        }
    }
}

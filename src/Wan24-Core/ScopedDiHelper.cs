using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Scoped DI helper (will dispose created objects, if possible; don't forget to dispose!)
    /// </summary>
    public partial class ScopedDiHelper : DiHelper
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Objects
        /// </summary>
        protected readonly ConcurrentDictionary<Type, object> ScopeObjects = new();
        /// <summary>
        /// Keyed objects
        /// </summary>
        protected readonly ConcurrentDictionary<object, ConcurrentDictionary<Type, object>> KeyedScopeObjects = new();

        /// <summary>
        /// Constructor
        /// </summary>
        public ScopedDiHelper() : base()
        {
            DisposableAdapter = new(
                (disposing) =>
                {
                    _ScopeNotCachedTypes.Clear();
                    if (DisposeServiceProvider)
                        ServiceProvider?.TryDispose();
                    ScopeObjects.Clear();
                    foreach (var dict in KeyedScopeObjects.Values)
                        dict.Clear();
                    KeyedScopeObjects.Clear();
                    ScopeObjectFactories.Clear();
                    KeyedScopeObjectFactories.Clear();
                    ScopeAsyncObjectFactories.Clear();
                    KeyedScopeAsyncObjectFactories.Clear();
                    Sync.Dispose();
                },
                async () =>
                {
                    _ScopeNotCachedTypes.Clear();
                    if (DisposeServiceProvider && ServiceProvider is not null) await ServiceProvider.TryDisposeAsync().DynamicContext();
                    ScopeObjects.Clear();
                    foreach (var dict in KeyedScopeObjects.Values)
                        dict.Clear();
                    KeyedScopeObjects.Clear();
                    ScopeObjectFactories.Clear();
                    KeyedScopeObjectFactories.Clear();
                    ScopeAsyncObjectFactories.Clear();
                    KeyedScopeAsyncObjectFactories.Clear();
                    await Sync.DisposeAsync().DynamicContext();
                });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceProvider">Service provider (will be disposed, if possible!)</param>
        public ScopedDiHelper(IServiceProvider serviceProvider) : this() => ServiceProvider = serviceProvider;

        /// <summary>
        /// DI object factories
        /// </summary>
        public readonly ConcurrentDictionary<Type, Di_Delegate> ScopeObjectFactories = new();
        /// <summary>
        /// Keyed DI object factories
        /// </summary>
        public readonly ConcurrentDictionary<object, ConcurrentDictionary<Type, Di_Delegate>> KeyedScopeObjectFactories = new();
        /// <summary>
        /// Asynchronous DI object factories
        /// </summary>
        public readonly ConcurrentDictionary<Type, DiAsync_Delegate> ScopeAsyncObjectFactories = new();
        /// <summary>
        /// Keyed asynchronous DI object factories
        /// </summary>
        public readonly ConcurrentDictionary<object, ConcurrentDictionary<Type, DiAsync_Delegate>> KeyedScopeAsyncObjectFactories = new();
    }
}

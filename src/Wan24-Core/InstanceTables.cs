namespace wan24.Core
{
    /// <summary>
    /// Instance tables
    /// </summary>
    public static class InstanceTables
    {
        /// <summary>
        /// Registered instance tables
        /// </summary>
        public static readonly Dictionary<Type, Type> Registered = new()
        {
            {typeof(Delay), typeof(DelayTable) },
            {typeof(IInMemoryCache), typeof(InMemoryCacheTable) },
            {typeof(IObjectLockManager), typeof(ObjectLockTable) },
            {typeof(IPool), typeof(PoolTable) },
            {typeof(IProcessingInfo), typeof(ProcessTable) },
            {typeof(IServiceWorker), typeof(ServiceWorkerTable) },
            {typeof(ITimer), typeof(TimerTable) }
        };
    }
}

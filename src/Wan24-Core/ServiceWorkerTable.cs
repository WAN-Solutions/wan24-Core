namespace wan24.Core
{
    /// <summary>
    /// Service worker table
    /// </summary>
    public static class ServiceWorkerTable
    {
        /// <summary>
        /// Service workers (key is a GUID)
        /// </summary>
        public static readonly ConcurrentChangeTokenDictionary<string, IServiceWorker> ServiceWorkers = [];
    }
}

using Microsoft.Extensions.Hosting;

namespace wan24.Core
{
    /// <summary>
    /// Interface for a service worker object
    /// </summary>
    public interface IServiceWorker : IHostedService, IDisposableObject
    {
        /// <summary>
        /// Display name
        /// </summary>
        string? Name { get; }
        /// <summary>
        /// Is running?
        /// </summary>
        bool IsRunning { get; }
        /// <summary>
        /// Last start time
        /// </summary>
        DateTime Started { get; }
        /// <summary>
        /// Last stop time
        /// </summary>
        DateTime Stopped { get; }
        /// <summary>
        /// Start
        /// </summary>
        Task StartAsync();
        /// <summary>
        /// Stop
        /// </summary>
        Task StopAsync();
        /// <summary>
        /// Restart
        /// </summary>
        Task RestartAsync();
    }
}

using Microsoft.Extensions.Hosting;

namespace wan24.Core
{
    /// <summary>
    /// Interface for a service worker object
    /// </summary>
    public interface IServiceWorker : IHostedService, IBasicDisposableObject, IDisposable, IAsyncDisposable
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
        /// Is paused?
        /// </summary>
        bool IsPaused { get; }
        /// <summary>
        /// Can be paused
        /// </summary>
        bool CanPause { get; }
        /// <summary>
        /// Last start time
        /// </summary>
        DateTime Started { get; }
        /// <summary>
        /// Paused time (if paused)
        /// </summary>
        DateTime Paused { get; }
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
        /// <summary>
        /// Pause
        /// </summary>
        Task PauseAsync();
        /// <summary>
        /// Resume from pause
        /// </summary>
        Task ResumeAsync();
    }
}

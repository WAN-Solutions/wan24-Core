namespace wan24.Core
{
    /// <summary>
    /// Interface for a timer object
    /// </summary>
    public interface ITimer
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
        /// Auto-reset?
        /// </summary>
        bool AutoReset { get; }
        /// <summary>
        /// Interval
        /// </summary>
        TimeSpan Interval { get; }
        /// <summary>
        /// Last elapsed
        /// </summary>
        DateTime LastElapsed { get; }
        /// <summary>
        /// Shedules
        /// </summary>
        DateTime Sheduled { get; }
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

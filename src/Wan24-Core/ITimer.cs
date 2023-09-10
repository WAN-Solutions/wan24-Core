namespace wan24.Core
{
    /// <summary>
    /// Interface for a timer object
    /// </summary>
    public interface ITimer : IServiceWorker
    {
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
    }
}

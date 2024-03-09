using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="LogLevel"/> extensions
    /// </summary>
    public static class LogLevelExtensions
    {
        /// <summary>
        /// Is tracing?
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>If tracing</returns>
        public static bool IsTracing(this LogLevel level) => level == LogLevel.Trace && !level.IsNoLogging();

        /// <summary>
        /// Is debugging?
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>If debugging</returns>
        public static bool IsDebugging(this LogLevel level) => level <= LogLevel.Debug && !level.IsNoLogging();

        /// <summary>
        /// Is informative?
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>If informative</returns>
        public static bool IsInformative(this LogLevel level) => level <= LogLevel.Information && !level.IsNoLogging();

        /// <summary>
        /// Is warning?
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>If warning</returns>
        public static bool IsWarning(this LogLevel level) => level <= LogLevel.Warning && !level.IsNoLogging();

        /// <summary>
        /// Is error?
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>If error</returns>
        public static bool IsError(this LogLevel level) => level <= LogLevel.Error && !level.IsNoLogging();

        /// <summary>
        /// Is critical?
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>If critical</returns>
        public static bool IsCritical(this LogLevel level) => level <= LogLevel.Critical && !level.IsNoLogging();

        /// <summary>
        /// Is no logging?
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>If not logging</returns>
        public static bool IsNoLogging(this LogLevel level) => level == LogLevel.None;
    }
}

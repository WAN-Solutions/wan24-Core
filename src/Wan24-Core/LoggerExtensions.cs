using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="ILogger"/> extensions
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Get the log level from a logger, if it's an <see cref="ILogger"/> - otherwise return <see cref="Settings.LogLevel"/>
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <returns>Log level</returns>
        public static LogLevel GetLogLevel(this ILogger logger) => (logger as LoggerBase)?.Level ?? Settings.LogLevel;
    }
}

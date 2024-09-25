using Microsoft.Extensions.Logging;
using System.Runtime;
using System.Runtime.CompilerServices;

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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static LogLevel GetLogLevel(this ILogger logger) => (logger as LoggerBase)?.Level ?? Settings.LogLevel;

        /// <summary>
        /// Get the final logger
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <returns>Final logger</returns>
        public static ILogger GetFinalLogger(this ILogger logger)
        {
            while (logger is LoggerBase baseLogger)
                if (baseLogger.Next is ILogger nextLogger)
                {
                    logger = nextLogger;
                }
                else
                {
                    break;
                }
            return logger;
        }
    }
}

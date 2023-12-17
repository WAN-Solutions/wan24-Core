using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace wan24.Core
{
    /// <summary>
    /// Debug logger
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="level">Log level</param>
    /// <param name="next">Next logger which should receive the message</param>
    public sealed class DebugLogger(in LogLevel level = Logging.DEFAULT_LOGLEVEL, in ILogger? next = null) : LoggerBase(level, next)
    {
        /// <inheritdoc/>
        protected override void LogInt<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Debug.WriteLine(GetMessage(logLevel, eventId, state, exception, formatter));
    }
}

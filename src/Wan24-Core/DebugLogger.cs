using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace wan24.Core
{
    /// <summary>
    /// Debug logger
    /// </summary>
    public sealed class DebugLogger : LoggerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="next">Next logger which should receive the message</param>
        public DebugLogger(in LogLevel level = LogLevel.Information, in ILogger? next = null) : base(level, next) { }

        /// <inheritdoc/>
        protected override void LogInt<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Debug.WriteLine(GetMessage(logLevel, eventId, state, exception, formatter));
    }
}

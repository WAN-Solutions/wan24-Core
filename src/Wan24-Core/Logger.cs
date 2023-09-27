using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Logger (adopts to <see cref="Logging"/> - NEVER use this as <see cref="Logging.Logger"/>!)
    /// </summary>
    public class Logger : LoggerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="next">Next logger which should receive the message</param>
        public Logger(in LogLevel level = Logging.DEFAULT_LOGLEVEL, in ILogger? next = null) :base(level, next) { }

        /// <inheritdoc/>
        protected override void LogInt<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            switch (logLevel)
            {
                case LogLevel.Trace: Logging.WriteTrace(formatter(state, exception)); break;
                case LogLevel.Debug: Logging.WriteDebug(formatter(state, exception)); break;
                case LogLevel.Information: Logging.WriteInfo(formatter(state, exception)); break;
                case LogLevel.Warning: Logging.WriteWarning(formatter(state, exception)); break;
                case LogLevel.Error: Logging.WriteError(formatter(state, exception)); break;
                case LogLevel.Critical: Logging.WriteCritical(formatter(state, exception)); break;
                default: throw new NotSupportedException($"Unsupported log level {logLevel}");
            }
        }
    }
}

using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Logger (adopts to <see cref="Logging"/> - NEVER use this as <see cref="Logging.Logger"/>!)
    /// </summary>
    public class Logger : ILogger
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="level">Log level</param>
        public Logger(LogLevel level = LogLevel.Information) => Level = level;

        /// <summary>
        /// Log level
        /// </summary>
        public LogLevel Level { get; set; }

        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => logLevel >= Level;

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
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

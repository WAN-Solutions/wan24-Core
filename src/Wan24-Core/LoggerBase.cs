using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a logger
    /// </summary>
    public abstract class LoggerBase : ILogger
    {
        /// <summary>
        /// Regular expression to match a new line
        /// </summary>
        internal static readonly Regex RX_NL = new(@"\r?\n\t?", RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="next">Next logger which should receive the message</param>
        protected LoggerBase(in LogLevel level = Logging.DEFAULT_LOGLEVEL, in ILogger? next = null)
        {
            Level = level;
            Next = next;
        }

        /// <summary>
        /// Log level
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Next logger which should receive the message
        /// </summary>
        public ILogger? Next { get; set; }

        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => logLevel >= Level;

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            LogInt(logLevel, eventId, state,exception,formatter);
            Next?.Log(logLevel, eventId, state, exception, formatter);
        }

        /// <summary>
        /// Writes a log entry
        /// </summary>
        /// <typeparam name="TState">State type</typeparam>
        /// <param name="logLevel">Level</param>
        /// <param name="eventId">Event ID</param>
        /// <param name="state">State</param>
        /// <param name="exception">Exception</param>
        /// <param name="formatter">Formatter</param>
        protected abstract void LogInt<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter);

        /// <summary>
        /// Get the mesage to log
        /// </summary>
        /// <typeparam name="TState">State type</typeparam>
        /// <param name="logLevel">Level</param>
        /// <param name="eventId">Event ID</param>
        /// <param name="state">State</param>
        /// <param name="exception">Exception</param>
        /// <param name="formatter">Formatter</param>
        /// <param name="nl">Add a new line at the end?</param>
        /// <returns>Message</returns>
        protected virtual string GetMessage<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter,
            bool nl = false
            )
            => $"{DateTime.Now}\t{logLevel}\t{RX_NL.Replace(formatter(state, exception), $"{Environment.NewLine}\t")}{(nl ? Environment.NewLine : string.Empty)}";
    }
}

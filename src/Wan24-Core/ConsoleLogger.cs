using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Console logger
    /// </summary>
    public sealed class ConsoleLogger : LoggerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="next">Next logger which should receive the message</param>
        public ConsoleLogger(in LogLevel level = LogLevel.Information, in ILogger? next = null) : base(level, next) { }

        /// <summary>
        /// Write to STDERR?
        /// </summary>
        public bool WriteToStdErr { get; set; } = true;

        /// <inheritdoc/>
        protected override void LogInt<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (WriteToStdErr)
            {
                Console.Error.WriteLine(GetMessage(logLevel, eventId, state, exception, formatter));
            }
            else
            {
                Console.WriteLine(GetMessage(logLevel, eventId, state, exception, formatter));
            }
        }
    }
}

using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Console logger
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="level">Log level</param>
    /// <param name="next">Next logger which should receive the message</param>
    public sealed class ConsoleLogger(in LogLevel? level = null, in ILogger? next = null) : LoggerBase(level, next)
    {
        /// <summary>
        /// Write to STDERR?
        /// </summary>
        public bool WriteToStdErr { get; set; } = true;

        /// <inheritdoc/>
        protected override void LogInt<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (ENV.IsBrowserApp)
            {
                if (logLevel < LogLevel.Warning)
                {
                    Console.WriteLine($"{logLevel.ToString().ToUpper()}\t{formatter(state, exception)}");
                }
                else
                {
                    Console.Error.WriteLine($"{logLevel.ToString().ToUpper()}\t{formatter(state, exception)}");
                }
            }
            else
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
}

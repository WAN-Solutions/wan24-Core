using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Category logger prepends a category before the written message
    /// </summary>
    public sealed class CategoryLogger : LoggerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="next">Next logger which should receive the message (including the category prefix)</param>
        /// <param name="level">Level</param>
        public CategoryLogger(in string category, in ILogger next, in LogLevel level = Logging.DEFAULT_LOGLEVEL) : base(level, next) => Category = category;

        /// <summary>
        /// Category
        /// </summary>
        public string Category { get; }

        /// <inheritdoc/>
        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Next!.Log(logLevel, eventId, state, exception, (state, ex) => $"{Category}\t{formatter(state, ex)}");

        /// <inheritdoc/>
        protected override void LogInt<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}

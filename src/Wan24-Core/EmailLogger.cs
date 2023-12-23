using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Email logger
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="template">Email template (will be disposed!)</param>
    /// <param name="fromEmail">Sender email address</param>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="level">Level</param>
    /// <param name="emailLevel">Email logging level</param>
    /// <param name="next">Next logger which should receive the message</param>
    /// <param name="mta">MTA (won't be disposed)</param>
    public class EmailLogger(
        in IEmailTemplate template,
        in string fromEmail,
        in string toEmail,
        in LogLevel? level = null,
        in LogLevel emailLevel = LogLevel.Warning,
        in ILogger? next = null,
        in IMta? mta = null
            ) : DisposableLoggerBase(level, next)
    {
        /// <summary>
        /// An object for thread locking
        /// </summary>
        protected readonly object SyncObject = new();

        /// <summary>
        /// MTA (won't be disposed)
        /// </summary>
        public IMta? MTA { get; } = mta;

        /// <summary>
        /// Email template (will be disposed!)
        /// </summary>
        public IEmailTemplate Template { get; } = template;

        /// <summary>
        /// Parser data
        /// </summary>
        public Dictionary<string, string> ParserData { get; set; } = [];

        /// <summary>
        /// Sender email address
        /// </summary>
        public string FromEmail { get; set; } = fromEmail;

        /// <summary>
        /// Recipient email address
        /// </summary>
        public string ToEmail { get; set; } = toEmail;

        /// <summary>
        /// Email logging level
        /// </summary>
        public LogLevel EmailLevel { get; set; } = emailLevel;

        /// <summary>
        /// Email sending frequency (set to <see cref="TimeSpan.Zero"/> for sending all emails)
        /// </summary>
        public TimeSpan SendingFrequency { get; set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// Time of the last sent email
        /// </summary>
        public DateTime LastEmail { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Number of skipped emails
        /// </summary>
        public int Skipped { get; protected set; }

        /// <inheritdoc/>
        protected override void LogInt<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            EnsureUndisposed();
            if (logLevel < EmailLevel) return;
            lock (SyncObject)
            {
                if (SendingFrequency > TimeSpan.Zero && DateTime.Now - LastEmail < SendingFrequency)
                {
                    Skipped++;
                    return;
                }
                LastEmail = DateTime.Now;
                _ = Template.SendAsync(FromEmail, ToEmail, GetParserData(logLevel, eventId, state, exception, formatter), MTA).DynamicContext();
                Skipped = 0;
            }
        }

        /// <summary>
        /// Get the email template parser data
        /// </summary>
        /// <typeparam name="TState">State type</typeparam>
        /// <param name="logLevel">Level</param>
        /// <param name="eventId">Event ID</param>
        /// <param name="state">State</param>
        /// <param name="exception">Exception</param>
        /// <param name="formatter">Formatter</param>
        /// <returns>Parser data</returns>
        protected virtual Dictionary<string, string> GetParserData<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
            )
            => new(ParserData)
                {
                    {"time", DateTime.Now.ToString() },
                    {"level", logLevel.ToString() },
                    {"skipped", Skipped.ToString() },
                    {"event", eventId.Id.ToString() },
                    {"eventName", eventId.Name ?? string.Empty },
                    {"exception", exception?.ToString() ?? string.Empty },
                    {"exceptionMessage", exception?.Message ?? string.Empty },
                    {"exceptionStack", exception?.StackTrace ?? string.Empty },
                    {"message", GetMessage(logLevel, eventId, state, exception, formatter) },
                };

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => Template.Dispose();

        /// <inheritdoc/>
        protected override async Task DisposeCore() => await Template.DisposeAsync().DynamicContext();
    }
}

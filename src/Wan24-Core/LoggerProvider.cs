using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Logger provider
    /// </summary>
    public sealed class LoggerProvider : BasicAllDisposableBase, ILoggerProvider
    {
        /// <summary>
        /// Singleton logger instance to use
        /// </summary>
        private readonly ILogger? Logger;
        /// <summary>
        /// Logger factory
        /// </summary>
        private readonly LoggerFactory_Delegate? Factory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Singleton logger instance to use</param>
        public LoggerProvider(ILogger logger) : base()
        {
            Logger = logger;
            Factory = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="factory">Logger factory</param>
        public LoggerProvider(LoggerFactory_Delegate factory) : base()
        {
            Logger = null;
            Factory = factory;
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName) => new CategoryLogger(categoryName, Logger ?? Factory!(this, categoryName));

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => Logger?.TryDispose();

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (Logger is not null) await Logger.TryDisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Delegate for a logger factory
        /// </summary>
        /// <param name="provider">Logger provider</param>
        /// <param name="categoryName">Category name to prepend to logged messages</param>
        /// <returns>Logger</returns>
        public delegate ILogger LoggerFactory_Delegate(LoggerProvider provider, string categoryName);
    }
}

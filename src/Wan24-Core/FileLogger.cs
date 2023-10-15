using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    /// <summary>
    /// File logger (don't forget to dispose an instance!)
    /// </summary>
    public class FileLogger : DisposableLoggerBase
    {
        /// <summary>
        /// Worker
        /// </summary>
        protected readonly LogQueueWorker Worker = null!;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="level">Log level</param>
        /// <param name="next">Next logger which should receive the message</param>
        /// <param name="maxQueue">Maximum number of queued messages before blocking</param>
        protected FileLogger(in string fileName, in LogLevel level = Logging.DEFAULT_LOGLEVEL, in ILogger? next = null, in int maxQueue = int.MaxValue) : base(level, next)
        {
            try
            {
                Worker = new(fileName, maxQueue);
                FileName = fileName;
            }
            catch
            {
                Worker?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Filename
        /// </summary>
        public string FileName { get; }

        /// <inheritdoc/>
        protected override void LogInt<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Worker.TryEnqueue(GetMessage(logLevel, eventId, state, exception, formatter, nl: true));

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => Worker.Dispose();

        /// <inheritdoc/>
        protected override async Task DisposeCore() => await Worker.DisposeAsync().DynamicContext();

        /// <summary>
        /// Create a file logger
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="level">Log level</param>
        /// <param name="next">Next logger which should receive the message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>File logger</returns>
        public static async Task<FileLogger> CreateAsync(
            string fileName, 
            LogLevel level = LogLevel.Information, 
            ILogger? next = null, 
            CancellationToken cancellationToken = default
            )
        {
            FileLogger res = new(fileName, level, next);
            try
            {
                await res.Worker.StartAsync(cancellationToken).DynamicContext();
                return res;
            }
            catch
            {
                await res.DisposeAsync().DynamicContext();
                throw;
            }
        }

        /// <summary>
        /// Log queue worker
        /// </summary>
        protected sealed class LogQueueWorker : ItemQueueWorkerBase<string>
        {
            /// <summary>
            /// Stream
            /// </summary>
            private readonly FileStream Stream;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="fileName">Filename</param>
            /// <param name="maxQueue">Maximum number of queued messages before blocking</param>
            public LogQueueWorker(in string fileName, in int maxQueue) : base(maxQueue)
                => Stream = new(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

            /// <inheritdoc/>
            protected override async Task ProcessItem(string item, CancellationToken cancellationToken)
            {
                await Stream.WriteAsync(item.GetBytes(), cancellationToken).DynamicContext();
                if (Queued == 0) await Stream.FlushAsync(cancellationToken).DynamicContext();
            }

            /// <inheritdoc/>
            protected override async Task AfterStopAsync(CancellationToken cancellationToken)
            {
                await Stream.FlushAsync(cancellationToken).DynamicContext();
                await base.AfterStopAsync(cancellationToken).DynamicContext();
            }

            /// <inheritdoc/>
            protected override async Task AfterPauseAsync(CancellationToken cancellationToken)
            {
                await Stream.FlushAsync(cancellationToken).DynamicContext();
                await base.AfterPauseAsync(cancellationToken).DynamicContext();
            }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                Stream.Dispose();
            }

            /// <inheritdoc/>
            protected override async Task DisposeCore()
            {
                await base.DisposeCore().DynamicContext();
                await Stream.DisposeAsync().DynamicContext();
            }
        }
    }
}

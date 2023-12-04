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
        /// <param name="mode">Create file mode</param>
        protected FileLogger(
            in string fileName, 
            in LogLevel level = Logging.DEFAULT_LOGLEVEL, 
            in ILogger? next = null, 
            in int maxQueue = int.MaxValue,
            in UnixFileMode? mode = null
            )
            : base(level, next)
        {
            try
            {
                Worker = new(fileName, maxQueue, mode);
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
        /// <param name="mode">Create file mode</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>File logger</returns>
        public static async Task<FileLogger> CreateAsync(
            string fileName, 
            LogLevel level = LogLevel.Information, 
            ILogger? next = null, 
            UnixFileMode? mode = null,
            CancellationToken cancellationToken = default
            )
        {
            FileLogger res = new(fileName, level, next, mode: mode);
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
            /// Need to flush the stream?
            /// </summary>
            private bool NeedFlush = false;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="fileName">Filename</param>
            /// <param name="maxQueue">Maximum number of queued messages before blocking</param>
            /// <param name="mode">Create file mode</param>
            public LogQueueWorker(in string fileName, in int maxQueue, UnixFileMode? mode) : base(maxQueue)
            {
                FileStreamOptions fileOptions = new()
                {
                    Mode = FileMode.OpenOrCreate,
                    Access = FileAccess.Write,
                    Share = FileShare.Read,
                };
#pragma warning disable CA1416 // Platform specific
                if (ENV.IsLinux) fileOptions.UnixCreateMode = mode ?? Settings.CreateFileMode;
#pragma warning restore CA1416 // Platform specific
                Stream = new(fileName, fileOptions);
                if (Stream.Length != 0) Stream.Position = Stream.Length;
            }

            /// <inheritdoc/>
            protected override async Task ProcessItem(string item, CancellationToken cancellationToken)
            {
                await Stream.WriteAsync(item.GetBytes(), cancellationToken).DynamicContext();
                if (Queued == 0)
                {
                    await Stream.FlushAsync(cancellationToken).DynamicContext();
                }
                else
                {
                    NeedFlush = true;
                }
            }

            /// <inheritdoc/>
            protected override async Task AfterStopAsync(CancellationToken cancellationToken)
            {
                if (NeedFlush)
                {
                    await Stream.FlushAsync(cancellationToken).DynamicContext();
                    NeedFlush = false;
                }
                await base.AfterStopAsync(cancellationToken).DynamicContext();
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

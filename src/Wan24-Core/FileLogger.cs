using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    /// <summary>
    /// File logger (don't forget to dispose an instance!)
    /// </summary>
    public class FileLogger : DisposableBase, ILogger
    {
        /// <summary>
        /// Stream
        /// </summary>
        protected readonly FileStream Stream = null!;
        /// <summary>
        /// Worker
        /// </summary>
        protected readonly LogQueueWorker Worker = null!;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="level">Log level</param>
        protected FileLogger(in string fileName, in LogLevel level = LogLevel.Information) : base()
        {
            try
            {
                Stream = new(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                Worker = new(Stream);
                FileName = fileName;
                Level = level;
            }
            catch
            {
                Worker?.Dispose();
                Stream?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Filename
        /// </summary>
        public string FileName { get; }

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
            Worker.TryEnqueue($"{DateTime.Now}\t{logLevel}\t{formatter(state, exception).Replace("\n", "\n\t")}\n");
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Worker.Dispose();
            Stream.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            Worker.Dispose();
            await Stream.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Create a file logger
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="level">Log level</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>File logger</returns>
        public static async Task<FileLogger> CreateAsync(string fileName, LogLevel level = LogLevel.Information, CancellationToken cancellationToken = default)
        {
            FileLogger res = new(fileName, level);
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
            private readonly Stream Stream;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="stream">Stream</param>
            public LogQueueWorker(in Stream stream) : base(int.MaxValue) => Stream = stream;

            /// <inheritdoc/>
            protected override async Task ProcessItem(string item, CancellationToken cancellationToken)
                => await Stream.WriteAsync(item.GetBytes(), cancellationToken).DynamicContext();
        }
    }
}

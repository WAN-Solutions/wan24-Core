using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Pipeline stream (don't forget to start the <see cref="Queue"/> before feeding data into the pipeline)
    /// </summary>
    public partial class PipelineStream : StreamBase
    {
        /// <summary>
        /// Cancellation
        /// </summary>
        protected readonly CancellationTokenSource Cancellation = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queueCapacity">Queue capacity</param>
        /// <param name="parallelism">Degree of parallelism (how many tasks to process in parallel)</param>
        /// <param name="inputBufferSize">Input buffer size in bytes (or <see langword="null"/> to write to the first element directly)</param>
        /// <param name="runInputBufferProcessor">If to run an input buffer processor which feeds the first element from the input buffer</param>
        /// <param name="outputBufferSize">Output buffer size in bytes (or <see langword="null"/>, if not readable)</param>
        /// <param name="clearBuffers">If to clear buffers after use</param>
        /// <param name="elements">Elements</param>
        public PipelineStream(
            in int queueCapacity,
            in int parallelism,
            in int? inputBufferSize,
            in bool runInputBufferProcessor,
            in int? outputBufferSize,
            in bool clearBuffers,
            params PipelineElementBase[] elements
            )
            : base()
        {
            ClearBuffers = clearBuffers;
            Queue = new(this, queueCapacity, parallelism)
            {
                Name = "Pipeline stream parallel processing queue"
            };
            InputBuffer = inputBufferSize.HasValue
                ? new(inputBufferSize.Value, clearBuffers)
                : null;
            OutputBuffer = outputBufferSize.HasValue
                ? new(outputBufferSize.Value, clearBuffers)
                : null;
            int index = -1;
            Elements = new(
                elements.Select(e =>
                {
                    e.Pipeline = this;
                    e.Position = ++index;
                    return new KeyValuePair<string, PipelineElementBase>(e.Name, e);
                })
                );
            Elements.Freeze();
            if (runInputBufferProcessor && inputBufferSize.HasValue) InputBufferProcessorTask = ((Func<Task>)ProcessInputBufferAsync).StartLongRunningTask();
        }

        /// <summary>
        /// Thread synchronization event (raised when not synchronized)
        /// </summary>
        public ResetEvent SyncEvent { get; } = new(initialState: true);

        /// <summary>
        /// Pause event (raised when not paused)
        /// </summary>
        public ResetEvent PauseEvent { get; } = new(initialState: true);

        /// <summary>
        /// If to clear buffers after use
        /// </summary>
        public bool ClearBuffers { get; }

        /// <summary>
        /// Logger
        /// </summary>
        public ILogger? Logger { get; init; }

        /// <inheritdoc/>
        [MemberNotNullWhen(returnValue: true, nameof(OutputBuffer))]
        public override bool CanRead => OutputBuffer is not null;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Flush() => EnsureUndisposed(allowDisposing: true);

        /// <inheritdoc/>
        [MemberNotNull(nameof(OutputBuffer))]
#pragma warning disable CS8774 // OutputBuffer must not be NULL
        protected override void EnsureReadable() => base.EnsureReadable();
#pragma warning restore CS8774 // OutputBuffer must not be NULL

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Cancellation.Cancel();
            base.Dispose(disposing);
            Queue.Dispose();
            Elements.Values.DisposeAll();
            InputBuffer?.Dispose();
            OutputBuffer?.Dispose();
            InputBufferProcessorTask?.GetAwaiter().GetResult();
            SyncEvent.Dispose();
            PauseEvent.Dispose();
            Cancellation.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            Cancellation.Cancel();
            await base.DisposeCore().DynamicContext();
            await Queue.DisposeAsync().DynamicContext();
            await Elements.Values.DisposeAllAsync().DynamicContext();
            if (InputBuffer is not null) await InputBuffer.DisposeAsync().DynamicContext();
            if (OutputBuffer is not null) await OutputBuffer.DisposeAsync().DynamicContext();
            if (InputBufferProcessorTask is not null) await InputBufferProcessorTask.DynamicContext();
            await SyncEvent.DisposeAsync().DynamicContext();
            await PauseEvent.DisposeAsync().DynamicContext();
            Cancellation.Cancel();
        }
    }
}

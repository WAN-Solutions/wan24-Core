using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Pipeline stream
    /// </summary>
    public partial class PipelineStream : StreamBase
    {
        /// <summary>
        /// Input buffer processor task
        /// </summary>
        protected readonly Task? InputBufferProcessorTask = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queueCapacity">Queue capacity</param>
        /// <param name="parallelism">Degree of parallelism (how many tasks to process in parallel)</param>
        /// <param name="inputBufferSize">Input buffer size in bytes (or <see langword="null"/> to write to the first element directly)</param>
        /// <param name="outputBufferSize">Output buffer size in bytes (or <see langword="null"/>, if not readable)</param>
        /// <param name="clearBuffers">If to clear buffers after use</param>
        /// <param name="elements">Elements</param>
        public PipelineStream(
            in int queueCapacity,
            in int parallelism,
            in int? inputBufferSize,
            in int? outputBufferSize,
            in bool clearBuffers,
            params PipelineElementBase[] elements
            )
            : base()
        {
            ClearBuffers = clearBuffers;
            Queue = new(this, queueCapacity, parallelism);
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
            if (inputBufferSize.HasValue) InputBufferProcessorTask = ((Func<Task>)ProcessInputBufferAsync).StartLongRunningTask();
        }

        /// <summary>
        /// Pipeline elements (read-only)
        /// </summary>
        public FreezableOrderedDictionary<string, PipelineElementBase> Elements { get; }

        /// <summary>
        /// Processing queue (needs to be started before sending anything into the pipeline stream!)
        /// </summary>
        public ProcessingQueue Queue { get; }

        /// <summary>
        /// Input buffer
        /// </summary>
        public BlockingBufferStream? InputBuffer { get; }

        /// <summary>
        /// Output buffer
        /// </summary>
        public BlockingBufferStream? OutputBuffer { get; }

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
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureReadable();
            Logger?.LogTrace("Synchron reading {count} bytes from the output buffer", count);
            int res = OutputBuffer.Read(buffer, offset, count);
            Logger?.LogTrace("Red {res} bytes synchron from the output buffer", res);
            return res;
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            Logger?.LogTrace("Synchron reading {count} bytes from the output buffer", buffer.Length);
            int res = OutputBuffer.Read(buffer);
            Logger?.LogTrace("Red {res} bytes synchron from the output buffer", res);
            return res;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            Logger?.LogTrace("Asynchron reading {count} bytes from the output buffer", count);
            int res = await OutputBuffer.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
            Logger?.LogTrace("Red {res} bytes asynchron from the output buffer", res);
            return res;
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            Logger?.LogTrace("Asynchron reading {count} bytes from the output buffer", buffer.Length);
            int res = await OutputBuffer.ReadAsync(buffer, cancellationToken).DynamicContext();
            Logger?.LogTrace("Red {res} bytes asynchron from the output buffer", res);
            return res;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            EnsureReadable();
            Logger?.LogTrace("Synchron reading a single byte from the output buffer");
            int res = OutputBuffer.ReadByte();
            Logger?.LogTrace("Red byte {byte} synchron from the output buffer", res);
            return res;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            if (InputBuffer is not null)
            {
                Logger?.LogTrace("Write {count} bytes to the input buffer", count);
                InputBuffer.Write(buffer, offset, count);
                return;
            }
            if (Elements.Count < 1)
            {
                Logger?.LogTrace("Write {count} bytes to the output buffer", count);
                OutputBuffer?.Write(buffer, offset, count);
                return;
            }
            RentedArray<byte> processingBuffer = new(len: count, clean: false)
            {
                Clear = ClearBuffers
            };
            buffer.AsSpan(offset, count).CopyTo(processingBuffer.Span);
            ProcessingQueueItem item = new()
            {
                Buffer = processingBuffer,
                Element = Elements[0]
            };
            try
            {
                Logger?.LogDebug("Enqueue {count} bytes to the processing queue", count);
                Queue.EnqueueAsync(item).AsTask().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger?.LogError("Failed to enqueue {count} bytes to the processing queue: {ex}", count, ex);
                item.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            if (InputBuffer is not null)
            {
                Logger?.LogTrace("Write {count} bytes to the input buffer", buffer.Length);
                InputBuffer.Write(buffer);
                return;
            }
            if (Elements.Count < 1)
            {
                Logger?.LogTrace("Write {count} bytes to the output buffer", buffer.Length);
                OutputBuffer?.Write(buffer);
                return;
            }
            RentedArray<byte> processingBuffer = new(len: buffer.Length, clean: false)
            {
                Clear = ClearBuffers
            };
            buffer.CopyTo(processingBuffer.Span);
            ProcessingQueueItem item = new()
            {
                Buffer = processingBuffer,
                Element = Elements[0]
            };
            try
            {
                Logger?.LogDebug("Enqueue {count} bytes to the processing queue", buffer.Length);
                Queue.EnqueueAsync(item).AsTask().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger?.LogError("Failed to enqueue {count} bytes to the processing queue: {ex}", buffer.Length, ex);
                item.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureUndisposed();
            if (InputBuffer is not null)
            {
                Logger?.LogTrace("Write {count} bytes to the input buffer", count);
                await InputBuffer.WriteAsync(buffer, offset, count, cancellationToken).DynamicContext();
                return;
            }
            if (Elements.Count < 1)
            {
                if (OutputBuffer is not null)
                {
                    Logger?.LogTrace("Write {count} bytes to the output buffer", count);
                    await OutputBuffer.WriteAsync(buffer, offset, count, cancellationToken).DynamicContext();
                }
                return;
            }
            RentedArray<byte> processingBuffer = new(len: count, clean: false)
            {
                Clear = ClearBuffers
            };
            buffer.AsSpan(offset, count).CopyTo(processingBuffer.Span);
            ProcessingQueueItem item = new()
            {
                Buffer = processingBuffer,
                Element = Elements[0]
            };
            try
            {
                Logger?.LogDebug("Enqueue {count} bytes to the processing queue", count);
                await Queue.EnqueueAsync(item, cancellationToken).DynamicContext();
            }
            catch (Exception ex)
            {
                Logger?.LogError("Failed to enqueue {count} bytes to the processing queue: {ex}", count, ex);
                await item.DisposeAsync().DynamicContext();
                throw;
            }
        }

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (InputBuffer is not null)
            {
                Logger?.LogTrace("Write {count} bytes to the input buffer", buffer.Length);
                await InputBuffer.WriteAsync(buffer, cancellationToken).DynamicContext();
                return;
            }
            if (Elements.Count < 1)
            {
                if (OutputBuffer is not null)
                {
                    Logger?.LogTrace("Write {count} bytes to the output buffer", buffer.Length);
                    await OutputBuffer.WriteAsync(buffer, cancellationToken).DynamicContext();
                }
                return;
            }
            RentedArray<byte> processingBuffer = new(len: buffer.Length, clean: false)
            {
                Clear = ClearBuffers
            };
            buffer.Span.CopyTo(processingBuffer.Span);
            ProcessingQueueItem item = new()
            {
                Buffer = processingBuffer,
                Element = Elements[0]
            };
            try
            {
                Logger?.LogDebug("Enqueue {count} bytes to the processing queue", buffer.Length);
                await Queue.EnqueueAsync(item, cancellationToken).DynamicContext();
            }
            catch (Exception ex)
            {
                Logger?.LogError("Failed to enqueue {count} bytes to the processing queue: {ex}", buffer.Length, ex);
                await item.DisposeAsync().DynamicContext();
                throw;
            }
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            this.GenericWriteByte(value);
        }

        /// <inheritdoc/>
        [MemberNotNull(nameof(OutputBuffer))]
#pragma warning disable CS8774 // OutputBuffer must not be NULL
        protected override void EnsureReadable() => base.EnsureReadable();
#pragma warning restore CS8774 // OutputBuffer must not be NULL

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Queue.Dispose();
            Elements.Values.DisposeAll();
            InputBuffer?.Dispose();
            OutputBuffer?.Dispose();
            InputBufferProcessorTask?.GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            await Queue.DisposeAsync().DynamicContext();
            await Elements.Values.DisposeAllAsync().DynamicContext();
            if (InputBuffer is not null) await InputBuffer.DisposeAsync().DynamicContext();
            if (OutputBuffer is not null) await OutputBuffer.DisposeAsync().DynamicContext();
            if (InputBufferProcessorTask is not null) await InputBufferProcessorTask.DynamicContext();
        }
    }
}

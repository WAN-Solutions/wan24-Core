using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    // Input buffer
    public partial class PipelineStream
    {
        /// <summary>
        /// Input buffer processor task
        /// </summary>
        protected readonly Task? InputBufferProcessorTask = null;

        /// <summary>
        /// Input buffer
        /// </summary>
        public BlockingBufferStream? InputBuffer { get; }

        /// <summary>
        /// Last exception of the input buffer processor
        /// </summary>
        public Exception? LastException { get; protected set; }

        /// <summary>
        /// Write an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Object</param>
        /// <param name="disposeOnError">If to dispose the <c>value</c> on error</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task WriteObjectAsync<T>([NotNull] T value, bool disposeOnError = true, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ArgumentNullException.ThrowIfNull(value);
            await SyncEvent.WaitAsync(cancellationToken).DynamicContext();
            await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
            if (Elements.Count < 1) throw new InvalidOperationException("No processing pipeline elements");
            if (Elements[0] is not IPipelineElementObject) throw new InvalidOperationException("The first processing pipeline element can't process an object");
            if (Elements[0] is not IPipelineElementObject<T>)
                throw new InvalidOperationException($"The first processing pipeline element can't process an object of type {typeof(T)}");
            ProcessingObjectQueueItem<T> item = new()
            {
                Object = value,
                ObjectType = typeof(T),
                Element = Elements[0]
            };
            try
            {
                Logger?.LogDebug("Enqueue {type} to the processing queue", typeof(T));
                await Queue.EnqueueAsync(item, cancellationToken).DynamicContext();
            }
            catch (Exception ex)
            {
                Logger?.LogDebug("Failed to enqueue {type} to the processing queue: {ex}", typeof(T), ex);
                await item.DisposeAsync().DynamicContext();
                if (disposeOnError) await value.TryDisposeAsync().DynamicContext();
                throw;
            }
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            SyncEvent.Wait();
            PauseEvent.Wait();
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
            RentedArray<byte> processingBuffer = CreateBuffer(count);
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
            SyncEvent.Wait();
            PauseEvent.Wait();
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
            RentedArray<byte> processingBuffer = CreateBuffer(buffer.Length);
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
            await SyncEvent.WaitAsync(cancellationToken).DynamicContext();
            await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
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
            RentedArray<byte> processingBuffer =CreateBuffer(count);
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
            await SyncEvent.WaitAsync(cancellationToken).DynamicContext();
            await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
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
            RentedArray<byte> processingBuffer = CreateBuffer(buffer.Length);
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

        /// <summary>
        /// Process the <see cref="InputBuffer"/>
        /// </summary>
        protected virtual async Task ProcessInputBufferAsync()
        {
            await Task.Yield();
            if (InputBuffer is null) throw new InvalidProgramException();
            RentedArray<byte>? buffer = null;
            try
            {
                int red;
                while (!IsDisposing)
                {
                    await SyncEvent.WaitAsync(Cancellation.Token).DynamicContext();
                    await PauseEvent.WaitAsync(Cancellation.Token).DynamicContext();
                    buffer = CreateBuffer();
                    red = await ReadStreamChunkAsync(InputBuffer, buffer.Memory, Cancellation.Token).DynamicContext();
                    if (Elements.Count < 1)
                    {
                        if (OutputBuffer is not null)
                        {
                            Logger?.LogDebug("Write {count} bytes from the input buffer to the output buffer", red);
                            await OutputBuffer.WriteAsync(buffer.Memory[..red], Cancellation.Token).DynamicContext();
                        }
                        else
                        {
                            Logger?.LogWarning("Discard {count} bytes from the input buffer (nothing to do)", red);
                        }
                    }
                    else
                    {
                        ProcessingQueueItem item = new()
                        {
                            Buffer = buffer,
                            BufferLength = red,
                            Element = Elements[0]
                        };
                        try
                        {
                            Logger?.LogDebug("Enqueue {count} bytes from the input buffer to the processing queue", red);
                            await Queue.EnqueueAsync(item, Cancellation.Token).DynamicContext();
                            buffer = null;
                        }
                        catch (Exception ex)
                        {
                            Logger?.LogError("Failed to enqueue {count} bytes from the input buffer to the processing queue: {ex}", red, ex);
                            await item.DisposeAsync().DynamicContext();
                            throw;
                        }
                    }
                }
            }
            catch (ObjectDisposedException) when (IsDisposing)
            {
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken.IsEqualTo(Cancellation.Token))
            {
            }
            catch (Exception ex)
            {
                LastException = ex;
                RaiseOnError();
            }
            finally
            {
                if (buffer is not null) await buffer.DisposeAsync().DynamicContext();
            }
        }

        /// <summary>
        /// Delegate for an <see cref="OnError"/> event handler
        /// </summary>
        /// <param name="pipeline">Pipeline</param>
        /// <param name="e">Arguments</param>
        public delegate void Error_Delegate(PipelineStream pipeline, EventArgs e);
        /// <summary>
        /// Raised on error
        /// </summary>
        public event Error_Delegate? OnError;
        /// <summary>
        /// Raise the see <see cref="OnError"/> event (see <see cref="LastException"/>)
        /// </summary>
        protected virtual void RaiseOnError() => OnError?.Invoke(this, EventArgs.Empty);
    }
}

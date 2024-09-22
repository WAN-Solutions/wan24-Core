using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    // Input buffer processor
    public partial class PipelineStream
    {
        /// <summary>
        /// Last exception of the input buffer processor
        /// </summary>
        public Exception? LastException { get; protected set; }

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
                    buffer = new(len: Settings.BufferSize, clean: false)
                    {
                        Clear = ClearBuffers
                    };
                    red = await InputBuffer.ReadAsync(buffer.Memory).DynamicContext();
                    if (Elements.Count < 1)
                    {
                        if (OutputBuffer is not null)
                        {
                            Logger?.LogDebug("Write {count} bytes from the input buffer to the output buffer", red);
                            await OutputBuffer.WriteAsync(buffer.Memory[..red]).DynamicContext();
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
                            Element = Elements[0]
                        };
                        try
                        {
                            Logger?.LogDebug("Enqueue {count} bytes from the input buffer to the processing queue", red);
                            Queue.EnqueueAsync(item).AsTask().GetAwaiter().GetResult();
                            buffer = null;
                        }
                        catch (Exception ex)
                        {
                            Logger?.LogError("Failed to enqueue {count} bytes from the input buffer to the processing queue: {ex}", red, ex);
                            item.Dispose();
                            throw;
                        }
                    }
                }
            }
            catch(ObjectDisposedException) when (IsDisposing)
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

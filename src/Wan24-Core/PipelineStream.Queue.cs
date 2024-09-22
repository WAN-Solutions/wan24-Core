using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    // Queue
    public abstract partial class PipelineStream
    {
        /// <summary>
        /// Processing queue
        /// </summary>
        public class ProcessingQueue : ParallelItemQueueWorkerBase<ProcessingQueueItem>
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="pipeline">Pipeline</param>
            /// <param name="parallelism">Degree of parallelism (how many tasks to process in parallel)</param>
            public ProcessingQueue(in PipelineStream pipeline, in int parallelism) : base(parallelism, parallelism)
            {
                Pipeline = pipeline;
                CanPause = true;
            }

            /// <summary>
            /// Pipeline
            /// </summary>
            public PipelineStream Pipeline { get; }

            /// <summary>
            /// Logger
            /// </summary>
            public ILogger? Logger => Pipeline.Logger;

            /// <inheritdoc/>
            protected override async Task ProcessItem(ProcessingQueueItem item, CancellationToken cancellationToken)
            {
                Logger?.LogDebug("Processing queued item for element #{pos}", item.Element.Position);
                PipelineResultBase? result = null;
                try
                {
                    if (!CancelToken.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                    {
                        if (item.Buffer is null && item.Result is null) throw new InvalidProgramException("No result and buffer");
                        result = item.Buffer is null
                            ? await item.Element.ProcessAsync(item.Result!, cancellationToken).DynamicContext()
                            : await item.Element.ProcessAsync(item.Buffer.Memory, cancellationToken).DynamicContext();
                        if(result is null)
                        {
                            // Processing stops here
                            Logger?.LogDebug("Processing queued item for element #{pos} has no result", item.Element.Position);
                            await item.DisposeAsync().DynamicContext();
                        }
                        else if(result.Next is null)
                        {
                            // Try to write the result to the output buffer of the pipeline
                            Logger?.LogDebug("Processing queued item for element #{pos} has no next element", item.Element.Position);
                            try
                            {
                                if (Pipeline.OutputBuffer is not null)
                                {
                                    // Process a result buffer
                                    if (result is IPipelineResultBuffer resultBuffer)
                                    {
                                        Logger?.LogDebug("Processing queued item for element #{pos} has no next element, but an output buffer", item.Element.Position);
                                        await Pipeline.OutputBuffer.WriteAsync(resultBuffer.Buffer, cancellationToken).DynamicContext();
                                    }
                                    // Process a result stream
                                    if(result is IPipelineResultStream resultStream)
                                        if (resultStream.Stream.CanSeek)
                                        {
                                            Logger?.LogDebug("Processing queued item for element #{pos} has no next element, but a seekable output stream", item.Element.Position);
                                            await resultStream.Stream.CopyExactlyPartialToAsync(
                                                Pipeline.OutputBuffer,
                                                resultStream.Stream.GetRemainingBytes(),
                                                cancellationToken: cancellationToken
                                                )
                                                .DynamicContext();
                                        }
                                        else
                                        {
                                            Logger?.LogDebug("Processing queued item for element #{pos} has no next element, but an output stream", item.Element.Position);
                                            await resultStream.Stream.CopyToAsync(Pipeline.OutputBuffer, cancellationToken).DynamicContext();
                                        }
                                }
                            }
                            finally
                            {
                                await item.DisposeAsync().DynamicContext();
                                await result.DisposeAsync().DynamicContext();
                            }
                        }
                        else
                        {
                            // Further result processing
                            Logger?.LogDebug("Processing queued item for element #{pos} has next element #{next}", item.Element.Position, result.Next.Position);
                            EnqueueResult(item, result);
                        }
                    }
                    else
                    {
                        // Cancelled
                        Logger?.LogWarning("Processing queued item for element #{pos} was cancelled", item.Element.Position);
                        await item.DisposeAsync().DynamicContext();
                    }
                }
                catch(Exception ex)
                {
                    Logger?.LogError("Processing queued item for element #{pos} failed exceptional: {ex}", item.Element.Position, ex);
                    await item.DisposeAsync().DynamicContext();
                    if (result is not null) await result.DisposeAsync().DynamicContext();
                    throw;
                }
            }

            /// <summary>
            /// Enqueue an element processing result for further processing
            /// </summary>
            /// <param name="item">Producing item</param>
            /// <param name="result">Result</param>
            protected virtual async void EnqueueResult(ProcessingQueueItem item, PipelineResultBase result)
            {
                await Task.Yield();
                ProcessingQueueItem newItem = new()
                {
                    Result = result,
                    PreviousElement = result.Element,
                    Element = result.Next!
                };
                try
                {
                    await EnqueueAsync(newItem, CancelToken).DynamicContext();
                    Logger?.LogTrace("Enqueueing next element #{next} processing from element #{pos} succeed", result.Next!.Position, item.Element.Position);
                }
                catch (Exception ex)
                {
                    Logger?.LogError("Enqueueing next element #{next} processing from element #{pos} result failed exceptional: {ex}", result.Next!.Position, item.Element.Position, ex);
                    await newItem.DisposeAsync().DynamicContext();
                    await result.DisposeAsync().DynamicContext();
                    LastException = ex;
                    StoppedExceptional = true;
                    await StopAsync().DynamicContext();
                }
                finally
                {
                    await item.DisposeAsync().DynamicContext();
                }
            }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                while (Queue.Reader.TryRead(out Task_Delegate? item))
                    item(CancellationToken.None).AsTask().GetAwaiter().GetResult();
            }

            /// <inheritdoc/>
            protected override async Task DisposeCore()
            {
                await base.DisposeCore().DynamicContext();
                while (Queue.Reader.TryRead(out Task_Delegate? item))
                    await item(CancellationToken.None).DynamicContext();
            }
        }

        /// <summary>
        /// Processing queue item
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        public class ProcessingQueueItem() : DisposableBase()
        {
            /// <summary>
            /// Buffer to process
            /// </summary>
            public RentedArray<byte>? Buffer { get; init; }

            /// <summary>
            /// Previous pipeline element result
            /// </summary>
            public PipelineResultBase? Result { get; init; }

            /// <summary>
            /// Previous element
            /// </summary>
            public PipelineElementBase? PreviousElement { get; init; }

            /// <summary>
            /// Next element
            /// </summary>
            public required PipelineElementBase Element { get; init; }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                Buffer?.Dispose();
                Result?.Dispose();
            }

            /// <inheritdoc/>
            protected override async Task DisposeCore()
            {
                if (Buffer is not null) await Buffer.DisposeAsync().DynamicContext();
                if (Result is not null) await Result.DisposeAsync().DynamicContext();
            }
        }
    }
}

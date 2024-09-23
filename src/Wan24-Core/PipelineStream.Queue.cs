using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace wan24.Core
{
    // Queue
    public partial class PipelineStream
    {
        /// <summary>
        /// Processing queue (needs to be started before sending anything into the pipeline stream!)
        /// </summary>
        public ProcessingQueue Queue { get; }

        /// <summary>
        /// Processing queue
        /// </summary>
        public class ProcessingQueue : ParallelItemQueueWorkerBase<ProcessingQueueItem>
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="pipeline">Pipeline</param>
            /// <param name="capacity">Capacity</param>
            /// <param name="parallelism">Degree of parallelism (how many tasks to process in parallel)</param>
            public ProcessingQueue(in PipelineStream pipeline, in int capacity, in int parallelism) : base(capacity, parallelism)
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
                IProcessingObjectQueueItem? objectItem = null;
                TypeInfoExt elementInterface,
                    itemInterface;
                PropertyInfoExt itemObjectProperty;
                MethodInfoExt processObjectMethod;
                object? returnValue;
                try
                {
                    if (!CancelToken.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                    {
                        while (true)
                        {
                            await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
                            await Pipeline.SyncEvent.WaitAsync(cancellationToken).DynamicContext();
                            await Pipeline.PauseEvent.WaitAsync(cancellationToken).DynamicContext();
                            objectItem = null;
                            if (item.Buffer is null && item.Result is null && (objectItem = item as IProcessingObjectQueueItem) is null)
                                throw new InvalidProgramException("No result and buffer");
                            // Call the processor method of the pipeline stream element
                            if (objectItem is null)
                            {
                                // Buffer or result processing
                                Logger?.LogDebug("Processing queued {type} item for element #{pos}", item.Buffer is null ? "Result" : "Buffer", item.Element.Position);
                                result = item.Buffer is null
                                    ? await item.Element.ProcessAsync(item.Result!, cancellationToken).DynamicContext()
                                    : await item.Element.ProcessAsync(
                                        item.BufferLength.HasValue
                                            ? item.Buffer.Memory[..item.BufferLength.Value]
                                            : item.Buffer.Memory,
                                        cancellationToken
                                        )
                                        .DynamicContext();
                            }
                            else
                            {
                                // Object processing
                                Logger?.LogDebug("Processing queued object ({type}) item for element #{pos}", objectItem.ObjectType, item.Element.Position);
                                elementInterface = TypeInfoExt.From(typeof(IPipelineElementObject<>)).MakeGenericType(objectItem.ObjectType);
                                if (!elementInterface.Type.IsAssignableFrom(item.Element.GetType()))
                                    throw new InvalidDataException($"Pipeline element {item.Element.GetType()} can't process an object of type {objectItem.ObjectType}");
                                itemInterface = TypeInfoExt.From(typeof(ProcessingObjectQueueItem<>)).MakeGenericType(objectItem.ObjectType);
                                if (!itemInterface.Type.IsAssignableFrom(item.GetType()))
                                    throw new InvalidDataException($"Pipeline processor queue item {item.GetType()} doesn't host an object of type {objectItem.ObjectType}");
                                processObjectMethod = elementInterface.Type.GetMethodCached(nameof(IPipelineElementObject<object>.ProcessAsync))
                                    ?? throw new InvalidProgramException($"Failed to get object processing method from {elementInterface.Type}");
                                if (processObjectMethod.Invoker is null)
                                    throw new InvalidProgramException($"Failed to get object processing method with an invoker from {elementInterface.Type}");
                                itemObjectProperty = itemInterface.Type.GetPropertyCached(nameof(ProcessingObjectQueueItem<object>.Object))
                                    ?? throw new InvalidProgramException($"Failed to get object property from {itemInterface.Type}");
                                if (itemObjectProperty.Getter is null)
                                    throw new InvalidProgramException($"Failed to get object property with a getter from {itemInterface.Type}");
                                returnValue = processObjectMethod.Invoker(
                                    item.Element,
                                    [
                                        itemObjectProperty.Getter(item) ?? throw new InvalidDataException($"Failed to get non-null object value from {itemObjectProperty.FullName}"),
                                        cancellationToken
                                    ]
                                    );
                                result = returnValue is null
                                    ? null
                                    : await TaskHelper.GetAnyTaskResultAsync(returnValue).DynamicContext() as PipelineResultBase;
                            }
                            // Handle the processing result
                            if (result is null)
                            {
                                // Processing stops here
                                Logger?.LogDebug("Processing queued item for element #{pos} has no result", item.Element.Position);
                                await item.DisposeAsync().DynamicContext();
                            }
                            else if (result.Next is null)
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
                                        if (result is IPipelineResultStream resultStream)
                                        {
                                            Logger?.LogDebug("Processing queued item for element #{pos} has no next element, but an output stream", item.Element.Position);
                                            await Pipeline.CopyStreamAsync(resultStream.Stream, Pipeline.OutputBuffer, cancellationToken).DynamicContext();
                                        }
                                    }
                                }
                                finally
                                {
                                    await item.DisposeAsync().DynamicContext();
                                    await result.DisposeAsync().DynamicContext();
                                }
                            }
                            else if (result.ProcessInParallel)
                            {
                                // Further parallel result processing
                                Logger?.LogDebug("Processing queued item for element #{pos} has next element #{next}", item.Element.Position, result.Next.Position);
                                EnqueueResult(item, result);
                            }
                            else
                            {
                                // Further non-parallel result processing
                                PipelineResultBase? newResult = await result.Next.ProcessAsync(result, cancellationToken).DynamicContext();
                                try
                                {
                                    await item.DisposeAsync().DynamicContext();
                                    item = new()
                                    {
                                        PreviousElement = result.Element,
                                        Element = result.Next,
                                        Result = result
                                    };
                                    await result.DisposeAsync().DynamicContext();
                                    result = newResult;
                                    continue;
                                }
                                catch
                                {
                                    if(newResult is not null) await newResult.DisposeAsync().DynamicContext();
                                    throw;
                                }
                            }
                            break;
                        }
                    }
                    else
                    {
                        // Cancelled
                        await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
                        await Pipeline.SyncEvent.WaitAsync(cancellationToken).DynamicContext();
                        await Pipeline.PauseEvent.WaitAsync(cancellationToken).DynamicContext();
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
                await PauseEvent.WaitAsync(CancelToken).DynamicContext();
                await Pipeline.SyncEvent.WaitAsync(CancelToken).DynamicContext();
                await Pipeline.PauseEvent.WaitAsync(CancelToken).DynamicContext();
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
            /// Buffer length in bytes
            /// </summary>
            public int? BufferLength { get; init; }

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

        /// <summary>
        /// Interface for a processing queue item which hosts an object to process
        /// </summary>
        public interface IProcessingObjectQueueItem : IDisposableObject
        {
            /// <summary>
            /// Processed object type
            /// </summary>
            Type ObjectType { get; }
        }

        /// <summary>
        /// Processing queue item which hosts an object to process
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <remarks>
        /// Constructor
        /// </remarks>
        public class ProcessingObjectQueueItem<T>() : ProcessingQueueItem(), IProcessingObjectQueueItem
        {
            /// <summary>
            /// Object to process
            /// </summary>
            [NotNull]
            public required T Object { get; init; }

            /// <inheritdoc/>
            public required Type ObjectType { get; init; }
        }
    }
}

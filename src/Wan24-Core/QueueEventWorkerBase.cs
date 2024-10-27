using wan24.Core.Enumerables;
using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a queue event worker (works on the next item when idle or getting a signal; may process items in parallel, depending on the time an event was raised, 
    /// and if any upcoming item is available for processing at that time)
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public abstract class QueueEventWorkerBase<T> : ItemQueueWorkerBase<T>, IQueueEventWorker
    {
        /// <summary>
        /// Queue worker event (raised when the next item should be processed)
        /// </summary>
        protected readonly ResetEvent QueueWorkerEvent = new(initialState: true);
        /// <summary>
        /// Active item worker tasks
        /// </summary>
        protected readonly ArrayWhereEnumerable<Task?> ActiveItemWorkerTasks;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="maxThreads">Maximum number of threads for parallel processing</param>
        protected QueueEventWorkerBase(in int capacity, int maxThreads = 0) : base(capacity)
        {
            if (maxThreads < 1) maxThreads = Environment.ProcessorCount;
            ActiveItemWorkerTasks = new(new Task[maxThreads], static t => t is not null);
            CanPause = true;
        }

        /// <inheritdoc/>
        public int MaxThreads => ActiveItemWorkerTasks.Length;

        /// <inheritdoc/>
        public int ProcessingItems => ActiveItemWorkerTasks.Count();

        /// <inheritdoc/>
        public override IEnumerable<Status> State
        {
            get
            {
                foreach (Status status in base.State) yield return status;
                yield return new(__("Max. threads"), MaxThreads, __("Maximum number of threads for parallel processing"));
                yield return new(__("Processing"), ProcessingItems, __("Number of currently processed items"));
                yield return new(__("Event"), QueueWorkerEvent.IsSet, __("If the queue worker event is set for processing the next queued item"));
            }
        }

        /// <summary>
        /// Item processor (has to call <see cref="Task.Yield"/> at the very beginning and raise the <see cref="QueueWorkerEvent"/> once; may run with others in parallel)
        /// </summary>
        /// <param name="item">Item to process</param>
        protected abstract Task ItemProcessorAsync(T item);

        /// <inheritdoc/>
        protected sealed override async Task ProcessItem(T item, CancellationToken cancellationToken)
        {
            // Wait for a processing signal and reset the event
            await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
            await QueueWorkerEvent.WaitAndResetAsync(cancellationToken).DynamicContext();
            // Wait for a free processing slot
            Task?[] tasks = ActiveItemWorkerTasks.Array;
            if (ActiveItemWorkerTasks.Count(t => !t!.IsCompleted) == tasks.Length)
                await Task.WhenAny(ActiveItemWorkerTasks.Array!).DynamicContext();
            // Start processing and clean up the active task list (stop processing on any error)
            bool isProcessingItem = false;
            Task? task;
            for (int i = 0, len = tasks.Length; i < len; i++)
            {
                // Check a task slot
                task = tasks[i];
                if (task is not null)
                {
                    // Ensure the current task was completed
                    if (!task.IsCompleted) continue;
                    // Stop processing on any error
                    if (!task.IsCompletedSuccessfully)
                        try
                        {
                            await Task.WhenAll(ActiveItemWorkerTasks!).DynamicContext();
                        }
                        finally
                        {
                            Array.Clear(tasks);
                        }
                }
                // Remove the completed task from the list and/or continue with the next slot, if processing the item already
                if (isProcessingItem)
                {
                    if (task is not null) tasks[i] = null;
                    continue;
                }
                // Start processing the item using the current slot
                await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
                tasks[i] = ItemProcessorAsync(item);
                isProcessingItem = true;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            QueueWorkerEvent.Dispose();
            base.Dispose(disposing);
            if (ActiveItemWorkerTasks.Any())
                try
                {
                    Task.WaitAll(ActiveItemWorkerTasks.ToArray()!);
                }
                catch (Exception ex)
                {
                    LastException = LastException is Exception exception
                        ? exception.Append(ex)
                        : ex;
                }
                finally
                {
                    Array.Clear(ActiveItemWorkerTasks.Array);
                }
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await QueueWorkerEvent.DisposeAsync().DynamicContext();
            await base.DisposeCore().DynamicContext();
            if (ActiveItemWorkerTasks.Any())
                try
                {
                    await Task.WhenAll(ActiveItemWorkerTasks!).DynamicContext();
                }
                catch (Exception ex)
                {
                    LastException = LastException is Exception exception
                        ? exception.Append(ex)
                        : ex;
                }
                finally
                {
                    Array.Clear(ActiveItemWorkerTasks.Array);
                }
        }
    }
}

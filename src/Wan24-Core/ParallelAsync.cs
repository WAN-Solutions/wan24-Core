﻿namespace wan24.Core
{
    /// <summary>
    /// Asynchronous parallel execution
    /// </summary>
    public static class ParallelAsync
    {
        /// <summary>
        /// For each
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="items">Items</param>
        /// <param name="itemHandler">Item handler</param>
        /// <param name="threads">Number of threads to use (<see langword="null"/> to use the number of available CPU cores)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task ForEachAsync<T>(IEnumerable<T> items, Func<T, CancellationToken, Task> itemHandler, int? threads = null, CancellationToken cancellationToken = default)
        {
            if (threads != null && threads < 1) throw new ArgumentOutOfRangeException(nameof(threads));
            using ParallelAsyncProcessor<T> processor = new(itemHandler, threads ?? Environment.ProcessorCount);
            await processor.StartAsync(cancellationToken).DynamicContext();
            await processor.EnqueueRangeAsync(items, cancellationToken).DynamicContext();
            await processor.WaitBoringAsync(cancellationToken).DynamicContext();
            await processor.StopAsync(cancellationToken).DynamicContext();
        }

        /// <summary>
        /// For each
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="items">Items</param>
        /// <param name="itemHandler">Item handler</param>
        /// <param name="threads">Number of threads to use (<see langword="null"/> to use the number of available CPU cores)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task ForEachAsync<T>(IAsyncEnumerable<T> items, Func<T, CancellationToken, Task> itemHandler, int? threads = null, CancellationToken cancellationToken = default)
        {
            if (threads != null && threads < 1) throw new ArgumentOutOfRangeException(nameof(threads));
            using ParallelAsyncProcessor<T> processor = new(itemHandler, threads ?? Environment.ProcessorCount);
            await processor.StartAsync(cancellationToken).DynamicContext();
            await processor.EnqueueRangeAsync(items, cancellationToken).DynamicContext();
            await processor.WaitBoringAsync(cancellationToken).DynamicContext();
            await processor.StopAsync(cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Asynchronous parallel item processor
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        internal sealed class ParallelAsyncProcessor<T> : ParallelItemQueueWorker<T>
        {
            /// <summary>
            /// Item handler
            /// </summary>
            private readonly Func<T, CancellationToken, Task> ItemHandler;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="itemHandler">Item handler</param>
            /// <param name="threads">Number of threads to use</param>
            internal ParallelAsyncProcessor(Func<T, CancellationToken, Task> itemHandler, int threads) : base(threads, threads) => ItemHandler = itemHandler;

            /// <inheritdoc/>
            protected override Task ProcessItem(T item, CancellationToken cancellationToken) => ItemHandler(item, cancellationToken);
        }
    }
}

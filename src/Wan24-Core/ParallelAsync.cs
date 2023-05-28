using System.Runtime.CompilerServices;

namespace wan24.Core
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
        public static async Task ForEachAsync<T>(
            IEnumerable<T> items, 
            Func<T, CancellationToken, Task> itemHandler, 
            int? threads = null, 
            CancellationToken cancellationToken = default
            )
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
        public static async Task ForEachAsync<T>(
            IAsyncEnumerable<T> items, 
            Func<T, CancellationToken, Task> itemHandler, 
            int? threads = null, 
            CancellationToken cancellationToken = default
            )
        {
            if (threads != null && threads < 1) throw new ArgumentOutOfRangeException(nameof(threads));
            using ParallelAsyncProcessor<T> processor = new(itemHandler, threads ?? Environment.ProcessorCount);
            await processor.StartAsync(cancellationToken).DynamicContext();
            await processor.EnqueueRangeAsync(items, cancellationToken).DynamicContext();
            await processor.WaitBoringAsync(cancellationToken).DynamicContext();
            await processor.StopAsync(cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Enumerate parallel filtered items (in an unspecified order)
        /// </summary>
        /// <typeparam name="tInput">Input type</typeparam>
        /// <typeparam name="tOutput">Output type</typeparam>
        /// <param name="items">Items</param>
        /// <param name="itemFilter">Filter function (returns <see langword="false"/> and a default to skip output, or <see langword="true"/> to yield the output)</param>
        /// <param name="threads">Number of threads to use (<see langword="null"/> to use the number of available CPU cores)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Filtered items</returns>
        public static async IAsyncEnumerable<tOutput> FilterAsync<tInput, tOutput>(
            IEnumerable<tInput> items,
            Func<tInput, CancellationToken, Task<(bool Yield, tOutput Output)>> itemFilter,
            int? threads = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            using SemaphoreSlim syncYield = new(1, 1);
            using SemaphoreSlim syncOutput = new(1, 1);
            syncOutput.Wait(cancellationToken);
            tOutput output = default!;
            bool done = false;
            ConfiguredTaskAwaitable filter = Task.Factory.StartNew(async () =>
            {
                try
                {
                    await ForEachAsync(items, async (item, ct) =>
                    {
                        (bool yieldOutput, tOutput outItem) = await itemFilter(item, ct).DynamicContext();
                        if (!yieldOutput) return;
                        await syncYield.WaitAsync(ct).DynamicContext();
                        output = outItem;
                        syncOutput.Release();
                    }, threads, cancellationToken).DynamicContext();
                }
                finally
                {
                    done = true;
                    syncOutput.Release();
                }
            }, cancellationToken, TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Current)
                .Unwrap()
                .DynamicContext();
            try
            {
                while (!done)
                {
                    await syncOutput.WaitAsync(cancellationToken).DynamicContext();
                    if (done) break;
                    yield return output;
                    syncYield.Release();
                }
            }
            finally
            {
                await filter;
            }
        }

        /// <summary>
        /// Enumerate parallel filtered items (in an unspecified order)
        /// </summary>
        /// <typeparam name="tInput">Input type</typeparam>
        /// <typeparam name="tOutput">Output type</typeparam>
        /// <param name="items">Items</param>
        /// <param name="itemFilter">Filter function (returns <see langword="false"/> and a default to skip output, or <see langword="true"/> to yield the output)</param>
        /// <param name="threads">Number of threads to use (<see langword="null"/> to use the number of available CPU cores)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Filtered items</returns>
        public static async IAsyncEnumerable<tOutput> FilterAsync<tInput, tOutput>(
            IAsyncEnumerable<tInput> items, 
            Func<tInput, CancellationToken, Task<(bool Yield, tOutput Output)>> itemFilter,
            int? threads = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            using SemaphoreSlim syncYield = new(1, 1);
            using SemaphoreSlim syncOutput = new(1, 1);
            syncOutput.Wait(cancellationToken);
            tOutput output = default!;
            bool done = false;
            ConfiguredTaskAwaitable filter = Task.Factory.StartNew(async () =>
            {
                try
                {
                    await ForEachAsync(items, async (item, ct) =>
                    {
                        (bool yieldOutput, tOutput outItem) = await itemFilter(item, ct).DynamicContext();
                        if (!yieldOutput) return;
                        await syncYield.WaitAsync(ct).DynamicContext();
                        output = outItem;
                        syncOutput.Release();
                    }, threads, cancellationToken).DynamicContext();
                }
                finally
                {
                    done = true;
                    syncOutput.Release();
                }
            }, cancellationToken, TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Current)
                .Unwrap()
                .DynamicContext();
            try
            {
                while (!done)
                {
                    await syncOutput.WaitAsync(cancellationToken).DynamicContext();
                    if (done) break;
                    yield return output;
                    syncYield.Release();
                }
            }
            finally
            {
                await filter;
            }
        }

        /// <summary>
        /// Enumerate parallel filtered items (in an unspecified order)
        /// </summary>
        /// <typeparam name="tInput">Input type</typeparam>
        /// <typeparam name="tOutput">Output type</typeparam>
        /// <param name="items">Items</param>
        /// <param name="itemFilter">Filter function (returns <see langword="false"/> and a default to skip output, or <see langword="true"/> to yield the output)</param>
        /// <param name="threads">Number of threads to use (<see langword="null"/> to use the number of available CPU cores)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Filtered items</returns>
        public static async IAsyncEnumerable<tOutput> FilterAsync<tInput, tOutput>(
            IEnumerable<tInput> items,
            Func<tInput, CancellationToken, (bool Yield, tOutput Output)> itemFilter,
            int? threads = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            using SemaphoreSlim syncYield = new(1, 1);
            using SemaphoreSlim syncOutput = new(1, 1);
            syncOutput.Wait(cancellationToken);
            tOutput output = default!;
            bool done = false;
            ConfiguredTaskAwaitable filter = Task.Factory.StartNew(async () =>
            {
                try
                {
                    await ForEachAsync(items, async (item, ct) =>
                    {
                        await Task.Yield();
                        (bool yieldOutput, tOutput outItem) = itemFilter(item, ct);
                        if (!yieldOutput) return;
                        await syncYield.WaitAsync(ct).DynamicContext();
                        output = outItem;
                        syncOutput.Release();
                    }, threads, cancellationToken).DynamicContext();
                }
                finally
                {
                    done = true;
                    syncOutput.Release();
                }
            }, cancellationToken, TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Current)
                .Unwrap()
                .DynamicContext();
            try
            {
                while (!done)
                {
                    await syncOutput.WaitAsync(cancellationToken).DynamicContext();
                    if (done) break;
                    yield return output;
                    syncYield.Release();
                }
            }
            finally
            {
                await filter;
            }
        }

        /// <summary>
        /// Enumerate parallel filtered items (in an unspecified order)
        /// </summary>
        /// <typeparam name="tInput">Input type</typeparam>
        /// <typeparam name="tOutput">Output type</typeparam>
        /// <param name="items">Items</param>
        /// <param name="itemFilter">Filter function (returns <see langword="false"/> and a default to skip output, or <see langword="true"/> to yield the output)</param>
        /// <param name="threads">Number of threads to use (<see langword="null"/> to use the number of available CPU cores)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Filtered items</returns>
        public static async IAsyncEnumerable<tOutput> FilterAsync<tInput, tOutput>(
            IAsyncEnumerable<tInput> items,
            Func<tInput, CancellationToken, (bool Yield, tOutput Output)> itemFilter,
            int? threads = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            using SemaphoreSlim syncYield = new(1, 1);
            using SemaphoreSlim syncOutput = new(1, 1);
            syncOutput.Wait(cancellationToken);
            tOutput output = default!;
            bool done = false;
            ConfiguredTaskAwaitable filter = Task.Factory.StartNew(async () =>
            {
                try
                {
                    await ForEachAsync(items, async (item, ct) =>
                    {
                        await Task.Yield();
                        (bool yieldOutput, tOutput outItem) = itemFilter(item, ct);
                        if (!yieldOutput) return;
                        await syncYield.WaitAsync(ct).DynamicContext();
                        output = outItem;
                        syncOutput.Release();
                    }, threads, cancellationToken).DynamicContext();
                }
                finally
                {
                    done = true;
                    syncOutput.Release();
                }
            }, cancellationToken, TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Current)
                .Unwrap()
                .DynamicContext();
            try
            {
                while (!done)
                {
                    await syncOutput.WaitAsync(cancellationToken).DynamicContext();
                    if (done) break;
                    yield return output;
                    syncYield.Release();
                }
            }
            finally
            {
                await filter;
            }
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

using System.Diagnostics;
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
        /// <param name="queueCapacity">Queue capacity</param>
        /// <param name="threads">Number of threads to use (<see langword="null"/> to use the number of available CPU cores)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of processed items</returns>
        public static async Task<int> ForEachAsync<T>(
            this IEnumerable<T> items,
            Func<T, CancellationToken, Task> itemHandler,
            int queueCapacity = int.MaxValue,
            int? threads = null,
            CancellationToken cancellationToken = default
            )
        {
            if (queueCapacity < 1) throw new ArgumentOutOfRangeException(nameof(queueCapacity));
            threads ??= Math.Min(queueCapacity, Environment.ProcessorCount);
            if (threads < 1) throw new ArgumentOutOfRangeException(nameof(threads));
            if (queueCapacity < threads) throw new ArgumentOutOfRangeException(nameof(queueCapacity));
            using ParallelAsyncProcessor<T> processor = new(itemHandler, queueCapacity, threads.Value);
            await processor.StartAsync(cancellationToken).DynamicContext();
            int enqueued;
            try
            {
                enqueued = await processor.EnqueueRangeAsync(items, cancellationToken).DynamicContext();
                while (!cancellationToken.IsCancellationRequested && processor.Processed != enqueued && processor.LastException == null)
                    await processor.WaitBoringAsync(cancellationToken).DynamicContext();
            }
            finally
            {
#pragma warning disable CA2016 // Forward cancellation token
                await processor.StopAsync().DynamicContext();
#pragma warning restore CA2016 // Forward cancellation token
            }
            if (processor.Processed != enqueued)//FIXME Happens during running the tests on Linux from time to time!?
            {
                Debugger.Break();
                throw new InvalidProgramException($"{enqueued} items enqueued, but only {processor.Processed} processed");
            }
            return processor.Processed;
        }

        /// <summary>
        /// For each
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="items">Items</param>
        /// <param name="itemHandler">Item handler</param>
        /// <param name="queueCapacity">Queue capacity</param>
        /// <param name="threads">Number of threads to use (<see langword="null"/> to use the number of available CPU cores)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of processed items</returns>
        public static async Task<int> ForEachAsync<T>(
            this IAsyncEnumerable<T> items,
            Func<T, CancellationToken, Task> itemHandler,
            int queueCapacity = int.MaxValue,
            int? threads = null,
            CancellationToken cancellationToken = default
            )
        {
            if (queueCapacity < 1) throw new ArgumentOutOfRangeException(nameof(queueCapacity));
            threads ??= Math.Min(queueCapacity, Environment.ProcessorCount);
            if (threads < 1) throw new ArgumentOutOfRangeException(nameof(threads));
            if (queueCapacity < threads) throw new ArgumentOutOfRangeException(nameof(queueCapacity));
            using ParallelAsyncProcessor<T> processor = new(itemHandler, queueCapacity, threads.Value);
            await processor.StartAsync(cancellationToken).DynamicContext();
            int enqueued;
            try
            {
                enqueued = await processor.EnqueueRangeAsync(items, cancellationToken).DynamicContext();
                while (!cancellationToken.IsCancellationRequested && processor.Processed != enqueued && processor.LastException == null)
                    await processor.WaitBoringAsync(cancellationToken).DynamicContext();
            }
            finally
            {
#pragma warning disable CA2016 // Forward cancellation token
                await processor.StopAsync().DynamicContext();
#pragma warning restore CA2016 // Forward cancellation token
            }
            if (processor.Processed != enqueued)//FIXME Happens during running the tests on Linux from time to time!?
            {
                Debugger.Break();
                throw new InvalidProgramException($"{enqueued} items enqueued, but only {processor.Processed} processed");
            }
            return processor.Processed;
        }

        /// <summary>
        /// Enumerate parallel filtered items (in an unspecified order)
        /// </summary>
        /// <typeparam name="tInput">Input type</typeparam>
        /// <typeparam name="tOutput">Output type</typeparam>
        /// <param name="items">Items</param>
        /// <param name="itemFilter">Filter function (returns <see langword="false"/> and a default to skip output, or <see langword="true"/> to yield the output)</param>
        /// <param name="queueCapacity">Queue capacity</param>
        /// <param name="threads">Number of threads to use (<see langword="null"/> to use the number of available CPU cores)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Filtered items</returns>
        public static async IAsyncEnumerable<tOutput> FilterAsync<tInput, tOutput>(
            this IEnumerable<tInput> items,
            Func<tInput, CancellationToken, Task<(bool Yield, tOutput Output)>> itemFilter,
            int queueCapacity = int.MaxValue,
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
                        tOutput outItem;
                        try
                        {
                            (bool yieldOutput, outItem) = await itemFilter(item, ct).DynamicContext();
                            if (!yieldOutput) return;
                            try
                            {
                                await syncYield.WaitAsync(ct).DynamicContext();
                            }
                            catch (ObjectDisposedException)
                            {
                                return;
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            if (ex.CancellationToken != cancellationToken) throw;
                            return;
                        }
                        output = outItem;
                        syncOutput.Release();
                    }, queueCapacity, threads, cancellationToken).DynamicContext();
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
                    try
                    {
                        await syncOutput.WaitAsync(cancellationToken).DynamicContext();
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken != cancellationToken) throw;
                        break;
                    }
                    if (done) break;
                    yield return output;
                    syncYield.Release();
                }
            }
            finally
            {
                syncYield.Dispose();
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
        /// <param name="queueCapacity">Queue capacity</param>
        /// <param name="threads">Number of threads to use (<see langword="null"/> to use the number of available CPU cores)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Filtered items</returns>
        public static async IAsyncEnumerable<tOutput> FilterAsync<tInput, tOutput>(
            this IAsyncEnumerable<tInput> items,
            Func<tInput, CancellationToken, Task<(bool Yield, tOutput Output)>> itemFilter,
            int queueCapacity = int.MaxValue,
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
                        tOutput outItem;
                        try
                        {
                            (bool yieldOutput, outItem) = await itemFilter(item, ct).DynamicContext();
                            if (!yieldOutput) return;
                            try
                            {
                                await syncYield.WaitAsync(ct).DynamicContext();
                            }
                            catch (ObjectDisposedException)
                            {
                                return;
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            if (ex.CancellationToken != cancellationToken) throw;
                            return;
                        }
                        output = outItem;
                        syncOutput.Release();
                    }, queueCapacity, threads, cancellationToken).DynamicContext();
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
                    try
                    {
                        await syncOutput.WaitAsync(cancellationToken).DynamicContext();
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken != cancellationToken) throw;
                        break;
                    }
                    if (done) break;
                    yield return output;
                    syncYield.Release();
                }
            }
            finally
            {
                syncYield.Dispose();
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
        /// <param name="queueCapacity">Queue capacity</param>
        /// <param name="threads">Number of threads to use (<see langword="null"/> to use the number of available CPU cores)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Filtered items</returns>
        public static async IAsyncEnumerable<tOutput> FilterAsync<tInput, tOutput>(
            this IEnumerable<tInput> items,
            Func<tInput, CancellationToken, (bool Yield, tOutput Output)> itemFilter,
            int queueCapacity = int.MaxValue,
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
                        tOutput outItem;
                        try
                        {
                            (bool yieldOutput, outItem) = itemFilter(item, ct);
                            if (!yieldOutput) return;
                            try
                            {
                                await syncYield.WaitAsync(ct).DynamicContext();
                            }
                            catch (ObjectDisposedException)
                            {
                                return;
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            if (ex.CancellationToken != cancellationToken) throw;
                            return;
                        }
                        output = outItem;
                        syncOutput.Release();
                    }, queueCapacity, threads, cancellationToken).DynamicContext();
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
                    try
                    {
                        await syncOutput.WaitAsync(cancellationToken).DynamicContext();
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken != cancellationToken) throw;
                        break;
                    }
                    if (done) break;
                    yield return output;
                    syncYield.Release();
                }
            }
            finally
            {
                syncYield.Dispose();
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
        /// <param name="queueCapacity">Queue capacity</param>
        /// <param name="threads">Number of threads to use (<see langword="null"/> to use the number of available CPU cores)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Filtered items</returns>
        public static async IAsyncEnumerable<tOutput> FilterAsync<tInput, tOutput>(
            this IAsyncEnumerable<tInput> items,
            Func<tInput, CancellationToken, (bool Yield, tOutput Output)> itemFilter,
            int queueCapacity = int.MaxValue,
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
                        tOutput outItem;
                        try
                        {
                            (bool yieldOutput, outItem) = itemFilter(item, ct);
                            if (!yieldOutput) return;
                            try
                            {
                                await syncYield.WaitAsync(ct).DynamicContext();
                            }
                            catch (ObjectDisposedException)
                            {
                                return;
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            if (ex.CancellationToken != cancellationToken) throw;
                            return;
                        }
                        output = outItem;
                        syncOutput.Release();
                    }, queueCapacity, threads, cancellationToken).DynamicContext();
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
                    try
                    {
                        await syncOutput.WaitAsync(cancellationToken).DynamicContext();
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken != cancellationToken) throw;
                        break;
                    }
                    if (done) break;
                    yield return output;
                    syncYield.Release();
                }
            }
            finally
            {
                syncYield.Dispose();
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
        /// <param name="options">Parallel options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Filtered items</returns>
        public static IEnumerable<tOutput> Filter<tInput, tOutput>(
            this IEnumerable<tInput> items,
            Func<tInput, ParallelLoopState, CancellationToken, (bool Yield, tOutput Output)> itemFilter,
            ParallelOptions? options = null,
            CancellationToken cancellationToken = default
            )
        {
            using SemaphoreSlim syncYield = new(1, 1);
            using SemaphoreSlim syncOutput = new(1, 1);
            syncOutput.Wait(cancellationToken);
            tOutput output = default!;
            bool done = false;
            Task filter = Task.Factory.StartNew(async () =>
            {
                await Task.Yield();
                try
                {
                    Parallel.ForEach(items, options ?? new() { CancellationToken = cancellationToken }, (item, state) =>
                    {
                        if (state.ShouldExitCurrentIteration) return;
                        tOutput outItem;
                        try
                        {
                            (bool yieldOutput, outItem) = itemFilter(item, state, cancellationToken);
                            if (!yieldOutput) return;
                            try
                            {
                                syncYield.Wait(cancellationToken);
                            }
                            catch (ObjectDisposedException)
                            {
                                return;
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            if (ex.CancellationToken != cancellationToken) throw;
                            return;
                        }
                        output = outItem;
                        syncOutput.Release();
                    });
                }
                finally
                {
                    done = true;
                    syncOutput.Release();
                }
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .Unwrap();
            try
            {
                while (!done)
                {
                    try
                    {
                        syncOutput.Wait(cancellationToken);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken != cancellationToken) throw;
                        break;
                    }
                    if (done) break;
                    yield return output;
                    syncYield.Release();
                }
            }
            finally
            {
                syncYield.Dispose();
#pragma warning disable CA2016 // Forward cancellation token
                filter.Wait();
#pragma warning restore CA2016 // Forward cancellation token
            }
        }

        /// <summary>
        /// Asynchronous parallel item processor
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        internal sealed class ParallelAsyncProcessor<T> : ParallelItemQueueWorkerBase<T>
        {
            /// <summary>
            /// Item handler
            /// </summary>
            private readonly Func<T, CancellationToken, Task> ItemHandler;
            /// <summary>
            /// Number of processed items
            /// </summary>
            private volatile int _Processed = 0;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="itemHandler">Item handler</param>
            /// <param name="queueCapacity">Queue capacity</param>
            /// <param name="threads">Number of threads to use</param>
            internal ParallelAsyncProcessor(Func<T, CancellationToken, Task> itemHandler, int queueCapacity, int threads) : base(queueCapacity, threads) => ItemHandler = itemHandler;

            /// <summary>
            /// Number of processed items
            /// </summary>
            public int Processed => _Processed;

            /// <inheritdoc/>
            protected override async Task ProcessItem(T item, CancellationToken cancellationToken)
            {
                try
                {
                    await ItemHandler(item, cancellationToken).DynamicContext();
                }
                finally
                {
                    _Processed++;
                }
            }
        }
    }
}

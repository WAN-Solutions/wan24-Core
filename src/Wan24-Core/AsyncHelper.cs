using System.Diagnostics.Contracts;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Asynchronous API helper
    /// </summary>
    public static class AsyncHelper
    {
        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<T> FluentAsync<T>(Task<T> task, Func<T, Task<T>> action) => await action(await task.DynamicContext()).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async ValueTask<T> FluentAsync<T>(ValueTask<T> task, Func<T, ValueTask<T>> action) => await action(await task.DynamicContext()).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<T> FluentAsync<T, T1>(Task<T> task, T1 p1, Func<T, T1, Task<T>> action)
            => await action(await task.DynamicContext(), p1).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async ValueTask<T> FluentAsync<T, T1>(ValueTask<T> task, T1 p1, Func<T, T1, ValueTask<T>> action)
            => await action(await task.DynamicContext(), p1).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <typeparam name="T2">Parameter 2 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="p2">Parameter 2</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<T> FluentAsync<T, T1, T2>(Task<T> task, T1 p1, T2 p2, Func<T, T1, T2, Task<T>> action)
            => await action(await task.DynamicContext(), p1, p2).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <typeparam name="T2">Parameter 2 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="p2">Parameter 2</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async ValueTask<T> FluentAsync<T, T1, T2>(ValueTask<T> task, T1 p1, T2 p2, Func<T, T1, T2, ValueTask<T>> action)
            => await action(await task.DynamicContext(), p1, p2).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <typeparam name="T2">Parameter 2 type</typeparam>
        /// <typeparam name="T3">Parameter 3 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="p2">Parameter 2</param>
        /// <param name="p3">Parameter 3</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<T> FluentAsync<T, T1, T2, T3>(Task<T> task, T1 p1, T2 p2, T3 p3, Func<T, T1, T2, T3, Task<T>> action)
            => await action(await task.DynamicContext(), p1, p2, p3).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <typeparam name="T2">Parameter 2 type</typeparam>
        /// <typeparam name="T3">Parameter 3 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="p2">Parameter 2</param>
        /// <param name="p3">Parameter 3</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async ValueTask<T> FluentAsync<T, T1, T2, T3>(ValueTask<T> task, T1 p1, T2 p2, T3 p3, Func<T, T1, T2, T3, ValueTask<T>> action)
            => await action(await task.DynamicContext(), p1, p2, p3).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <typeparam name="T2">Parameter 2 type</typeparam>
        /// <typeparam name="T3">Parameter 3 type</typeparam>
        /// <typeparam name="T4">Parameter 4 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="p2">Parameter 2</param>
        /// <param name="p3">Parameter 3</param>
        /// <param name="p4">Parameter 4</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<T> FluentAsync<T, T1, T2, T3, T4>(Task<T> task, T1 p1, T2 p2, T3 p3, T4 p4, Func<T, T1, T2, T3, T4, Task<T>> action)
            => await action(await task.DynamicContext(), p1, p2, p3, p4).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <typeparam name="T2">Parameter 2 type</typeparam>
        /// <typeparam name="T3">Parameter 3 type</typeparam>
        /// <typeparam name="T4">Parameter 4 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="p2">Parameter 2</param>
        /// <param name="p3">Parameter 3</param>
        /// <param name="p4">Parameter 4</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async ValueTask<T> FluentAsync<T, T1, T2, T3, T4>(ValueTask<T> task, T1 p1, T2 p2, T3 p3, T4 p4, Func<T, T1, T2, T3, T4, ValueTask<T>> action)
            => await action(await task.DynamicContext(), p1, p2, p3, p4).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <typeparam name="T2">Parameter 2 type</typeparam>
        /// <typeparam name="T3">Parameter 3 type</typeparam>
        /// <typeparam name="T4">Parameter 4 type</typeparam>
        /// <typeparam name="T5">Parameter 5 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="p2">Parameter 2</param>
        /// <param name="p3">Parameter 3</param>
        /// <param name="p4">Parameter 4</param>
        /// <param name="p5">Parameter 5</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<T> FluentAsync<T, T1, T2, T3, T4, T5>(Task<T> task, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, Func<T, T1, T2, T3, T4, T5, Task<T>> action)
            => await action(await task.DynamicContext(), p1, p2, p3, p4, p5).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <typeparam name="T2">Parameter 2 type</typeparam>
        /// <typeparam name="T3">Parameter 3 type</typeparam>
        /// <typeparam name="T4">Parameter 4 type</typeparam>
        /// <typeparam name="T5">Parameter 5 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="p2">Parameter 2</param>
        /// <param name="p3">Parameter 3</param>
        /// <param name="p4">Parameter 4</param>
        /// <param name="p5">Parameter 5</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async ValueTask<T> FluentAsync<T, T1, T2, T3, T4, T5>(ValueTask<T> task, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, Func<T, T1, T2, T3, T4, T5, ValueTask<T>> action)
            => await action(await task.DynamicContext(), p1, p2, p3, p4, p5).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <typeparam name="T2">Parameter 2 type</typeparam>
        /// <typeparam name="T3">Parameter 3 type</typeparam>
        /// <typeparam name="T4">Parameter 4 type</typeparam>
        /// <typeparam name="T5">Parameter 5 type</typeparam>
        /// <typeparam name="T6">Parameter 6 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="p2">Parameter 2</param>
        /// <param name="p3">Parameter 3</param>
        /// <param name="p4">Parameter 4</param>
        /// <param name="p5">Parameter 5</param>
        /// <param name="p6">Parameter 6</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<T> FluentAsync<T, T1, T2, T3, T4, T5, T6>(Task<T> task, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, Func<T, T1, T2, T3, T4, T5, T6, Task<T>> action)
            => await action(await task.DynamicContext(), p1, p2, p3, p4, p5, p6).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <typeparam name="T2">Parameter 2 type</typeparam>
        /// <typeparam name="T3">Parameter 3 type</typeparam>
        /// <typeparam name="T4">Parameter 4 type</typeparam>
        /// <typeparam name="T5">Parameter 5 type</typeparam>
        /// <typeparam name="T6">Parameter 6 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="p2">Parameter 2</param>
        /// <param name="p3">Parameter 3</param>
        /// <param name="p4">Parameter 4</param>
        /// <param name="p5">Parameter 5</param>
        /// <param name="p6">Parameter 6</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async ValueTask<T> FluentAsync<T, T1, T2, T3, T4, T5, T6>(ValueTask<T> task, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, Func<T, T1, T2, T3, T4, T5, T6, ValueTask<T>> action)
            => await action(await task.DynamicContext(), p1, p2, p3, p4, p5, p6).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <typeparam name="T2">Parameter 2 type</typeparam>
        /// <typeparam name="T3">Parameter 3 type</typeparam>
        /// <typeparam name="T4">Parameter 4 type</typeparam>
        /// <typeparam name="T5">Parameter 5 type</typeparam>
        /// <typeparam name="T6">Parameter 6 type</typeparam>
        /// <typeparam name="T7">Parameter 7 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="p2">Parameter 2</param>
        /// <param name="p3">Parameter 3</param>
        /// <param name="p4">Parameter 4</param>
        /// <param name="p5">Parameter 5</param>
        /// <param name="p6">Parameter 6</param>
        /// <param name="p7">Parameter 7</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<T> FluentAsync<T, T1, T2, T3, T4, T5, T6, T7>(Task<T> task, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, Func<T, T1, T2, T3, T4, T5, T6, T7, Task<T>> action)
            => await action(await task.DynamicContext(), p1, p2, p3, p4, p5, p6, p7).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <typeparam name="T2">Parameter 2 type</typeparam>
        /// <typeparam name="T3">Parameter 3 type</typeparam>
        /// <typeparam name="T4">Parameter 4 type</typeparam>
        /// <typeparam name="T5">Parameter 5 type</typeparam>
        /// <typeparam name="T6">Parameter 6 type</typeparam>
        /// <typeparam name="T7">Parameter 7 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="p2">Parameter 2</param>
        /// <param name="p3">Parameter 3</param>
        /// <param name="p4">Parameter 4</param>
        /// <param name="p5">Parameter 5</param>
        /// <param name="p6">Parameter 6</param>
        /// <param name="p7">Parameter 7</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async ValueTask<T> FluentAsync<T, T1, T2, T3, T4, T5, T6, T7>(ValueTask<T> task, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, Func<T, T1, T2, T3, T4, T5, T6, T7, ValueTask<T>> action)
            => await action(await task.DynamicContext(), p1, p2, p3, p4, p5, p6, p7).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <typeparam name="T2">Parameter 2 type</typeparam>
        /// <typeparam name="T3">Parameter 3 type</typeparam>
        /// <typeparam name="T4">Parameter 4 type</typeparam>
        /// <typeparam name="T5">Parameter 5 type</typeparam>
        /// <typeparam name="T6">Parameter 6 type</typeparam>
        /// <typeparam name="T7">Parameter 7 type</typeparam>
        /// <typeparam name="T8">Parameter 8 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="p2">Parameter 2</param>
        /// <param name="p3">Parameter 3</param>
        /// <param name="p4">Parameter 4</param>
        /// <param name="p5">Parameter 5</param>
        /// <param name="p6">Parameter 6</param>
        /// <param name="p7">Parameter 7</param>
        /// <param name="p8">Parameter 8</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<T> FluentAsync<T, T1, T2, T3, T4, T5, T6, T7, T8>(Task<T> task, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, Func<T, T1, T2, T3, T4, T5, T6, T7, T8, Task<T>> action)
            => await action(await task.DynamicContext(), p1, p2, p3, p4, p5, p6, p7, p8).DynamicContext();

        /// <summary>
        /// Fluent asynchronous API helper
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="T1">Parameter 1 type</typeparam>
        /// <typeparam name="T2">Parameter 2 type</typeparam>
        /// <typeparam name="T3">Parameter 3 type</typeparam>
        /// <typeparam name="T4">Parameter 4 type</typeparam>
        /// <typeparam name="T5">Parameter 5 type</typeparam>
        /// <typeparam name="T6">Parameter 6 type</typeparam>
        /// <typeparam name="T7">Parameter 7 type</typeparam>
        /// <typeparam name="T8">Parameter 8 type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="p1">Parameter 1</param>
        /// <param name="p2">Parameter 2</param>
        /// <param name="p3">Parameter 3</param>
        /// <param name="p4">Parameter 4</param>
        /// <param name="p5">Parameter 5</param>
        /// <param name="p6">Parameter 6</param>
        /// <param name="p7">Parameter 7</param>
        /// <param name="p8">Parameter 8</param>
        /// <param name="action">Action</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async ValueTask<T> FluentAsync<T, T1, T2, T3, T4, T5, T6, T7, T8>(ValueTask<T> task, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, Func<T, T1, T2, T3, T4, T5, T6, T7, T8, ValueTask<T>> action)
            => await action(await task.DynamicContext(), p1, p2, p3, p4, p5, p6, p7, p8).DynamicContext();

        /// <summary>
        /// Final fluent asynchronous API result action
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="action">Action</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task FinallyAsync<T>(this Task<T> task, Action<T> action) => action(await task.DynamicContext());

        /// <summary>
        /// Final fluent asynchronous API result action
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="action">Action</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async ValueTask FinallyAsync<T>(this ValueTask<T> task, Action<T> action) => action(await task.DynamicContext());

        /// <summary>
        /// Final fluent asynchronous API result action
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="action">Action</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task FinallyAsync<T>(this Task<T> task, Func<T, Task> action) => await action(await task.DynamicContext()).DynamicContext();

        /// <summary>
        /// Final fluent asynchronous API result action
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="action">Action</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async ValueTask FinallyAsync<T>(this ValueTask<T> task, Func<T, ValueTask> action) => await action(await task.DynamicContext()).DynamicContext();

        /// <summary>
        /// Final fluent asynchronous API result action
        /// </summary>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <typeparam name="tReturn">Return type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="action">Action</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<tReturn> FinallyAsync<tResult, tReturn>(this Task<tResult> task, Func<tResult, tReturn> action) => action(await task.DynamicContext());

        /// <summary>
        /// Final fluent asynchronous API result action
        /// </summary>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <typeparam name="tReturn">Return type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="action">Action</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async ValueTask<tReturn> FinallyAsync<tResult, tReturn>(this ValueTask<tResult> task, Func<tResult, tReturn> action) => action(await task.DynamicContext());

        /// <summary>
        /// Final fluent asynchronous API result action
        /// </summary>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <typeparam name="tReturn">Return type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="action">Action</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<tReturn> FinallyAsync<tResult, tReturn>(this Task<tResult> task, Func<tResult, Task<tReturn>> action)
            => await action(await task.DynamicContext()).DynamicContext();

        /// <summary>
        /// Final fluent asynchronous API result action
        /// </summary>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <typeparam name="tReturn">Return type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="action">Action</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async ValueTask<tReturn> FinallyAsync<tResult, tReturn>(this ValueTask<tResult> task, Func<tResult, ValueTask<tReturn>> action)
            => await action(await task.DynamicContext()).DynamicContext();

        /// <summary>
        /// Wait one
        /// </summary>
        /// <param name="waitHandle">Wait handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WaitAsync(this WaitHandle waitHandle, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            using CancellationTokenRegistration ctr = cancellationToken.Register(() => tcs.TrySetCanceled());
            RegisteredWaitHandle registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                waitHandle, 
                (state, timedOut) => tcs.TrySetResult(), 
                tcs, 
                millisecondsTimeOutInterval: -1, 
                executeOnlyOnce: true
                );
            try
            {
                await tcs.Task.WaitAsync(cancellationToken).DynamicContext();
            }
            finally
            {
                registeredWaitHandle.Unregister(waitHandle);
            }
        }

        /// <summary>
        /// Wait one
        /// </summary>
        /// <param name="waitHandle">Wait handle</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WaitAsync(this WaitHandle waitHandle, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            using CancellationTokenRegistration ctr = cancellationToken.Register(() => tcs.TrySetCanceled());
            RegisteredWaitHandle registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(waitHandle, (state, timedOut) =>
            {
                if (timedOut) tcs.TrySetException(new TimeoutException());
                else tcs.TrySetResult();
            }, tcs, timeout, executeOnlyOnce: true);
            try
            {
                await tcs.Task.DynamicContext();
}
            finally
            {
                registeredWaitHandle.Unregister(waitHandle);
            }
        }

        /// <summary>
        /// Try dispose, if disposable
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static T TryDispose<T>(this T obj)
        {
            Contract.Assert(obj is not null);
            if(obj is IDisposableObject dObj)
            {
                if (!dObj.IsDisposing) dObj.Dispose();
            }
            else if (obj is IDisposable disposable) disposable?.Dispose();
            else if (obj is IAsyncDisposable asyncDisposable) asyncDisposable.DisposeAsync().AsTask().Wait();
            return obj;
        }

        /// <summary>
        /// Try dispose, if disposable
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<T> TryDisposeAsync<T>(this T obj)
        {
            await Task.Yield();
            Contract.Assert(obj is not null);
            if (obj is IDisposableObject dObj)
            {
                if (!dObj.IsDisposing) await dObj.DisposeAsync().DynamicContext();
            }
            else if (obj is IAsyncDisposable asyncDisposable) await asyncDisposable.DisposeAsync().DynamicContext();
            else if (obj is IDisposable disposable) disposable?.Dispose();
            return obj;
        }

        /// <summary>
        /// Try to dispose disposable objects
        /// </summary>
        /// <param name="objs">Objects</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static void TryDisposeAll(this IEnumerable<object> objs)
        {
            foreach (object obj in objs) obj.TryDispose();
        }

        /// <summary>
        /// Try to dispose disposable objects
        /// </summary>
        /// <param name="objs">Objects</param>
        /// <param name="parallel">Dispose parallel?</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task TryDisposeAllAsync(this IEnumerable<object> objs, bool parallel = true)
        {
            if (parallel)
            {
                await (from obj in objs select obj.TryDisposeAsync()).WaitAll().DynamicContext();
            }
            else
            {
                foreach (object obj in objs) await obj.TryDisposeAsync().DynamicContext();
            }
        }
    }
}

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
        public static async Task FinallyAsync<T>(this Task<T> task, Func<T, Task> action) => await action(await task.DynamicContext()).DynamicContext();

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
        public static async Task<tReturn> FinallyAsync<tResult, tReturn>(this Task<tResult> task, Func<tResult, Task<tReturn>> action)
            => await action(await task.DynamicContext()).DynamicContext();
    }
}

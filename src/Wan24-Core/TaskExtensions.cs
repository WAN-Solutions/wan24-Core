using System.Reflection;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Task extensions
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Task result property name
        /// </summary>
        private const string RESULT_PROPERTY_NAME = "Result";

        /// <summary>
        /// Get the result from a task
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        public static T GetResult<T>(this Task task) => (T)GetTaskResult(task, typeof(T))!;

        /// <summary>
        /// Get the result from a task
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        public static T? GetResultNullable<T>(this Task task) => (T?)GetTaskResult(task, typeof(T?));

        /// <summary>
        /// Get the result from a task
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        public static object GetResult(this Task task, Type type) => GetTaskResult(task, type)!;

        /// <summary>
        /// Get the result from a task
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        public static object? GetResultNullable(this Task task, Type type) => GetTaskResult(task, type);

        /// <summary>
        /// Get the result from a task
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        public static T GetResult<T>(this ValueTask task) => (T)GetTaskResult(task, typeof(T))!;

        /// <summary>
        /// Get the result from a task
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        public static T? GetResultNullable<T>(this ValueTask task) => (T?)GetTaskResult(task, typeof(T?));

        /// <summary>
        /// Get the result from a task
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        public static object GetResult(this ValueTask task, Type type) => GetTaskResult(task, type)!;

        /// <summary>
        /// Get the result from a task
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        public static object? GetResultNullable(this ValueTask task, Type type) => GetTaskResult(task, type);

        /// <summary>
        /// Wait for all tasks
        /// </summary>
        /// <param name="tasks">Tasks</param>
        public static async Task WaitAll(this IEnumerable<Task> tasks)
        {
            foreach (Task task in tasks) await task.ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Wait for all task results
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="tasks">Tasks</param>
        /// <returns>Results</returns>
        public static async Task<T[]> WaitAll<T>(this IEnumerable<Task<T>> tasks)
        {
            List<T> res = new();
            foreach (Task<T> task in tasks) res.Add(await task.ConfigureAwait(continueOnCapturedContext: false));
            return res.ToArray();
        }

        /// <summary>
        /// Wait for all tasks
        /// </summary>
        /// <param name="tasks">Tasks</param>
        public static async Task WaitAll(this IEnumerable<ValueTask> tasks)
        {
            foreach (ValueTask task in tasks) await task.ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Wait for all task results
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="tasks">Tasks</param>
        /// <returns>Results</returns>
        public static async Task<T[]> WaitAll<T>(this IEnumerable<ValueTask<T>> tasks)
        {
            List<T> res = new();
            foreach (ValueTask<T> task in tasks) res.Add(await task.ConfigureAwait(continueOnCapturedContext: false));
            return res.ToArray();
        }

        /// <summary>
        /// Return to the awaiting context
        /// </summary>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        public static ConfiguredTaskAwaitable FixedContext(this Task task) => task.ConfigureAwait(continueOnCapturedContext: true);

        /// <summary>
        /// Return to the awaiting context
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        public static ConfiguredTaskAwaitable<T> FixedContext<T>(this Task<T> task) => task.ConfigureAwait(continueOnCapturedContext: true);

        /// <summary>
        /// Return to the awaiting context
        /// </summary>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        public static ConfiguredValueTaskAwaitable FixedContext(this ValueTask task) => task.ConfigureAwait(continueOnCapturedContext: true);

        /// <summary>
        /// Return to the awaiting context
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        public static ConfiguredValueTaskAwaitable<T> FixedContext<T>(this ValueTask<T> task) => task.ConfigureAwait(continueOnCapturedContext: true);

        /// <summary>
        /// Return in any thread context
        /// </summary>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        public static ConfiguredTaskAwaitable DynamicContext(this Task task) => task.ConfigureAwait(continueOnCapturedContext: false);

        /// <summary>
        /// Return in any thread context
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        public static ConfiguredTaskAwaitable<T> DynamicContext<T>(this Task<T> task) => task.ConfigureAwait(continueOnCapturedContext: false);

        /// <summary>
        /// Return in any thread context
        /// </summary>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        public static ConfiguredValueTaskAwaitable DynamicContext(this ValueTask task) => task.ConfigureAwait(continueOnCapturedContext: false);

        /// <summary>
        /// Return in any thread context
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        public static ConfiguredValueTaskAwaitable<T> DynamicContext<T>(this ValueTask<T> task) => task.ConfigureAwait(continueOnCapturedContext: false);

        /// <summary>
        /// Get the task result
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        private static object? GetTaskResult(Task task, Type type)
                => typeof(Task<>).MakeGenericType(type).GetProperty(RESULT_PROPERTY_NAME, BindingFlags.Instance | BindingFlags.Public)!.GetValue(task);

        /// <summary>
        /// Get the task result
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        private static object? GetTaskResult(ValueTask task, Type type)
                => typeof(ValueTask<>).MakeGenericType(type).GetProperty(RESULT_PROPERTY_NAME, BindingFlags.Instance | BindingFlags.Public)!.GetValue(task);
    }
}

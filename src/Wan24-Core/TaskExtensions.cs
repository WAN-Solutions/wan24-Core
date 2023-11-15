using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Task extensions
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static T GetResult<T>(this Task task)
        {
            PropertyInfoExt pi = task.GetType().GetPropertyCached("Result")
                ?? throw new ArgumentException("Not a result task", nameof(task));
            if (pi.Getter is null) throw new ArgumentException("Result property has no getter", nameof(task));
            if (!typeof(T).IsAssignableFrom(pi.PropertyType)) throw new ArgumentException($"Result type {pi.PropertyType} mismatch", nameof(task));
            return (T)(pi.Getter(task) ?? throw new InvalidDataException("Task result is NULL"));
        }

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static T? GetResultNullable<T>(this Task task)
        {
            PropertyInfoExt pi = task.GetType().GetPropertyCached("Result")
                ?? throw new ArgumentException("Not a result task", nameof(task));
            if (pi.Getter is null) throw new ArgumentException("Result property has no getter", nameof(task));
            if (!typeof(T).IsAssignableFrom(pi.PropertyType)) throw new ArgumentException($"Result type {pi.PropertyType} mismatch", nameof(task));
            return (T?)pi.Getter(task);
        }

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object GetResult(this Task task, in Type type)
        {
            PropertyInfoExt pi = task.GetType().GetPropertyCached("Result")
                ?? throw new ArgumentException("Not a result task", nameof(task));
            if (pi.Getter is null) throw new ArgumentException("Result property has no getter", nameof(task));
            if (!type.IsAssignableFrom(pi.PropertyType)) throw new ArgumentException("Result type mismatch", nameof(task));
            return pi.Getter(task) ?? throw new InvalidDataException("Task result is NULL");
        }

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object? GetResultNullable(this Task task, in Type type)
        {
            PropertyInfoExt pi = task.GetType().GetPropertyCached("Result")
                ?? throw new ArgumentException("Not a result task", nameof(task));
            if (pi.Getter is null) throw new ArgumentException("Result property has no getter", nameof(task));
            if (!type.IsAssignableFrom(pi.PropertyType)) throw new ArgumentException("Result type mismatch", nameof(task));
            return pi.Getter(task);
        }

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static T GetResult<T>(this ValueTask task)
        {
            PropertyInfoExt pi = task.GetType().GetPropertyCached("Result")
                ?? throw new ArgumentException("Not a result task", nameof(task));
            if (pi.Getter is null) throw new ArgumentException("Result property has no getter", nameof(task));
            if (!typeof(T).IsAssignableFrom(pi.PropertyType)) throw new ArgumentException($"Result type {pi.PropertyType} mismatch", nameof(task));
            return (T)(pi.Getter(task) ?? throw new InvalidDataException("Task result is NULL"));
        }

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static T? GetResultNullable<T>(this ValueTask task)
        {
            PropertyInfoExt pi = task.GetType().GetPropertyCached("Result")
                ?? throw new ArgumentException("Not a result task", nameof(task));
            if (pi.Getter is null) throw new ArgumentException("Result property has no getter", nameof(task));
            if (!typeof(T).IsAssignableFrom(pi.PropertyType)) throw new ArgumentException($"Result type {pi.PropertyType} mismatch", nameof(task));
            return (T?)pi.Getter(task);
        }

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object GetResult(this ValueTask task, in Type type)
        {
            PropertyInfoExt pi = task.GetType().GetPropertyCached("Result")
                ?? throw new ArgumentException("Not a result task", nameof(task));
            if (pi.Getter is null) throw new ArgumentException("Result property has no getter", nameof(task));
            if (!type.IsAssignableFrom(pi.PropertyType)) throw new ArgumentException("Result type mismatch", nameof(task));
            return pi.Getter(task) ?? throw new InvalidDataException("Task result is NULL");
        }

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object? GetResultNullable(this ValueTask task, in Type type)
        {
            PropertyInfoExt pi = task.GetType().GetPropertyCached("Result")
                ?? throw new ArgumentException("Not a result task", nameof(task));
            if (pi.Getter is null) throw new ArgumentException("Result property has no getter", nameof(task));
            if (!type.IsAssignableFrom(pi.PropertyType)) throw new ArgumentException("Result type mismatch", nameof(task));
            return pi.Getter(task);
        }

        /// <summary>
        /// Wait for all tasks
        /// </summary>
        /// <param name="tasks">Tasks</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task WaitAll(this IEnumerable<Task> tasks)
        {
            List<Exception> exceptions = new();
            foreach (Task task in tasks)
                try
                {
                    await task.DynamicContext();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            if (exceptions.Count != 0) throw new AggregateException(exceptions);
        }

        /// <summary>
        /// Wait for all task results
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="tasks">Tasks</param>
        /// <returns>Results</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<T[]> WaitAll<T>(this IEnumerable<Task<T>> tasks)
        {
            List<Exception> exceptions = new();
            List<T> res = new();
            foreach (Task<T> task in tasks)
                try
                {
                    res.Add(await task.DynamicContext());
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            if (exceptions.Count != 0) throw new AggregateException(exceptions);
            return res.ToArray();
        }

        /// <summary>
        /// Wait for all tasks
        /// </summary>
        /// <param name="tasks">Tasks</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task WaitAll(this IEnumerable<ValueTask> tasks)
        {
            List<Exception> exceptions = new();
            foreach (ValueTask task in tasks)
                try
                {
                    await task.DynamicContext();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            if (exceptions.Count != 0) throw new AggregateException(exceptions);
        }

        /// <summary>
        /// Wait for all task results
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="tasks">Tasks</param>
        /// <returns>Results</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<T[]> WaitAll<T>(this IEnumerable<ValueTask<T>> tasks)
        {
            List<Exception> exceptions = new();
            List<T> res = new();
            foreach (ValueTask<T> task in tasks)
                try
                {
                    res.Add(await task.DynamicContext());
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            if (exceptions.Count != 0) throw new AggregateException(exceptions);
            return res.ToArray();
        }

        /// <summary>
        /// Return to the awaiting context
        /// </summary>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static ConfiguredTaskAwaitable FixedContext(this Task task) => task.ConfigureAwait(continueOnCapturedContext: true);

        /// <summary>
        /// Return to the awaiting context
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static ConfiguredTaskAwaitable<T> FixedContext<T>(this Task<T> task) => task.ConfigureAwait(continueOnCapturedContext: true);

        /// <summary>
        /// Return to the awaiting context
        /// </summary>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static ConfiguredValueTaskAwaitable FixedContext(this ValueTask task) => task.ConfigureAwait(continueOnCapturedContext: true);

        /// <summary>
        /// Return to the awaiting context
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static ConfiguredValueTaskAwaitable<T> FixedContext<T>(this ValueTask<T> task) => task.ConfigureAwait(continueOnCapturedContext: true);

        /// <summary>
        /// Return in any thread context
        /// </summary>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static ConfiguredTaskAwaitable DynamicContext(this Task task) => task.ConfigureAwait(continueOnCapturedContext: false);

        /// <summary>
        /// Return in any thread context
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static ConfiguredTaskAwaitable<T> DynamicContext<T>(this Task<T> task) => task.ConfigureAwait(continueOnCapturedContext: false);

        /// <summary>
        /// Return in any thread context
        /// </summary>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static ConfiguredValueTaskAwaitable DynamicContext(this ValueTask task) => task.ConfigureAwait(continueOnCapturedContext: false);

        /// <summary>
        /// Return in any thread context
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Task</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static ConfiguredValueTaskAwaitable<T> DynamicContext<T>(this ValueTask<T> task) => task.ConfigureAwait(continueOnCapturedContext: false);

        /// <summary>
        /// Start a long running task
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="scheduler">Scheduler</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Long running task</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static Task StartLongRunningTask(this Func<Task> action, in TaskScheduler? scheduler = null, in CancellationToken cancellationToken = default)
            => Task.Factory.StartNew(
                action,
                cancellationToken,
                TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously,
                scheduler ?? TaskScheduler.Current
                )
                .Unwrap();

        /// <summary>
        /// Start a long running task
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="scheduler">Scheduler</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Long running task</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static Task<T> StartLongRunningTask<T>(this Func<Task<T>> action, in TaskScheduler? scheduler = null, in CancellationToken cancellationToken = default)
            => Task.Factory.StartNew(
                action,
                cancellationToken,
                TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously,
                scheduler ?? TaskScheduler.Current
                )
                .Unwrap();

        /// <summary>
        /// Start a task which will use fair task scheduler selection
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="scheduler">Scheduler</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static Task StartFairTask(this Func<Task> action, in TaskScheduler? scheduler = null, in CancellationToken cancellationToken = default)
            => Task.Factory.StartNew(
                action,
                cancellationToken,
                TaskCreationOptions.PreferFairness | TaskCreationOptions.RunContinuationsAsynchronously,
                scheduler ?? TaskScheduler.Current
                )
                .Unwrap();

        /// <summary>
        /// Start a task which will use fair task scheduler selection
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="scheduler">Scheduler</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static Task<T> StartFairTask<T>(this Func<Task<T>> action, in TaskScheduler? scheduler = null, in CancellationToken cancellationToken = default)
            => Task.Factory.StartNew(
                action,
                cancellationToken,
                TaskCreationOptions.PreferFairness | TaskCreationOptions.RunContinuationsAsynchronously,
                scheduler ?? TaskScheduler.Current
                )
                .Unwrap();

        /// <summary>
        /// Unwrap the final result type of a task recursive
        /// </summary>
        /// <param name="task">Task</param>
        /// <returns>Final result type</returns>
        public static Type? UnwrapFinalResultType(this Task task)
        {
            Type type = task.GetType();
            Type? res = null;
            while (true)
            {
                if (!type.IsGenericType || !typeof(Task).IsAssignableFrom(type)) return res;
                res = type = type.GetGenericArguments()[0];
            }
        }
    }
}

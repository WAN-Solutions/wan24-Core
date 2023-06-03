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
        /// <see cref="GetResult{T}(Task)"/> method
        /// </summary>
        private static readonly MethodInfo GetResultMethod;
        /// <summary>
        /// <see cref="GetResultNullable{T}(Task)"/> method
        /// </summary>
        private static readonly MethodInfo GetResultNullableMethod;
        /// <summary>
        /// <see cref="GetResult{T}(ValueTask)"/> method
        /// </summary>
        private static readonly MethodInfo GetResultValueMethod;
        /// <summary>
        /// <see cref="GetResultNullable{T}(ValueTask)"/> method
        /// </summary>
        private static readonly MethodInfo GetResultNullableValueMethod;

        /// <summary>
        /// Constructor
        /// </summary>
        static TaskExtensions()
        {
            Type type = typeof(TaskExtensions);
            GetResultMethod = type.GetMethod(
                nameof(GetResult),
                BindingFlags.Public | BindingFlags.Static,
                filter: null,
                genericArgumentCount: 1,
                exactTypes: true,
                returnType: null,
                typeof(Task)
                )
                ?? throw new InvalidProgramException($"Failed to reflect the {nameof(GetResult)} method");
            GetResultNullableMethod = type.GetMethod(
                nameof(GetResultNullable),
                BindingFlags.Public | BindingFlags.Static,
                filter: null,
                genericArgumentCount: 1,
                exactTypes: true,
                returnType: null,
                typeof(Task)
                )
                ?? throw new InvalidProgramException($"Failed to reflect the {nameof(GetResultNullable)} method");
            GetResultValueMethod = type.GetMethod(
                nameof(GetResult),
                BindingFlags.Public | BindingFlags.Static,
                filter: null,
                genericArgumentCount: 1,
                exactTypes: true,
                returnType: null,
                typeof(ValueTask)
                )
                ?? throw new InvalidProgramException($"Failed to reflect the {nameof(GetResult)} method");
            GetResultNullableValueMethod = type.GetMethod(
                nameof(GetResultNullable),
                BindingFlags.Public | BindingFlags.Static,
                filter: null,
                genericArgumentCount: 1,
                exactTypes: true,
                returnType: null,
                typeof(ValueTask)
                )
                ?? throw new InvalidProgramException($"Failed to reflect the {nameof(GetResultNullable)} method");
        }

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        public static T GetResult<T>(this Task task) => ((Task<T>)task).Result;

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        public static T? GetResultNullable<T>(this Task task) => ((Task<T?>)task).Result;

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        public static object GetResult(this Task task, Type type)
            => GetResultMethod.MakeGenericMethod(type).Invoke(obj: null, new object?[] { task })
                ?? throw new ArgumentException("The task result is NULL", nameof(task));

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        public static object? GetResultNullable(this Task task, Type type) => GetResultNullableMethod.MakeGenericMethod(type).Invoke(obj: null, new object?[] { task });

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        public static T GetResult<T>(this ValueTask task)
            => task is ValueTask<T> valueTask ? valueTask.Result : throw new ArgumentException($"{nameof(ValueTask)} is not a {nameof(ValueTask)}<T>", nameof(task));

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        public static T? GetResultNullable<T>(this ValueTask task)
            => task is ValueTask<T> valueTask ? valueTask.Result : throw new ArgumentException($"{nameof(ValueTask)} is not a {nameof(ValueTask)}<T>", nameof(task));

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        public static object GetResult(this ValueTask task, Type type)
            => GetResultValueMethod.MakeGenericMethod(type).Invoke(obj: null, new object?[] { task })
                ?? throw new ArgumentException("The task result is NULL", nameof(task));

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        public static object? GetResultNullable(this ValueTask task, Type type) => GetResultNullableValueMethod.MakeGenericMethod(type).Invoke(obj: null, new object?[] { task });

        /// <summary>
        /// Wait for all tasks
        /// </summary>
        /// <param name="tasks">Tasks</param>
        public static async Task WaitAll(this IEnumerable<Task> tasks)
        {
            List<Exception> exceptions = new();
            foreach (Task task in tasks)
                try
                {
                    await task.DynamicContext();
                }
                catch(Exception ex)
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
        public static async Task<T[]> WaitAll<T>(this IEnumerable<Task<T>> tasks)
        {
            List<Exception> exceptions = new();
            List<T> res = new();
            foreach (Task<T> task in tasks)
                try
                {
                    res.Add(await task.DynamicContext());
                }
                catch(Exception ex)
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
        /// Start a long running task
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="scheduler">Scheduler</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Long running task</returns>
        public static Task StartLongRunningTask(this Func<Task> action, TaskScheduler? scheduler = null, CancellationToken cancellationToken = default)
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
        public static Task<T> StartLongRunningTask<T>(this Func<Task<T>> action, TaskScheduler? scheduler = null, CancellationToken cancellationToken = default)
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
        public static Task StartFairTask(this Func<Task> action, TaskScheduler? scheduler = null, CancellationToken cancellationToken = default)
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
        public static Task<T> StartFairTask<T>(this Func<Task<T>> action, TaskScheduler? scheduler = null, CancellationToken cancellationToken = default)
            => Task.Factory.StartNew(
                action,
                cancellationToken,
                TaskCreationOptions.PreferFairness | TaskCreationOptions.RunContinuationsAsynchronously,
                scheduler ?? TaskScheduler.Current
                )
                .Unwrap();

        /// <summary>
        /// Add a cancellation token to a task
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public static Task WithCancellation(this Task task, CancellationToken cancellationToken)
            => Task.Run(async () => await task.DynamicContext(), cancellationToken);

        /// <summary>
        /// Add a cancellation token to a task
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
            => Task.Run(async () => await task.DynamicContext(), cancellationToken);

        /// <summary>
        /// Add a timeout to a task
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Task</returns>
        public static async Task WithTimeout(this Task task, TimeSpan timeout)
        {
            using CancellationTokenSource cts = new();
            Task timeoutTask = Task.Delay(timeout, cts.Token);
            try
            {
                if (await Task.WhenAny(task, timeoutTask).DynamicContext() == timeoutTask)
                {
                    TimeoutException ex = new();
                    ex.Data[timeout] = true;
                    throw ex;
                }
            }
            finally
            {
                cts.Cancel();
            }
        }

        /// <summary>
        /// Add a timeout to a task
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Task</returns>
        public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
        {
            using CancellationTokenSource cts = new();
            Task timeoutTask = Task.Delay(timeout, cts.Token);
            try
            {
                if (await Task.WhenAny(task, timeoutTask).DynamicContext() == timeoutTask)
                {
                    TimeoutException ex = new();
                    ex.Data[timeout] = true;
                    throw ex;
                }
                return await task;
            }
            finally
            {
                cts.Cancel();
            }
        }

        /// <summary>
        /// Add a timeout to a task
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public static async Task WithTimeoutAndCancellation(this Task task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            using CancellationTokenSource cts = new();
            Task timeoutTask = Task.Delay(timeout, cts.Token);
            CancellationTokenRegistration registration = default;
            void Canceled()
            {
                registration.Dispose();
                cts.Cancel();
            }
            registration = cancellationToken.Register(Canceled);
            try
            {
                if (await Task.WhenAny(task.WithCancellation(cancellationToken), timeoutTask).DynamicContext() == timeoutTask)
                {
                    TimeoutException ex = new();
                    ex.Data[timeout] = true;
                    throw ex;
                }
            }
            finally
            {
                registration.Dispose();
                cts.Cancel();
            }
        }

        /// <summary>
        /// Add a timeout to a task
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public static async Task<T> WithTimeoutAndCancellation<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            using CancellationTokenSource cts = new();
            Task timeoutTask = Task.Delay(timeout, cts.Token);
            CancellationTokenRegistration registration = default;
            void Canceled()
            {
                registration.Dispose();
                cts.Cancel();
            }
            registration = cancellationToken.Register(Canceled);
            try
            {
                if (await Task.WhenAny(task.WithCancellation(cancellationToken), timeoutTask).DynamicContext() == timeoutTask)
                {
                    TimeoutException ex = new();
                    ex.Data[timeout] = true;
                    throw ex;
                }
                return await task;
            }
            finally
            {
                registration.Dispose();
                cts.Cancel();
            }
        }
    }
}

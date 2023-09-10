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
            return (T?)pi.Getter(task);
        }

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#pragma warning disable IDE0060 // Remove unused parameter
        public static object GetResult(this Task task, in Type type)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            PropertyInfoExt pi = task.GetType().GetPropertyCached("Result")
                ?? throw new ArgumentException("Not a result task", nameof(task));
            if (pi.Getter is null) throw new ArgumentException("Result property has no getter", nameof(task));
            return pi.Getter(task) ?? throw new InvalidDataException("Task result is NULL");
        }

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#pragma warning disable IDE0060 // Remove unused parameter
        public static object? GetResultNullable(this Task task, in Type type)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            PropertyInfoExt pi = task.GetType().GetPropertyCached("Result")
                ?? throw new ArgumentException("Not a result task", nameof(task));
            if (pi.Getter is null) throw new ArgumentException("Result property has no getter", nameof(task));
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
            return (T?)pi.Getter(task);
        }

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#pragma warning disable IDE0060 // Remove unused parameter
        public static object GetResult(this ValueTask task, in Type type)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            PropertyInfoExt pi = task.GetType().GetPropertyCached("Result")
                ?? throw new ArgumentException("Not a result task", nameof(task));
            if (pi.Getter is null) throw new ArgumentException("Result property has no getter", nameof(task));
            return pi.Getter(task) ?? throw new InvalidDataException("Task result is NULL");
        }

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#pragma warning disable IDE0060 // Remove unused parameter
        public static object? GetResultNullable(this ValueTask task, in Type type)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            PropertyInfoExt pi = task.GetType().GetPropertyCached("Result")
                ?? throw new ArgumentException("Not a result task", nameof(task));
            if (pi.Getter is null) throw new ArgumentException("Result property has no getter", nameof(task));
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
        /// Add a cancellation token to a task
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        /// <exception cref="TaskCanceledException">If canceled</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static Task WithCancellation(this Task task, in CancellationToken cancellationToken) => task.WaitAsync(cancellationToken);

        /// <summary>
        /// Add a cancellation token to a task
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        /// <exception cref="TaskCanceledException">If canceled</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static Task<T> WithCancellation<T>(this Task<T> task, in CancellationToken cancellationToken) => task.WaitAsync(cancellationToken);

        /// <summary>
        /// Add a timeout to a task
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Task</returns>
        /// <exception cref="TimeoutException">On timeout (<c>TimeoutException.Data[timeout]</c> is set)</exception>
        public static async Task WithTimeout(this Task task, TimeSpan timeout)
        {
            using CancellationTokenSource cts = new(timeout);
            Task timeoutTask = Task.Delay(timeout, cts.Token);
            try
            {
                if(await Task.WhenAny(task, timeoutTask).DynamicContext() == timeoutTask)
                {
                    TimeoutException ex = new();
                    ex.Data[timeout] = true;
                    throw ex;
                }
                await task.DynamicContext();
            }
            catch (TaskCanceledException ex)
            {
                if (ex.CancellationToken != cts.Token) throw;
                TimeoutException timeoutException = new(message: null, ex);
                timeoutException.Data[timeout] = true;
                throw timeoutException;
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken != cts.Token) throw;
                TimeoutException timeoutException = new(message: null, ex);
                timeoutException.Data[timeout] = true;
                throw timeoutException;
            }
        }

        /// <summary>
        /// Add a timeout to a task
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Task</returns>
        /// <exception cref="TimeoutException">On timeout (<c>TimeoutException.Data[timeout]</c> is set)</exception>
        public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
        {
            using CancellationTokenSource cts = new(timeout);
            Task timeoutTask = Task.Delay(timeout, cts.Token);
            try
            {
                if(await Task.WhenAny(task, timeoutTask).DynamicContext() == timeoutTask)
                {
                    TimeoutException ex = new();
                    ex.Data[timeout] = true;
                    throw ex;
                }
                return await task.DynamicContext();
            }
            catch (TaskCanceledException ex)
            {
                if (ex.CancellationToken != cts.Token) throw;
                TimeoutException timeoutException = new();
                timeoutException.Data[timeout] = true;
                throw timeoutException;
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken != cts.Token) throw;
                TimeoutException timeoutException = new();
                timeoutException.Data[timeout] = true;
                throw timeoutException;
            }
        }

        /// <summary>
        /// Add a timeout to a task
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        /// <exception cref="OperationCanceledException">If canceled</exception>
        /// <exception cref="TimeoutException">On timeout</exception>
        public static async Task WithTimeoutAndCancellation(this Task task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            using CancellationTokenSource cts = new(timeout);
            bool canceled = false;
            CancellationTokenRegistration registration = default!;
            void Canceled()
            {
                canceled = true;
                registration.Dispose();
                cts.Cancel();
            }
            registration = cancellationToken.Register(Canceled);
            try
            {
                await task.WithCancellation(cts.Token).DynamicContext();
            }
            catch (TaskCanceledException ex)
            {
                if (canceled || ex.CancellationToken != cts.Token) throw;
                throw new TimeoutException(message: null, ex);
            }
            finally
            {
                registration.Dispose();
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
        /// <exception cref="OperationCanceledException">If canceled</exception>
        /// <exception cref="TimeoutException">On timeout</exception>
        public static async Task<T> WithTimeoutAndCancellation<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            using CancellationTokenSource cts = new(timeout);
            bool canceled = false;
            CancellationTokenRegistration registration = default!;
            void Canceled()
            {
                canceled = true;
                registration.Dispose();
                cts.Cancel();
            }
            registration = cancellationToken.Register(Canceled);
            try
            {
                return await task.WithCancellation(cts.Token).DynamicContext();
            }
            catch (TaskCanceledException ex)
            {
                if (canceled || ex.CancellationToken != cts.Token) throw;
                throw new TimeoutException(message: null, ex);
            }
            finally
            {
                registration.Dispose();
            }
        }
    }
}

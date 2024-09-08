using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Task helper
    /// </summary>
    public static class TaskHelper
    {
        /// <summary>
        /// <see cref="AwaitTaskAsync{T}(Task{T})"/>
        /// </summary>
        public static readonly MethodInfoExt AwaitTaskMethod = typeof(TaskHelper).GetMethodCached(nameof(AwaitTaskAsync), BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidProgramException();
        /// <summary>
        /// <see cref="AwaitValueTaskAsync{T}(ValueTask{T})"/>
        /// </summary>
        public static readonly MethodInfoExt AwaitValueTaskMethod = typeof(TaskHelper).GetMethodCached(nameof(AwaitValueTaskAsync), BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidProgramException();

        /// <summary>
        /// Get the task result type, if any
        /// </summary>
        /// <param name="task">Possible task</param>
        /// <returns>Result type</returns>
        public static Type? GetTaskResultType(object task)
        {
            Type type = task.GetType();
            Type? res = type.IsTask() && type.IsGenericType ? type.GetGenericArgumentCached(index: 0) : null;
            return res is null || res == typeof(void) ? null : res;
        }

        /// <summary>
        /// Get the result of a task, if the given <c>task</c> is a <see cref="Task"/> or a <see cref="ValueTask"/> or a <see cref="ValueTask{TResult}"/>
        /// </summary>
        /// <param name="task">Possible task (will be awaited)</param>
        /// <returns>(Task) result (is the given <c>task</c>, if it was not a task)</returns>
        public static object? GetAnyTaskResult(in object task)
        {
            if (!task.IsTask()) return task;
            Type type = task.GetType();
            if (GetTaskResultType(task) is not Type resultType)
            {
                if (task is Task realTask) realTask.GetAwaiter().GetResult();
                else if (task is ValueTask valueTask) valueTask.GetAwaiter().GetResult();
                return null;
            }
            return typeof(Task).IsAssignableFrom(type)
                ? GetResult(task, resultType)
                : GetValueTaskResult(task, resultType);
        }

        /// <summary>
        /// Get the result of a task, if the given <c>task</c> is a <see cref="Task"/> or a <see cref="ValueTask"/> or a <see cref="ValueTask{TResult}"/>
        /// </summary>
        /// <param name="task">Possible task (will be awaited)</param>
        /// <returns>(Task) result (is the given <c>task</c>, if it was not a task)</returns>
        public static async Task<object?> GetAnyTaskResultAsync(object task)
        {
            if (!task.IsTask()) return task;
            Type type = task.GetType();
            if (GetTaskResultType(task) is not Type resultType)
            {
                if (task is Task realTask) await realTask.DynamicContext();
                else if (task is ValueTask valueTask) await valueTask.DynamicContext();
                return null;
            }
            if (typeof(Task).IsAssignableFrom(type))
            {
                await ((Task)AwaitTaskMethod.MakeGenericMethod(resultType).Invoker!(null, [task])!).DynamicContext();
                return GetResult(task, resultType);
            }
            await ((ValueTask)AwaitValueTaskMethod.MakeGenericMethod(resultType).Invoker!(null, [task])!).DynamicContext();
            return GetValueTaskResult(task, resultType);
        }

        /// <summary>
        /// Get the final task result of a possible nested task
        /// </summary>
        /// <param name="task">Possible task (will be awaited)</param>
        /// <returns>(Task) result (is the given <c>task</c>, if it was not a task)</returns>
        public static object? GetAnyFinalTaskResult(in object task)
        {
            object? res = GetAnyTaskResult(task);
            while (res is not null && res.IsTask()) res = GetAnyTaskResult(res);
            return res;
        }

        /// <summary>
        /// Get the final task result of a possible nested task
        /// </summary>
        /// <param name="task">Possible task (will be awaited)</param>
        /// <returns>(Task) result (is the given <c>task</c>, if it was not a task)</returns>
        public static async Task<object?> GetAnyFinalTaskResultAsync(object task)
        {
            object? res = await GetAnyTaskResultAsync(task).DynamicContext();
            while (res is not null && res.IsTask()) res = await GetAnyTaskResultAsync(res).DynamicContext();
            return res;
        }

        /// <summary>
        /// Get the result from a task
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static T GetResult<T>(in object task) => (T)GetResult(task, typeof(T));

        /// <summary>
        /// Get the result from a task
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static T? GetResultNullable<T>(in object task) => (T?)GetResultNullable(task, typeof(T));

        /// <summary>
        /// Get the result from a task
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static object GetResult(in object task, Type? type = null)
        {
            if (!task.IsTask()) throw new ArgumentException("Not a task", nameof(task));
            Type taskType = task.GetType();
            if (!taskType.IsGenericType) throw new ArgumentException("Not a result task", nameof(task));
            Type resultType = taskType.GetGenericArgumentCached(index: 0);
            if (type is not null && !type.IsAssignableFrom(resultType)) throw new ArgumentException($"Task returns {resultType}", nameof(type));
            type ??= resultType;
            Type awaiterType = typeof(TaskAwaiter<>).MakeGenericType(resultType);
            MethodInfoExt getAwaiter = typeof(Task<>).MakeGenericType(resultType).GetMethodsCached()
                    .FirstOrDefault(m => m.Name == nameof(Task<bool>.GetAwaiter) && m.ReturnType == awaiterType)
                    ?? throw new InvalidProgramException("Task.GetAwaiter method not found"),
                getResult = awaiterType.GetMethodsCached().FirstOrDefault(m => m.Name == nameof(TaskAwaiter<bool>.GetResult) && m.ReturnType == resultType)
                    ?? throw new InvalidProgramException("TaskAwaiter.GetResult method not found");
            if (getAwaiter.Invoker is null) throw new InvalidProgramException("Task.GetAwaiter can't be invoked");
            if (getResult.Invoker is null) throw new InvalidProgramException("TaskAwaiter.GetResult can't be invoked");
            if (getAwaiter.Invoker(task, []) is not object awaiter) throw new InvalidProgramException("Task.GetAwaiter didn't return an awaiter");
            return getResult.Invoker(awaiter, []) ?? throw new InvalidDataException("Task result is NULL");
        }

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static object? GetResultNullable(in object task, Type? type = null)
        {
            if (!task.IsTask()) throw new ArgumentException("Not a task", nameof(task));
            Type taskType = task.GetType();
            if (!taskType.IsGenericType) throw new ArgumentException("Not a result task", nameof(task));
            Type resultType = taskType.GetGenericArgumentCached(index: 0);
            if (type is not null && !type.IsAssignableFrom(resultType)) throw new ArgumentException($"Task returns {resultType}", nameof(type));
            type ??= resultType;
            Type awaiterType = typeof(TaskAwaiter<>).MakeGenericType(resultType);
            MethodInfoExt getAwaiter = typeof(Task<>).MakeGenericType(resultType).GetMethodsCached()
                    .FirstOrDefault(m => m.Name == nameof(Task<bool>.GetAwaiter) && m.ReturnType == awaiterType)
                    ?? throw new InvalidProgramException("Task.GetAwaiter method not found"),
                getResult = awaiterType.GetMethodsCached().FirstOrDefault(m => m.Name == nameof(TaskAwaiter<bool>.GetResult) && m.ReturnType == resultType)
                    ?? throw new InvalidProgramException("TaskAwaiter.GetResult method not found");
            if (getAwaiter.Invoker is null) throw new InvalidProgramException("Task.GetAwaiter can't be invoked");
            if (getResult.Invoker is null) throw new InvalidProgramException("TaskAwaiter.GetResult can't be invoked");
            if (getAwaiter.Invoker(task, []) is not object awaiter) throw new InvalidProgramException("Task.GetAwaiter didn't return an awaiter");
            return getResult.Invoker(awaiter, []);
        }

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static T GetValueTaskResult<T>(in object task) => (T)GetValueTaskResult(task, typeof(T));

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static T? GetValueTaskResultNullable<T>(in object task) => (T?)GetValueTaskResultNullable(task, typeof(T));

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static object GetValueTaskResult(in object task, Type? type = null)
        {
            Type taskType = task.GetType();
            if (!taskType.IsGenericType) throw new ArgumentException("Not a result task", nameof(task));
            Type resultType = taskType.GetGenericArgumentCached(index: 0);
            if (type is not null && !type.IsAssignableFrom(resultType)) throw new ArgumentException($"Task returns {resultType}", nameof(type));
            type ??= resultType;
            if (taskType.GetGenericTypeDefinition() != typeof(ValueTask<>)) throw new ArgumentException("Not a ValueTask<>", nameof(task));
            Type awaiterType = typeof(ValueTaskAwaiter<>).MakeGenericType(resultType);
            MethodInfoExt getAwaiter = typeof(ValueTask<>).MakeGenericType(resultType).GetMethodsCached()
                    .FirstOrDefault(m => m.Name == nameof(ValueTask<bool>.GetAwaiter) && m.ReturnType == awaiterType)
                    ?? throw new InvalidProgramException("ValueTask.GetAwaiter method not found"),
                getResult = awaiterType.GetMethodsCached().FirstOrDefault(m => m.Name == nameof(ValueTaskAwaiter<bool>.GetResult) && m.ReturnType == resultType)
                    ?? throw new InvalidProgramException("ValueTaskAwaiter.GetResult method not found");
            if (getAwaiter.Invoker is null) throw new InvalidProgramException("ValueTask.GetAwaiter can't be invoked");
            if (getResult.Invoker is null) throw new InvalidProgramException("ValueTaskAwaiter.GetResult can't be invoked");
            if (getAwaiter.Invoker(task, []) is not object awaiter) throw new InvalidProgramException("ValueTask.GetAwaiter didn't return an awaiter");
            return getResult.Invoker(awaiter, []) ?? throw new InvalidDataException("Task result is NULL");
        }

        /// <summary>
        /// Get the result from a task (the task should be completed already!)
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="type">Result type</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static object? GetValueTaskResultNullable(in object task, Type? type = null)
        {
            Type taskType = task.GetType();
            if (!taskType.IsGenericType) throw new ArgumentException("Not a result task", nameof(task));
            Type resultType = taskType.GetGenericArgumentCached(index: 0);
            if (type is not null && !type.IsAssignableFrom(resultType)) throw new ArgumentException($"Task returns {resultType}", nameof(type));
            type ??= resultType;
            if (taskType.GetGenericTypeDefinition() != typeof(ValueTask<>)) throw new ArgumentException("Not a ValueTask<>", nameof(task));
            Type awaiterType = typeof(ValueTaskAwaiter<>).MakeGenericType(resultType);
            MethodInfoExt getAwaiter = typeof(ValueTask<>).MakeGenericType(resultType).GetMethodsCached()
                    .FirstOrDefault(m => m.Name == nameof(ValueTask<bool>.GetAwaiter) && m.ReturnType == awaiterType)
                    ?? throw new InvalidProgramException("ValueTask.GetAwaiter method not found"),
                getResult = awaiterType.GetMethodsCached().FirstOrDefault(m => m.Name == nameof(ValueTaskAwaiter<bool>.GetResult) && m.ReturnType == resultType)
                    ?? throw new InvalidProgramException("ValueTaskAwaiter.GetResult method not found");
            if (getAwaiter.Invoker is null) throw new InvalidProgramException("ValueTask.GetAwaiter can't be invoked");
            if (getResult.Invoker is null) throw new InvalidProgramException("ValueTaskAwaiter.GetResult can't be invoked");
            if (getAwaiter.Invoker(task, []) is not object awaiter) throw new InvalidProgramException("ValueTask.GetAwaiter didn't return an awaiter");
            return getResult.Invoker(awaiter, []);
        }

        /// <summary>
        /// Await a task
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="task">Task</param>
        private static async Task AwaitTaskAsync<T>(Task<T> task) => await task.DynamicContext();

        /// <summary>
        /// Await a value task
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="task">Task</param>
        private static async ValueTask AwaitValueTaskAsync<T>(ValueTask<T> task) => await task.DynamicContext();
    }
}

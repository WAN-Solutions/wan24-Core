﻿using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    // Invoke method
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Invoke a method and complete parameters with default values
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object? InvokeAuto(this MethodInfo mi, in object? obj, params object?[] param)
            => mi.Invoke(obj, mi.GetParametersCached().GetDiObjects(param));

        /// <summary>
        /// Invoke a method and complete parameters with default values
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object? InvokeAuto(this MethodInfo mi, in object? obj, in IServiceProvider serviceProvider, params object?[] param)
            => mi.Invoke(obj, mi.GetParametersCached().GetDiObjects(param, serviceProvider));

        /// <summary>
        /// Invoke a method and complete parameters with default values
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static T? InvokeAuto<T>(this MethodInfo mi, in object? obj, params object?[] param) => (T?)InvokeAuto(mi, obj, param);

        /// <summary>
        /// Invoke a method and complete parameters with default values
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static T? InvokeAuto<T>(this MethodInfo mi, in object? obj, in IServiceProvider serviceProvider, params object?[] param)
            => (T?)InvokeAuto(mi, obj, serviceProvider, param);

        /// <summary>
        /// Invoke a method and complete parameters with default values
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        public static async Task<object?> InvokeAutoAsync(this MethodInfo mi, object? obj, params object?[] param)
        {
            await Task.Yield();
            if (mi.ReturnType is not Type retType) throw new ArgumentException("Method has no return type (task expected)", nameof(mi));
            bool isTask = typeof(Task).IsAssignableFrom(retType);
            if (!isTask && !typeof(ValueTask).IsAssignableFrom(retType)) throw new ArgumentException("Task return type expected", nameof(mi));
            if (mi.Invoke(obj, await mi.GetParametersCached().GetDiObjectsAsync(param).DynamicContext()) is not object res) return null;
            if (isTask)
            {
                Task task = (Task)res;
                await task.DynamicContext();
                return retType.IsGenericType ? task.GetResult(retType.GetGenericArguments()[0]) : null;
            }
            else
            {
                ValueTask task = (ValueTask)res;
                await task.DynamicContext();
                return retType.IsGenericType ? task.GetResult(retType.GetGenericArguments()[0]) : null;
            }
        }

        /// <summary>
        /// Invoke a method and complete parameters with default values
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        public static async Task<object?> InvokeAutoAsync(this MethodInfo mi, object? obj, IServiceProvider serviceProvider, params object?[] param)
        {
            await Task.Yield();
            if (mi.ReturnType is not Type retType) throw new ArgumentException("Method has no return type (task expected)", nameof(mi));
            bool isTask = typeof(Task).IsAssignableFrom(retType);
            if (!isTask && !typeof(ValueTask).IsAssignableFrom(retType)) throw new ArgumentException("Task return type expected", nameof(mi));
            if (mi.Invoke(obj, await mi.GetParametersCached().GetDiObjectsAsync(param, serviceProvider).DynamicContext()) is not object res) return null;
            if (isTask)
            {
                Task task = (Task)res;
                await task.DynamicContext();
                return retType.IsGenericType ? task.GetResult(retType.GetGenericArguments()[0]) : null;
            }
            else
            {
                ValueTask task = (ValueTask)res;
                await task.DynamicContext();
                return retType.IsGenericType ? task.GetResult(retType.GetGenericArguments()[0]) : null;
            }
        }

        /// <summary>
        /// Invoke a method and complete parameters with default values
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        public static async Task<object?> InvokeAutoAsync(this MethodInfo mi, object? obj, IAsyncServiceProvider serviceProvider, params object?[] param)
        {
            await Task.Yield();
            if (mi.ReturnType is not Type retType) throw new ArgumentException("Method has no return type (task expected)", nameof(mi));
            bool isTask = typeof(Task).IsAssignableFrom(retType);
            if (!isTask && !typeof(ValueTask).IsAssignableFrom(retType)) throw new ArgumentException("Task return type expected", nameof(mi));
            if (mi.Invoke(obj, await mi.GetParametersCached().GetDiObjectsAsync(param, serviceProvider).DynamicContext()) is not object res) return null;
            if (isTask)
            {
                Task task = (Task)res;
                await task.DynamicContext();
                return retType.IsGenericType ? task.GetResult(retType.GetGenericArguments()[0]) : null;
            }
            else
            {
                ValueTask task = (ValueTask)res;
                await task.DynamicContext();
                return retType.IsGenericType ? task.GetResult(retType.GetGenericArguments()[0]) : null;
            }
        }

        /// <summary>
        /// Invoke a method and complete parameters with default values
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static async Task<T?> InvokeAutoAsync<T>(this MethodInfo mi, object? obj, params object?[] param)
            => (T?)await InvokeAutoAsync(mi, obj, param).DynamicContext();

        /// <summary>
        /// Invoke a method and complete parameters with default values
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static async Task<T?> InvokeAutoAsync<T>(this MethodInfo mi, object? obj, IServiceProvider serviceProvider, params object?[] param)
            => (T?)await InvokeAutoAsync(mi, obj, serviceProvider, param).DynamicContext();

        /// <summary>
        /// Invoke a method and complete parameters with default values
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static async Task<T?> InvokeAutoAsync<T>(this MethodInfo mi, object? obj, IAsyncServiceProvider serviceProvider, params object?[] param)
            => (T?)await InvokeAutoAsync(mi, obj, serviceProvider, param).DynamicContext();
    }
}

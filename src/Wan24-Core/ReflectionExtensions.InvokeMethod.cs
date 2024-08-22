using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    // Invoke method
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Invoke a method
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        public static object? InvokeFast(this MethodInfo mi, in object? obj, params object?[] param) => CreateMethodInvoker(mi)(obj, param);

        /// <summary>
        /// Invoke a method and complete parameters with default values
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object? InvokeAuto(this MethodInfo mi, in object? obj, params object?[] param)
            => mi.InvokeFast(obj, mi.GetParametersCached().GetDiObjects(param));

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
            => mi.InvokeFast(obj, mi.GetParametersCached().GetDiObjects(param, serviceProvider));

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
        /// Invoke a method and complete parameters with default values (and wait for a possible task result)
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        public static async Task<object?> InvokeAutoAsync(this MethodInfo mi, object? obj, params object?[] param)
        {
            await Task.Yield();
            if (mi.InvokeFast(obj, await mi.GetParametersCached().GetDiObjectsAsync(param).DynamicContext()) is not object res) return null;
            return res.IsTask() ? await TaskHelper.GetAnyTaskResultAsync(res).DynamicContext() : null;
        }

        /// <summary>
        /// Invoke a method and complete parameters with default values (and wait for a possible task result)
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        public static async Task<object?> InvokeAutoAsync(this MethodInfo mi, object? obj, IServiceProvider serviceProvider, params object?[] param)
        {
            await Task.Yield();
            if (mi.InvokeFast(obj, await mi.GetParametersCached().GetDiObjectsAsync(param, serviceProvider).DynamicContext()) is not object res) return null;
            return res.IsTask() ? await TaskHelper.GetAnyTaskResultAsync(res).DynamicContext() : null;
        }

        /// <summary>
        /// Invoke a method and complete parameters with default values (and wait for a possible task result)
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        public static async Task<object?> InvokeAutoAsync(this MethodInfo mi, object? obj, IAsyncServiceProvider serviceProvider, params object?[] param)
        {
            await Task.Yield();
            if (mi.InvokeFast(obj, await mi.GetParametersCached().GetDiObjectsAsync(param, serviceProvider).DynamicContext()) is not object res) return null;
            return res.IsTask() ? await TaskHelper.GetAnyTaskResultAsync(res).DynamicContext() : res;
        }

        /// <summary>
        /// Invoke a method and complete parameters with default values (and wait for a possible task result)
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
        /// Invoke a method and complete parameters with default values (and wait for a possible task result)
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
        /// Invoke a method and complete parameters with default values (and wait for a possible task result)
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

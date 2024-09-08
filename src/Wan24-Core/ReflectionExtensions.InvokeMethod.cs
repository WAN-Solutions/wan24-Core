using System.Linq.Expressions;
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

        /// <summary>
        /// Determine if a method invoker can be created
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>If a method invoker can be created</returns>
        public static bool CanCreateMethodInvoker(this MethodInfo mi)
            => !mi.IsSpecialName &&
                (
                    mi.DeclaringType is null ||
                    (
                        !mi.DeclaringType.IsGenericTypeDefinition &&
                        !mi.DeclaringType.ContainsGenericParameters
                    )
                ) &&
                (
                    !mi.IsGenericMethod ||
                    mi.IsConstructedGenericMethod
                ) &&
                !mi.GetParametersCached().Any(p => p.ParameterType.IsByRef || p.ParameterType.IsByRefLike || p.IsOut || p.ParameterType.IsPointer) &&
                !mi.ReturnType.IsByRef && !mi.ReturnType.IsByRefLike && !mi.ReturnType.IsPointer;


        /// <summary>
        /// Create a delegate for method invocation
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>Invocation delegate (first parameter is the target object, which must be <see langword="null"/> for a static method, 2nd parameter are the method parameters 
        /// (all are required!))</returns>
        public static Func<object?, object?[], object?> CreateMethodInvoker(this MethodInfo mi)
        {
            if (mi.IsSpecialName) throw new ArgumentException("Non-special method required", nameof(mi));
            if (mi.IsGenericMethod && !mi.IsConstructedGenericMethod) throw new ArgumentException("Constructed generic method required", nameof(mi));
            int hc = mi.GetHashCode();
            if (MethodInvokeDelegateCache.TryGetValue(hc, out Func<object?, object?[], object?>? res)) return res;
            ParameterExpression paramsArg = Expression.Parameter(typeof(object?[]), "parameters");
            ParameterInfo[] pis = [..mi.GetParametersCached()];
            Expression[] parameters = new Expression[pis.Length];
            for (int i = 0; i < pis.Length; i++)
                parameters[i] = Expression.Convert(Expression.ArrayIndex(paramsArg, Expression.Constant(i)), pis[i].ParameterType);
            if (mi.IsStatic)
            {
                if (mi.ReturnType == typeof(void))
                {
                    Action<object?[]> lambda = Expression.Lambda<Action<object?[]>>(Expression.Call(null, mi, [.. parameters]), paramsArg).CompileExt();
                    res = (obj, param) =>
                    {
                        lambda(param);
                        return null;
                    };
                }
                else
                {
                    Func<object?[], object?> lambda = Expression.Lambda<Func<object?[], object?>>(
                        Expression.Convert(Expression.Call(null, mi, [.. parameters]), typeof(object)),
                        paramsArg
                        ).CompileExt();
                    res = (obj, param) => lambda(param);
                }
            }
            else
            {
                ParameterExpression objArg = Expression.Parameter(typeof(object), "obj");
                UnaryExpression objArg2 = Expression.Convert(objArg, mi.DeclaringType!);
                if (mi.ReturnType == typeof(void))
                {
                    Action<object?, object?[]> lambda = Expression.Lambda<Action<object?, object?[]>>(
                        Expression.Call(objArg2, mi, [.. parameters]),
                        objArg,
                        paramsArg
                        ).CompileExt();
                    res = (obj, param) =>
                    {
                        lambda(obj, param);
                        return null;
                    };
                }
                else
                {
                    Func<object?, object?[], object?> lambda = Expression.Lambda<Func<object?, object?[], object?>>(
                        Expression.Convert(Expression.Call(objArg2, mi, [.. parameters]), typeof(object)),
                        objArg,
                        paramsArg
                        ).CompileExt();
                    res = (obj, param) => lambda(obj, param);
                }
            }
            MethodInvokeDelegateCache.TryAdd(hc, res);
            return res;
        }

        /// <summary>
        /// Get a method which matches the given filter parameters
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Method name (<see langword="null"/> to skip method name check)</param>
        /// <param name="bindingFlags">Binding flags (used to select methods of the type)</param>
        /// <param name="filter">Additional filter function (needs to return <see langword="true"/> to accept the given method as return value)</param>
        /// <param name="genericArgumentCount">Number of generic arguments</param>
        /// <param name="exactTypes">Require exact types?</param>
        /// <param name="returnType">Return type (<see langword="null"/> to skip the return value type check)</param>
        /// <param name="parameterTypes">Parameter types (or <see langword="null"/> to skip parameter type checks; a single <see langword="null"/> parameter type would allow any parameter 
        /// type)</param>
        /// <returns>Matching method</returns>
        public static MethodInfoExt? GetMethod(
            this Type type,
            in string? name = null,
            in BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic,
            in Func<MethodInfoExt, bool>? filter = null,
            in int? genericArgumentCount = null,
            in bool exactTypes = true,
            in Type? returnType = null,
            params Type?[]? parameterTypes
            )
        {
            Type[] pt;
            foreach (MethodInfoExt mi in type.GetMethodsCached(bindingFlags))
            {
                // Check method name, return type and generic argument count
                if (
                    (name is not null && mi.Name != name) ||
                    (returnType is not null && !MatchReturnType(mi, returnType, exactTypes)) ||
                    (genericArgumentCount is not null && (genericArgumentCount.Value != 0 != mi.Method.IsGenericMethodDefinition || mi.GenericArguments.Length != genericArgumentCount.Value))
                    )
                    continue;
                // Check parameters
                if (parameterTypes is not null)
                {
                    pt = mi.Parameters.Select(p => p.ParameterType).ToArray();
                    if (pt.Length != parameterTypes.Length) continue;
                    bool isMatch = true;
                    for (int i = 0; i < parameterTypes.Length; i++)
                    {
                        if (parameterTypes[i] is null || MatchParameterType(mi, pt[i], parameterTypes[i]!, exactTypes)) continue;
                        isMatch = false;
                        break;
                    }
                    if (!isMatch) continue;
                }
                // Run additional filter
                if (!(filter?.Invoke(mi) ?? true)) continue;
                return mi;
            }
            return null;
        }
    }
}

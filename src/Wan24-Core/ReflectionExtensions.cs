using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Reflection extensions
    /// </summary>
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Invoke a method and complete parameters with default values
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        public static object? InvokeAuto(this MethodInfo mi, in object? obj, params object?[] param)
        {
            List<object?> par = new(param);
            ParameterInfo[] pis = mi.GetParametersCached();
#pragma warning disable IDE0018 // Can declare inline
            object? di;
#pragma warning restore IDE0018 // Can declare inline
            for (int i = par.Count; i < pis.Length; i++)
                if (DiHelper.GetDiObject(pis[i].ParameterType, out di))
                {
                    par.Add(di);
                }
                else if (!pis[i].HasDefaultValue)
                {
                    throw new ArgumentException($"Missing required parameter #{i} ({pis[i].Name}) for invoking method {mi.DeclaringType}.{mi.Name}", nameof(param));
                }
                else
                {
                    par.Add(pis[i].DefaultValue);
                }
            return mi.Invoke(obj, par.ToArray());
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
        public static T? InvokeAuto<T>(this MethodInfo mi, in object? obj, params object?[] param) => (T?)InvokeAuto(mi, obj, param);

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
            List<object?> par = new(param);
            ParameterInfo[] pis = mi.GetParametersCached();
            object? di;
            bool use;
            for (int i = par.Count; i < pis.Length; i++)
            {
                (di, use) = await DiHelper.GetDiObjectAsync(pis[i].ParameterType).DynamicContext();
                if (use)
                {
                    par.Add(di);
                }
                else if (!pis[i].HasDefaultValue)
                {
                    throw new ArgumentException($"Missing required parameter #{i} ({pis[i].Name}) for invoking method {mi.DeclaringType}.{mi.Name}", nameof(param));
                }
                else
                {
                    par.Add(pis[i].DefaultValue);
                }
            }
            if (mi.Invoke(obj, par.ToArray()) is not object res) return null;
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
        /// Invoke a constructor and complete parameters with default values
        /// </summary>
        /// <param name="ci">Constructor</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        public static object InvokeAuto(this ConstructorInfo ci, params object?[] param)
        {
            List<object?> par = new(param);
            ParameterInfo[] pis = ci.GetParametersCached();
#pragma warning disable IDE0018 // Can declare inline
            object? di;
#pragma warning restore IDE0018 // Can declare inline
            for (int i = par.Count; i < pis.Length; i++)
                if (DiHelper.GetDiObject(pis[i].ParameterType, out di))
                {
                    par.Add(di);
                }
                else if (!pis[i].HasDefaultValue)
                {
                    throw new ArgumentException($"Missing required parameter #{i} ({pis[i].Name}) for invoking constructor of {ci.DeclaringType}.{ci.Name}", nameof(param));
                }
                else
                {
                    par.Add(pis[i].DefaultValue);
                }
            return ci.Invoke(par.ToArray());
        }

        /// <summary>
        /// Invoke a constructor and complete parameters with default values
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="ci">Constructor</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static T InvokeAuto<T>(this ConstructorInfo ci, params object?[] param) => (T)InvokeAuto(ci, param);

        /// <summary>
        /// Invoke a possible constructor and complete parameters with default values
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="usePrivate">Use private constructors, too?</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance or <see langword="null"/>, if no constructor could be found</returns>
        public static object? ConstructAuto(this Type type, in bool usePrivate = false, params object?[] param)
            => ConstructAuto(type, out _, usePrivate, param);

        /// <summary>
        /// Invoke a possible constructor and complete parameters with default values
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="usedConstructor">Used constructor</param>
        /// <param name="usePrivate">Use private constructors, too?</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance or <see langword="null"/>, if no constructor could be found</returns>
        public static object? ConstructAuto(this Type type, out ConstructorInfo? usedConstructor, in bool usePrivate = false, params object?[] param)
        {
            NullabilityInfoContext nic = new();
            BindingFlags flags = usePrivate
                ? BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                : BindingFlags.Public | BindingFlags.Instance;
            List<object?> par;
            ParameterInfo[] pis;
#pragma warning disable IDE0018 // Can declare inline
            object? di;
#pragma warning restore IDE0018 // Can declare inline
            bool use;
            foreach (ConstructorInfo ci in type.GetConstructors(flags).OrderByDescending(c => c.GetParametersCached().Length))
            {
                par = new();
                pis = ci.GetParametersCached();
                use = true;
                for (int i = par.Count; use && i < pis.Length; i++)
                    if (param.FirstOrDefault(p => p is not null && pis[i].ParameterType.IsAssignableFrom(p.GetType())) is object p) par.Add(p);
                    else if (DiHelper.GetDiObject(pis[i].ParameterType, out di)) par.Add(di);
                    else if (pis[i].HasDefaultValue) par.Add(pis[i].DefaultValue);
                    else if (pis[i].IsNullable(nic)) par.Add(null);
                    else use = false;
                if (!use) continue;
                usedConstructor = ci;
                return ci.Invoke(par.ToArray());
            }
            throw new InvalidOperationException($"{type} can't be instanced (private: {usePrivate}) with the given parameters");
        }

        /// <summary>
        /// Determine if a type is nullable
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) is not null;

        /// <summary>
        /// Determine if a method return value is nullable
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsNullable(this MethodInfo mi, in NullabilityInfoContext? nic = null) => IsNullable(mi.ReturnParameter, nic);

        /// <summary>
        /// Determine if a parameter is nullable
        /// </summary>
        /// <param name="pi">Parameter</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNullable(this ParameterInfo pi, in NullabilityInfoContext? nic = null) => (nic ?? new NullabilityInfoContext()).Create(pi).IsNullable();

        /// <summary>
        /// Determine if a property is nullable
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNullable(this PropertyInfo pi, in NullabilityInfoContext? nic = null) => (nic ?? new NullabilityInfoContext()).Create(pi).IsNullable();

        /// <summary>
        /// Determine if a field is nullable
        /// </summary>
        /// <param name="fi">Field</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNullable(this FieldInfo fi, in NullabilityInfoContext? nic = null) => (nic ?? new NullabilityInfoContext()).Create(fi).IsNullable();

        /// <summary>
        /// Determine if nullable
        /// </summary>
        /// <param name="ni">Nullability info</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNullable(this NullabilityInfo ni) => !(ni.ReadState == NullabilityState.NotNull || ni.WriteState == NullabilityState.NotNull);

        /// <summary>
        /// Get the final array element type of a multi-dimensional array type
        /// </summary>
        /// <param name="type">Array type</param>
        /// <returns>Final array element type</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static Type GetFinalElementType(this Type type)
        {
            if (type.IsAbstract) throw new ArgumentException("Not an array type", nameof(type));
            Type res = type.GetElementType()!;
            while (res.IsArray) res = res.GetElementType()!;
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
        public static MethodInfo? GetMethod(
            this Type type,
            in string? name = null,
            in BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic,
            in Func<MethodInfo, bool>? filter = null,
            in int? genericArgumentCount = null,
            in bool exactTypes = true,
            in Type? returnType = null,
            params Type?[]? parameterTypes
            )
        {
            Type[] pt;
            foreach (MethodInfo mi in type.GetMethodsCached(bindingFlags))
            {
                // Check method name, return type and generic argument count
                if (
                    (name is not null && mi.Name != name) ||
                    (returnType is not null && !MatchReturnType(mi, returnType, exactTypes)) ||
                    (genericArgumentCount is not null && (genericArgumentCount.Value != 0 != mi.IsGenericMethodDefinition || mi.GetGenericArguments().Length != genericArgumentCount.Value))
                    )
                    continue;
                // Check parameters
                if (parameterTypes is not null)
                {
                    pt = mi.GetParametersCached().Select(p => p.ParameterType).ToArray();
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

        /// <summary>
        /// Get a constructor which matches the given filter parameters
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags (used to select methods of the type)</param>
        /// <param name="filter">Additional filter function (needs to return <see langword="true"/> to accept the given method as return value)</param>
        /// <param name="exactTypes">Require exact types?</param>
        /// <param name="parameterTypes">Parameter types (or <see langword="null"/> to skip parameter type checks; a single <see langword="null"/> parameter type would allow any parameter 
        /// type)</param>
        /// <returns>Matching constructor</returns>
        public static ConstructorInfo? GetConstructor(
            this Type type,
            in BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic,
            in Func<ConstructorInfo, bool>? filter = null,
            in bool exactTypes = true,
            params Type?[]? parameterTypes
            )
        {
            Type[] pt;
            foreach (ConstructorInfo ci in type.GetConstructorsCached(bindingFlags))
            {
                // Check parameters
                if (parameterTypes is not null)
                {
                    pt = ci.GetParametersCached().Select(p => p.ParameterType).ToArray();
                    if (pt.Length != parameterTypes.Length) continue;
                    bool isMatch = true;
                    for (int i = 0; i < parameterTypes.Length; i++)
                    {
                        if (parameterTypes[i] is null || MatchParameterType(pt[i], parameterTypes[i]!, exactTypes)) continue;
                        isMatch = false;
                        break;
                    }
                    if (!isMatch) continue;
                }
                // Run additional filter
                if (!(filter?.Invoke(ci) ?? true)) continue;
                return ci;
            }
            return null;
        }

        /// <summary>
        /// Match a method return type against an expected type
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="expectedReturnType">Expected return type</param>
        /// <param name="exact">Exact type match?</param>
        /// <returns>Is match?</returns>
        private static bool MatchReturnType(in MethodInfo method, in Type expectedReturnType, in bool exact)
        {
            if (method.IsGenericMethod && method.ReturnType.IsGenericType && method.ReturnType.GetGenericArguments()[0].IsGenericMethodParameter)
                return expectedReturnType.IsAssignableFrom(method.ReturnType.GetGenericTypeDefinition());
            return (exact && method.ReturnType == expectedReturnType) || (!exact && expectedReturnType.IsAssignableFrom(method.ReturnType));
        }

        /// <summary>
        /// Match a method parameter type against an expected type
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="parameterType">Parameter type</param>
        /// <param name="expectedType">Expected type</param>
        /// <param name="exact">Exact type match?</param>
        /// <returns>Is match?</returns>
        private static bool MatchParameterType(in MethodInfo method, in Type parameterType, in Type expectedType, in bool exact)
        {
            if (method.IsGenericMethod && parameterType.IsGenericType && parameterType.GetGenericArguments()[0].IsGenericMethodParameter)
                return parameterType.GetGenericTypeDefinition().IsAssignableFrom(expectedType);
            return (exact && parameterType == expectedType) || (!exact && parameterType.IsAssignableFrom(expectedType));
        }

        /// <summary>
        /// Match a constructor parameter type against an expected type
        /// </summary>
        /// <param name="parameterType">Parameter type</param>
        /// <param name="expectedType">Expected type</param>
        /// <param name="exact">Exact type match?</param>
        /// <returns>Is match?</returns>
        private static bool MatchParameterType(in Type parameterType, in Type expectedType, in bool exact)
            => (exact && parameterType == expectedType) || (!exact && parameterType.IsAssignableFrom(expectedType));
    }
}

using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Reflection extensions
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Invoke a method and complete parameters with default values
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="obj">Object</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        public static object? InvokeAuto(this MethodInfo mi, object? obj, params object?[] param)
        {
            List<object?> par = new(param);
            ParameterInfo[] pis = mi.GetParameters();
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
        public static T? InvokeAuto<T>(this MethodInfo mi, object? obj, params object?[] param) => (T?)InvokeAuto(mi, obj, param);

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
            ParameterInfo[] pis = mi.GetParameters();
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
        public static async Task<T?> InvokeAutoAsync<T>(this MethodInfo mi, object? obj, params object?[] param)
            => (T?)await InvokeAutoAsync(mi, obj, param).DynamicContext();

        /// <summary>
        /// Invoke a possible constructor and complete parameters with default values
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="usePrivate">Use private constructors, too?</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance or <see langword="null"/>, if no constructor could be found</returns>
        public static object? ConstructAuto(this Type type, bool usePrivate = false, params object?[] param)
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
            foreach (ConstructorInfo ci in type.GetConstructors(flags).OrderByDescending(c => c.GetParameters().Length))
            {
                par = new();
                pis = ci.GetParameters();
                use = true;
                for (int i = par.Count; use && i < pis.Length; i++)
                    if (param.FirstOrDefault(p => p != null && pis[i].ParameterType.IsAssignableFrom(p.GetType())) is object p) par.Add(p);
                    else if (DiHelper.GetDiObject(pis[i].ParameterType, out di)) par.Add(di);
                    else if (pis[i].HasDefaultValue) par.Add(pis[i].DefaultValue);
                    else if (pis[i].IsNullable(nic)) par.Add(null);
                    else use = false;
                if (!use) continue;
                return ci.Invoke(par.ToArray());
            }
            throw new InvalidOperationException($"{type} can't be instanced (private: {usePrivate}) with the given parameters");
        }

        /// <summary>
        /// Determine if a type is nullable
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Is nullable?</returns>
        public static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) != null;

        /// <summary>
        /// Determine if a method return value is nullable
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        public static bool IsNullable(this MethodInfo mi, NullabilityInfoContext? nic = null) => IsNullable(mi.ReturnParameter, nic);

        /// <summary>
        /// Determine if a parameter is nullable
        /// </summary>
        /// <param name="pi">Parameter</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        public static bool IsNullable(this ParameterInfo pi, NullabilityInfoContext? nic = null) => (nic ?? new NullabilityInfoContext()).Create(pi).IsNullable();

        /// <summary>
        /// Determine if a property is nullable
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        public static bool IsNullable(this PropertyInfo pi, NullabilityInfoContext? nic = null) => (nic ?? new NullabilityInfoContext()).Create(pi).IsNullable();

        /// <summary>
        /// Determine if a field is nullable
        /// </summary>
        /// <param name="fi">Field</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        public static bool IsNullable(this FieldInfo fi, NullabilityInfoContext? nic = null) => (nic ?? new NullabilityInfoContext()).Create(fi).IsNullable();

        /// <summary>
        /// Determine if nullable
        /// </summary>
        /// <param name="ni">Nullability info</param>
        /// <returns>Is nullable?</returns>
        public static bool IsNullable(this NullabilityInfo ni) => !(ni.ReadState == NullabilityState.NotNull || ni.WriteState == NullabilityState.NotNull);

        /// <summary>
        /// Get a method which matches the given filter parameters
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Method name (<see langword="null"/> to skip method name check)</param>
        /// <param name="bindingFlags">Binding flags (used to select methods of the type)</param>
        /// <param name="filter">Additional filter function (needs to return <see langword="true"/> to accept the given method as return value)</param>
        /// <param name="exactTypes">Compare exact types (otherwise use <see cref="Type.IsAssignableFrom(Type?)"/>)</param>
        /// <param name="genericArgumentCount">Number of generic arguments</param>
        /// <param name="returnType">Return type (<see langword="null"/> equals a <see cref="void"/> return value)</param>
        /// <param name="parameterTypes">Parameter types (or <see langword="null"/> to skip parameter type checks)</param>
        /// <returns>Matching method</returns>
        public static MethodInfo? GetMethod(
            this Type type, 
            string? name, 
            BindingFlags bindingFlags, 
            Func<MethodInfo, bool>? filter, 
            bool exactTypes, 
            int genericArgumentCount, 
            Type? returnType, 
            params Type[]? parameterTypes
            )
        {
            Type[] pt;//TODO Test ReflectionExtensions.GetMethod
            foreach (MethodInfo mi in type.GetMethods(bindingFlags))
            {
                // Check method name, return type and generic argument count
                if (
                    (name != null && mi.Name != name) ||
                    (returnType == null && mi.ReturnType != null) || (returnType != null && (exactTypes ? mi.ReturnType != returnType : !returnType.IsAssignableFrom(mi.ReturnType))) ||
                    (!mi.IsGenericMethodDefinition || mi.GetGenericArguments().Length != genericArgumentCount)
                    )
                    continue;
                // Check parameters
                if (parameterTypes != null)
                {
                    pt = mi.GetParameters().Select(p => p.ParameterType).ToArray();
                    if (pt.Length != parameterTypes.Length || (exactTypes && !parameterTypes.SequenceEqual(pt))) continue;
                    if (!exactTypes)
                    {
                        bool isMatch = true;
                        for (int i = 0; i < parameterTypes.Length; i++)
                        {
                            if (parameterTypes[i].IsAssignableFrom(pt[i])) continue;
                            isMatch = false;
                            break;
                        }
                        if (!isMatch) continue;
                    }
                }
                // Run additional filter
                if (!(filter?.Invoke(mi) ?? true)) continue;
                return mi;
            }
            return null;
        }
    }
}

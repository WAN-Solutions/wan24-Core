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
            object? di;
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
            object? di;
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
        /// Change the type of an object
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Converted object</returns>
        public static T ConvertType<T>(this object obj) => (T)Convert.ChangeType(obj, typeof(T));
    }
}

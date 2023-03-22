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
            for (int i = par.Count; i < pis.Length; i++)
            {
                if (!pis[i].HasDefaultValue)
                    throw new ArgumentException($"Missing required parameter #{i} ({pis[i].Name}) for invoking method {mi.DeclaringType}.{mi.Name}", nameof(param));
                par.Add(pis[i].DefaultValue);
            }
            return mi.Invoke(obj, par.ToArray());
        }
    }
}

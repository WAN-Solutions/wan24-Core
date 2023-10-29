using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Dependency injection helper extensions
    /// </summary>
    public static class DiHelperExtensions
    {
        /// <summary>
        /// Get DI objects
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <param name="values">Given values</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="nic">Nullability info context</param>
        /// <param name="throwOnMissing">Throw an exception on missing value?</param>
        /// <returns>Parameters (if the length doesn't match the number of parameters, DI failed)</returns>
        public static object?[] GetDiObjects(
            this IEnumerable<ParameterInfo> parameters,
            object?[]? values = null,
            IServiceProvider? serviceProvider = null,
            NullabilityInfoContext? nic = null,
            bool throwOnMissing = true
            )
        {
            List<object?> valueList = new(values ?? Array.Empty<object?>());
            List<object?> res = new();
            int i,
                len = valueList.Count;
#pragma warning disable IDE0018 // Declare inline
            object? value;
#pragma warning restore IDE0018 // Declare inline
            bool found;
            foreach (ParameterInfo pi in parameters)
            {
                found = false;
                for (i = 0; i < len; i++)
                {
                    if (valueList[i] is null || !pi.ParameterType.IsAssignableFrom(valueList[i]!.GetType())) continue;
                    res.Add(valueList[i]);
                    valueList.RemoveAt(i);
                    len--;
                    found = true;
                    break;
                }
                if (!found)
                    if (DiHelper.GetDiObject(pi.ParameterType, out value, serviceProvider)) res.Add(value);
                    else if (pi.IsNullable(nic ??= new()) && (!pi.HasDefaultValue || pi.DefaultValue is null)) res.Add(null);
                    else if (pi.HasDefaultValue) res.Add(pi.DefaultValue);
                    else if (throwOnMissing) throw new ArgumentException($"Can't get value of type {pi.ParameterType}", pi.Name);
            }
            return res.ToArray();
        }

        /// <summary>
        /// Get DI objects
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <param name="values">Given values</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="nic">Nullability info context</param>
        /// <param name="throwOnMissing">Throw an exception on missing value?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Parameters (if the length doesn't match the number of parameters, DI failed)</returns>
        public static async Task<object?[]> GetDiObjectsAsync(
            this IEnumerable<ParameterInfo> parameters,
            object?[]? values = null,
            IServiceProvider? serviceProvider = null,
            NullabilityInfoContext? nic = null,
            bool throwOnMissing = true,
            CancellationToken cancellationToken = default
            )
        {
            List<object?> valueList = new(values ?? Array.Empty<object?>());
            List<object?> res = new();
            int i,
                len = valueList.Count;
            DiHelper.AsyncResult di;
            bool found;
            foreach (ParameterInfo pi in parameters)
            {
                found = false;
                for (i = 0; i < len; i++)
                {
                    if (valueList[i] is null || !pi.ParameterType.IsAssignableFrom(valueList[i]!.GetType())) continue;
                    res.Add(valueList[i]);
                    valueList.RemoveAt(i);
                    len--;
                    found = true;
                    break;
                }
                if (found) continue;
                di = await DiHelper.GetDiObjectAsync(pi.ParameterType, serviceProvider, cancellationToken).DynamicContext();
                if (di.Use) res.Add(di.Object);
                else if (pi.IsNullable(nic ??= new()) && (!pi.HasDefaultValue || pi.DefaultValue is null)) res.Add(null);
                else if (pi.HasDefaultValue) res.Add(pi.DefaultValue);
                else if (throwOnMissing) throw new ArgumentException($"Can't get value of type {pi.ParameterType}", pi.Name);
            }
            return res.ToArray();
        }

        /// <summary>
        /// Get DI objects
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="values">Given values</param>
        /// <param name="nic">Nullability info context</param>
        /// <param name="throwOnMissing">Throw an exception on missing value?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Parameters (if the length doesn't match the number of parameters, DI failed)</returns>
        public static async Task<object?[]> GetDiObjectsAsync(
            this IEnumerable<ParameterInfo> parameters,
            IAsyncServiceProvider serviceProvider,
            object?[]? values = null,
            NullabilityInfoContext? nic = null,
            bool throwOnMissing = true,
            CancellationToken cancellationToken = default
            )
        {
            List<object?> valueList = new(values ?? Array.Empty<object?>());
            List<object?> res = new();
            int i,
                len = valueList.Count;
            object? value;
            bool found;
            foreach (ParameterInfo pi in parameters)
            {
                found = false;
                for (i = 0; i < len; i++)
                {
                    if (valueList[i] is null || !pi.ParameterType.IsAssignableFrom(valueList[i]!.GetType())) continue;
                    res.Add(valueList[i]);
                    valueList.RemoveAt(i);
                    len--;
                    found = true;
                    break;
                }
                if (found) continue;
                value = await serviceProvider.GetServiceAsync(pi.ParameterType, cancellationToken).DynamicContext();
                if (value is not null) res.Add(value);
                else if (pi.IsNullable(nic ??= new()) && (!pi.HasDefaultValue || pi.DefaultValue is null)) res.Add(null);
                else if (pi.HasDefaultValue) res.Add(pi.DefaultValue);
                else if (throwOnMissing) throw new ArgumentException($"Can't get value of type {pi.ParameterType}", pi.Name);
            }
            return res.ToArray();
        }

        /// <summary>
        /// Get DI objects
        /// </summary>
        /// <param name="types">Types</param>
        /// <param name="values">Given values</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="throwOnMissing">Throw an exception on missing value?</param>
        /// <returns>DI objects (if the length doesn't match the number of types, DI failed)</returns>
        public static object?[] GetDiObjects(this IEnumerable<Type> types, object?[]? values = null, IServiceProvider? serviceProvider = null, bool throwOnMissing = true)
        {
            List<object?> valueList = new(values ?? Array.Empty<object?>());
            List<object?> res = new();
            int i,
                len = valueList.Count,
                index = 0;
#pragma warning disable IDE0018 // Declare inline
            object? value;
#pragma warning restore IDE0018 // Declare inline
            bool found;
            foreach (Type type in types)
            {
                found = false;
                for (i = 0; i < len; i++)
                {
                    if (valueList[i] is null || !type.IsAssignableFrom(valueList[i]!.GetType())) continue;
                    res.Add(valueList[i]);
                    valueList.RemoveAt(i);
                    len--;
                    found = true;
                    break;
                }
                if (!found)
                    if (DiHelper.GetDiObject(type, out value, serviceProvider)) res.Add(value);
                    else if (type.IsNullable()) res.Add(null);
                    else if (throwOnMissing) throw new ArgumentException($"Missing value #{index} of type {type}", nameof(values));
                index++;
            }
            return res.ToArray();
        }

        /// <summary>
        /// Get DI objects
        /// </summary>
        /// <param name="types">Types</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="values">Given values</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="throwOnMissing">Throw an exception on missing value?</param>
        /// <returns>DI objects (if the length doesn't match the number of types, DI failed)</returns>
        public static async Task<object?[]> GetDiObjectsAsync(
            this IEnumerable<Type> types,
            IAsyncServiceProvider serviceProvider,
            object?[]? values = null,
            bool throwOnMissing = true,
            CancellationToken cancellationToken = default
            )
        {
            List<object?> valueList = new(values ?? Array.Empty<object?>());
            List<object?> res = new();
            int i,
                len = valueList.Count,
                index = 0;
            object? value;
            bool found;
            foreach (Type type in types)
            {
                found = false;
                for (i = 0; i < len; i++)
                {
                    if (valueList[i] is null || !type.IsAssignableFrom(valueList[i]!.GetType())) continue;
                    res.Add(valueList[i]);
                    valueList.RemoveAt(i);
                    len--;
                    found = true;
                    break;
                }
                if (found)
                {
                    index++;
                    continue;
                }
                value = await serviceProvider.GetServiceAsync(type, cancellationToken).DynamicContext();
                if (value is not null) res.Add(value);
                else if (type.IsNullable()) res.Add(null);
                else if (throwOnMissing) throw new ArgumentException($"Missing value #{index} of type {type}", nameof(values));
                index++;
            }
            return res.ToArray();
        }

        /// <summary>
        /// Get DI objects
        /// </summary>
        /// <param name="types">Types</param>
        /// <param name="values">Given values</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="throwOnMissing">Throw an exception on missing value?</param>
        /// <returns>DI objects (if the length doesn't match the number of types, DI failed)</returns>
        public static async Task<object?[]> GetDiObjectsAsync(
            this IEnumerable<Type> types,
            object?[]? values = null,
            IServiceProvider? serviceProvider = null,
            bool throwOnMissing = true,
            CancellationToken cancellationToken = default
            )
        {
            List<object?> valueList = new(values ?? Array.Empty<object?>());
            List<object?> res = new();
            int i,
                len = valueList.Count,
                index = 0;
            DiHelper.AsyncResult di;
            bool found;
            foreach (Type type in types)
            {
                found = false;
                for (i = 0; i < len; i++)
                {
                    if (valueList[i] is null || !type.IsAssignableFrom(valueList[i]!.GetType())) continue;
                    res.Add(valueList[i]);
                    valueList.RemoveAt(i);
                    len--;
                    found = true;
                    break;
                }
                if (found)
                {
                    index++;
                    continue;
                }
                di = await DiHelper.GetDiObjectAsync(type, serviceProvider, cancellationToken).DynamicContext();
                if (di.Use) res.Add(di.Object);
                else if (type.IsNullable()) res.Add(null);
                else if (throwOnMissing) throw new ArgumentException($"Missing value #{index} of type {type}", nameof(values));
                index++;
            }
            return res.ToArray();
        }
    }
}

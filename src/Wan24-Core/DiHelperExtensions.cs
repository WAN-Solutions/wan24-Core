using Microsoft.Extensions.DependencyInjection;
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
        /// <param name="throwOnMissing">Throw an exception on missing value? (will throw on missing keyed DI parameter anyway)</param>
        /// <param name="valuesAreOrdered">If values are ordered in the order of the given parameters</param>
        /// <returns>Parameters (if the length doesn't match the number of parameters, DI failed)</returns>
        public static object?[] GetDiObjects(
            this IEnumerable<ParameterInfo> parameters,
            object?[]? values = null,
            IServiceProvider? serviceProvider = null,
            NullabilityInfoContext? nic = null,
            bool throwOnMissing = true,
            bool valuesAreOrdered = false
            )
        {
            if (valuesAreOrdered && values is null)
                throw new ArgumentNullException(nameof(values));
            List<object?> valueList = new(values ?? []),
                res = [];
            int i,
                len = valueList.Count,
                current = -1;
            object? value;
            bool found,
                nullable;
            Type paramType;
            nic ??= new();
            foreach (ParameterInfo pi in parameters)
            {
                found = false;
                current++;
                paramType = pi.ParameterType.GetNonNullableType();
                nullable = paramType != pi.ParameterType || pi.IsNullable(nic);
                if (valuesAreOrdered)
                {
                    if (current < len)
                    {
                        if (valueList[current] is null)
                        {
                            if (!nullable)
                                throw new ArgumentNullException(pi.Name);
                        }
                        else if (!paramType.IsAssignableFrom(valueList[current]!.GetType().GetNonNullableType()))
                        {
                            throw new ArgumentException($"{pi.ParameterType} value type expected ({valueList[current]!.GetType()} given)", pi.Name);
                        }
                        res.Add(valueList[current]);
                        continue;
                    }
                }
                else
                {
                    for (i = 0; i < len; i++)
                    {
                        if (valueList[i] is null || !paramType.IsAssignableFrom(valueList[i]!.GetType().GetNonNullableType())) continue;
                        res.Add(valueList[i]);
                        valueList.RemoveAt(i);
                        len--;
                        found = true;
                        break;
                    }
                    if (found) continue;
                }
                if (pi.GetCustomAttributeCached<FromKeyedServicesAttribute>() is FromKeyedServicesAttribute attr)
                {
                    if (!DiHelper.GetKeyedDiObject(attr.Key, paramType, out value, serviceProvider))
                        throw new ArgumentException($"Can't get keyed DI value of type {pi.ParameterType} from key \"{attr.Key}\"", pi.Name);
                    res.Add(value);
                }
                else if (DiHelper.GetDiObject(paramType, out value, serviceProvider)) res.Add(value);
                else if (nullable && (!pi.HasDefaultValue || pi.DefaultValue is null)) res.Add(null);
                else if (pi.HasDefaultValue) res.Add(pi.DefaultValue);
                else if (throwOnMissing) throw new ArgumentException($"Can't get value of type {pi.ParameterType}", pi.Name);
            }
            return [.. res];
        }

        /// <summary>
        /// Get DI objects
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <param name="values">Given values</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="nic">Nullability info context</param>
        /// <param name="throwOnMissing">Throw an exception on missing value? (will throw on missing keyed DI parameter anyway)</param>
        /// <param name="valuesAreOrdered">If values are ordered in the order of the given parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Parameters (if the length doesn't match the number of parameters, DI failed)</returns>
        public static async Task<object?[]> GetDiObjectsAsync(
            this IEnumerable<ParameterInfo> parameters,
            object?[]? values = null,
            IServiceProvider? serviceProvider = null,
            NullabilityInfoContext? nic = null,
            bool throwOnMissing = true,
            bool valuesAreOrdered = false,
            CancellationToken cancellationToken = default
            )
        {
            if (valuesAreOrdered && values is null)
                throw new ArgumentNullException(nameof(values));
            List<object?> valueList = new(values ?? []),
                res = [];
            int i,
                len = valueList.Count,
                current = -1;
            ITryAsyncResult di;
            bool found,
                nullable;
            Type paramType;
            nic ??= new();
            foreach (ParameterInfo pi in parameters)
            {
                found = false;
                current++;
                paramType = pi.ParameterType.GetNonNullableType();
                nullable = paramType != pi.ParameterType || pi.IsNullable(nic);
                if (valuesAreOrdered)
                {
                    if (current < len)
                    {
                        if (valueList[current] is null)
                        {
                            if (!nullable)
                                throw new ArgumentNullException(pi.Name);
                        }
                        else if (!paramType.IsAssignableFrom(valueList[current]!.GetType().GetNonNullableType()))
                        {
                            throw new ArgumentException($"{pi.ParameterType} value type expected ({valueList[current]!.GetType()} given)", pi.Name);
                        }
                        res.Add(valueList[current]);
                        continue;
                    }
                }
                else
                {
                    for (i = 0; i < len; i++)
                    {
                        if (valueList[i] is null || !paramType.IsAssignableFrom(valueList[i]!.GetType().GetNonNullableType())) continue;
                        res.Add(valueList[i]);
                        valueList.RemoveAt(i);
                        len--;
                        found = true;
                        break;
                    }
                    if (found) continue;
                }
                if (pi.GetCustomAttributeCached<FromKeyedServicesAttribute>() is FromKeyedServicesAttribute attr)
                {
                    di = await DiHelper.GetKeyedDiObjectAsync(attr.Key, paramType, serviceProvider, cancellationToken: cancellationToken).DynamicContext();
                    if (!di.Succeed)
                        throw new ArgumentException($"Can't get keyed DI value of type {pi.ParameterType} from key \"{attr.Key}\"", pi.Name);
                    res.Add(di.Result);
                    continue;
                }
                di = await DiHelper.GetDiObjectAsync(paramType, serviceProvider, cancellationToken).DynamicContext();
                if (di.Succeed) res.Add(di.Result);
                else if (nullable && (!pi.HasDefaultValue || pi.DefaultValue is null)) res.Add(null);
                else if (pi.HasDefaultValue) res.Add(pi.DefaultValue);
                else if (throwOnMissing) throw new ArgumentException($"Can't get value of type {pi.ParameterType}", pi.Name);
            }
            return [.. res];
        }

        /// <summary>
        /// Get DI objects
        /// </summary>
        /// <param name="types">Types</param>
        /// <param name="values">Given values</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="throwOnMissing">Throw an exception on missing value?</param>
        /// <param name="valuesAreOrdered">If values are ordered in the order of the given parameters</param>
        /// <returns>DI objects (if the length doesn't match the number of types, DI failed)</returns>
        public static object?[] GetDiObjects(
            this IEnumerable<Type> types,
            object?[]? values = null,
            IServiceProvider? serviceProvider = null,
            bool throwOnMissing = true,
            bool valuesAreOrdered = false
            )
        {
            List<object?> valueList = new(values ?? []),
                res = [];
            int i,
                len = valueList.Count,
                index = 0;
#pragma warning disable IDE0018 // Declare inline
            object? value;
#pragma warning restore IDE0018 // Declare inline
            bool found,
                nullable;
            Type current;
            foreach (Type type in types)
            {
                found = false;
                current = type.GetNonNullableType();
                nullable = current != type || type.IsNullable();
                if (valuesAreOrdered)
                {
                    if (index < len)
                    {
                        if (valueList[index] is null && !nullable)
                            throw new ArgumentNullException(nameof(values), $"{type} value type expected (NULL given)");
                        if (valueList[index] is not null && !current.IsAssignableFrom(valueList[index]!.GetType().GetNonNullableType()))
                            throw new ArgumentException($"{type} value type expected ({valueList[index]!.GetType()} given)", nameof(values));
                        res.Add(valueList[index]);
                        found = true;
                    }
                }
                else
                {
                    for (i = 0; i < len; i++)
                    {
                        if (valueList[i] is null || !current.IsAssignableFrom(valueList[i]!.GetType().GetNonNullableType())) continue;
                        res.Add(valueList[i]);
                        valueList.RemoveAt(i);
                        len--;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    if (DiHelper.GetDiObject(current, out value, serviceProvider)) res.Add(value);
                    else if (nullable) res.Add(null);
                    else if (throwOnMissing) throw new ArgumentException($"Missing value #{index} of type {type}", nameof(values));
                }
                index++;
            }
            return [.. res];
        }

        /// <summary>
        /// Get DI objects
        /// </summary>
        /// <param name="types">Types</param>
        /// <param name="values">Given values</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="throwOnMissing">Throw an exception on missing value?</param>
        /// <param name="valuesAreOrdered">If values are ordered in the order of the given parameters</param>
        /// <returns>DI objects (if the length doesn't match the number of types, DI failed)</returns>
        public static async Task<object?[]> GetDiObjectsAsync(
            this IEnumerable<Type> types,
            object?[]? values = null,
            IServiceProvider? serviceProvider = null,
            bool throwOnMissing = true,
            bool valuesAreOrdered = false,
            CancellationToken cancellationToken = default
            )
        {
            List<object?> valueList = new(values ?? []),
                res = [];
            int i,
                len = valueList.Count,
                index = 0;
            ITryAsyncResult di;
            bool found,
                nullable;
            Type current;
            foreach (Type type in types)
            {
                found = false;
                current = type.GetNonNullableType();
                nullable = current != type || type.IsNullable();
                if (valuesAreOrdered)
                {
                    if (index < len)
                    {
                        if (valueList[index] is null && !nullable)
                            throw new ArgumentNullException(nameof(values), $"{type} value type expected (NULL given)");
                        if (valueList[index] is not null && !current.IsAssignableFrom(valueList[index]!.GetType().GetNonNullableType()))
                            throw new ArgumentException($"{type} value type expected ({valueList[index]!.GetType()} given)", nameof(values));
                        res.Add(valueList[index]);
                        found = true;
                    }
                }
                else
                {
                    for (i = 0; i < len; i++)
                    {
                        if (valueList[i] is null || !current.IsAssignableFrom(valueList[i]!.GetType().GetNonNullableType())) continue;
                        res.Add(valueList[i]);
                        valueList.RemoveAt(i);
                        len--;
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    index++;
                    continue;
                }
                di = await DiHelper.GetDiObjectAsync(current, serviceProvider, cancellationToken).DynamicContext();
                if (di.Succeed) res.Add(di.Result);
                else if (nullable) res.Add(null);
                else if (throwOnMissing) throw new ArgumentException($"Missing value #{index} of type {type}", nameof(values));
                index++;
            }
            return [.. res];
        }
    }
}

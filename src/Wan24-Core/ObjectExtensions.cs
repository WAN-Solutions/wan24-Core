using System.Collections;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Object extensions
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Determine if an object is within an object list
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="objects">Object list</param>
        /// <returns>Is within the object list?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool In(this object obj, params object?[] objects)
        {
            int len = objects.Length;
            if (len == 0) return false;
            for (int i = 0; i < len; i++) if (objects[i] is not null && obj.Equals(objects[i])) return true;
            return false;
        }

        /// <summary>
        /// Determine if an object is within an object list
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="objects">Object list</param>
        /// <returns>Is within the object list?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool In<T>(this T obj, in IEnumerable<T?> objects) => objects.Contains(obj);

        /// <summary>
        /// Change the type of an object
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Converted object</returns>
        [Obsolete("Use CastType instead")]//TODO Remove in v3
        public static T ConvertType<T>(this object obj) => CastType<T>(obj);

        /// <summary>
        /// Ensure a valid object state
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="state">State (if <see langword="false"/>, the state is invalid)</param>
        /// <param name="error">Error message</param>
        /// <returns>Object</returns>
        /// <exception cref="InvalidOperationException">The object is not in a state which allows to perform the operation</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public static T EnsureValidState<T>(this T obj, in bool state, in string? error = null)
        {
            if (state) return obj;
            throw new InvalidOperationException(error);
        }

        /// <summary>
        /// Get the display text
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Display text</returns>
        public static string GetDisplayText<T>(this T value)
        {
            Contract.Assert(value is not null);
            if (
                value is ICustomAttributeProvider attributeProvider &&
                (
                    attributeProvider.GetCustomAttributeCached<DisplayTextAttribute>()?.DisplayText ??
                    attributeProvider.GetCustomAttributeCached<DisplayNameAttribute>()?.DisplayName
                ) is string res
                ) return res;
            Type t = value.GetType();
            if (t.IsEnum)
            {
                string str = value.ToString() ?? throw new ArgumentException("Not an enumeration value", nameof(value));
                IEnumInfo info = EnumExtensions.GetEnumInfo(t);
                return info.ValueDisplayTexts.TryGetValue(str, out string? text) ? text : str;
            }
            else if (StringValueConverter.CanConvertToString(value.GetType()) && StringValueConverter.Convert(value) is string txt)
            {
                return txt;
            }
            else
            {
                return value.ToString() ?? t.ToString();
            }
        }

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <typeparam name="tObject">Object type</typeparam>
        /// <typeparam name="tReturn">Return type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="action">Action</param>
        /// <returns>Return value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static tReturn Do<tObject, tReturn>(this tObject obj, in Func<tObject, tReturn> action) => action(obj);

        /// <summary>
        /// Create a dictionary from an object only using basic types which are JSON compatible (hides <see cref="SensitiveDataAttribute"/> marked property values)
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="maxRecursionDepth">Maximum recursion depth</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, object?> ToDictionary<T>(this T obj, in int maxRecursionDepth = 32) => ToDictionary([], obj, [], maxRecursionDepth);

        /// <summary>
        /// Create a dictionary from an object only using basic types which are JSON compatible (hides <see cref="SensitiveDataAttribute"/> marked property values)
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="maxRecursionDepth">Maximum recursion depth</param>
        /// <returns>Dictionary</returns>
        public static OrderedDictionary<string, object?> ToOrderedDictionary<T>(this T obj, in int maxRecursionDepth = 32)
            => ToOrderedDictionary([], obj, [], maxRecursionDepth);

        /// <summary>
        /// Create a dictionary from an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="obj">Object</param>
        /// <param name="seen">Seen objects (to prevent an endless recursion)</param>
        /// <param name="maxRecursionDepth">Maximum recursion depth</param>
        /// <returns>Dictionary</returns>
        private static Dictionary<string, object?> ToDictionary<T>(in Dictionary<string, object?> dict, in T obj, in HashSet<object> seen, in int maxRecursionDepth)
        {
            int newMaxDepth = maxRecursionDepth - 1;
            if (newMaxDepth < 0) throw new StackOverflowException();
            Contract.Assert(obj is not null);
            seen.Add(obj);
            object? value;
            string jsonValue;
            SensitiveDataAttribute? attr;
            foreach (PropertyInfoExt pi in from pi in obj.GetType().GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public)
                                           where pi.Getter is not null
                                           select pi)
            {
                Contract.Assert(pi.Getter is not null);
                value = pi.Getter(obj);
                if (value is not null)
                {
                    attr = pi.GetCustomAttributeCached<SensitiveDataAttribute>();
                    if (attr is not null)
                        value = attr.CanSanitizeValue
                            ? attr.CreateSanitizedValue(obj, pi.Name, value)
                            : $"(sensitive data of type {value.GetType().ToString() ?? "NULL"} removed)";
                    if (value is not null && (attr?.CanSanitizeValue ?? true))
                        if (seen.Contains(value))
                        {
                            value = $"(skipping already seen value of type {value.GetType()} to avoid an endless recursion)";
                        }
                        else
                        {
                            jsonValue = value.ToJson();
                            if (jsonValue[0] == '{')
                            {
                                value = newMaxDepth == 0
                                    ? $"(max. recursion depth reached, skipping object type of {value.GetType()})"
                                    : ToDictionary(new(), value, seen, newMaxDepth);
                            }
                            else if (jsonValue[0] == '[')
                            {
                                value = newMaxDepth == 0
                                    ? $"(max. recursion depth reached, skipping array type of {value.GetType()})"
                                    : ToArray((IEnumerable)value, seen, newMaxDepth);
                            }
                        }
                }
                dict[pi.Name] = value;
            }
            seen.Remove(obj);
            return dict;
        }

        /// <summary>
        /// Create a dictionary from an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="obj">Object</param>
        /// <param name="seen">Seen objects (to prevent an endless recursion)</param>
        /// <param name="maxRecursionDepth">Maximum recursion depth</param>
        /// <returns>Dictionary</returns>
        private static OrderedDictionary<string, object?> ToOrderedDictionary<T>(
            in OrderedDictionary<string, object?> dict,
            in T obj,
            in HashSet<object> seen,
            in int maxRecursionDepth
            )
        {
            int newMaxDepth = maxRecursionDepth - 1;
            if (newMaxDepth < 0) throw new StackOverflowException();
            Contract.Assert(obj is not null);
            seen.Add(obj);
            object? value;
            string jsonValue;
            SensitiveDataAttribute? attr;
            foreach (PropertyInfoExt pi in from pi in obj.GetType().GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public)
                                           where pi.Getter is not null
                                           orderby pi.Name
                                           select pi)
            {
                Contract.Assert(pi.Getter is not null);
                value = pi.Getter(obj);
                if (value is not null)
                {
                    attr = pi.GetCustomAttributeCached<SensitiveDataAttribute>();
                    if (attr is not null)
                        value = attr.CanSanitizeValue
                            ? attr.CreateSanitizedValue(obj, pi.Name, value)
                            : $"(sensitive data of type {value.GetType().ToString() ?? "NULL"} removed)";
                    if (value is not null && (attr?.CanSanitizeValue ?? true))
                        if (seen.Contains(value))
                        {
                            value = $"(skipping already seen value of type {value.GetType()} to avoid an endless recursion)";
                        }
                        else
                        {
                            jsonValue = value.ToJson();
                            if (jsonValue[0] == '{')
                            {
                                value = newMaxDepth == 0
                                    ? $"(max. recursion depth reached, skipping object type of {value.GetType()})"
                                    : ToOrderedDictionary(new(), value, seen, newMaxDepth);
                            }
                            else if (jsonValue[0] == '[')
                            {
                                value = newMaxDepth == 0
                                    ? $"(max. recursion depth reached, skipping array type of {value.GetType()})"
                                    : ToArray2((IEnumerable)value, seen, newMaxDepth);
                            }
                        }
                }
                dict[pi.Name] = value;
            }
            seen.Remove(obj);
            return dict;
        }

        /// <summary>
        /// Create an array
        /// </summary>
        /// <param name="arr">Array</param>
        /// <param name="seen">Seen objects (to prevent an endless recursion)</param>
        /// <param name="maxRecursionDepth">Maximum recursion depth</param>
        /// <returns>Array</returns>
        private static object?[] ToArray(in IEnumerable arr, in HashSet<object> seen, in int maxRecursionDepth)
        {
            int newMaxDepth = maxRecursionDepth - 1;
            if (newMaxDepth < 0) throw new StackOverflowException();
            seen.Add(arr);
            string jsonValue;
            List<object?> res = [];
            foreach (object? item in arr)
                if (item is null)
                {
                    res.Add(null);
                }
                else if (seen.Contains(item))
                {
                    res.Add($"(skipping already seen item of type {item.GetType()}) to avoid an endless recursion");
                }
                else
                {
                    jsonValue = item.ToJson();
                    if (jsonValue[0] == '{')
                    {
                        res.Add(
                            newMaxDepth == 0
                                ? $"(max. recursion depth reached, skipping object type of {item.GetType()})"
                                : ToDictionary(new(), item, seen, newMaxDepth)
                                );
                    }
                    else if (jsonValue[0] == '[')
                    {
                        res.Add(
                            newMaxDepth == 0
                                ? $"(max. recursion depth reached, skipping array type of {item.GetType()})"
                                : ToArray((IEnumerable)item, seen, newMaxDepth)
                                );
                    }
                    else
                    {
                        res.Add(item);
                    }
                }
            seen.Remove(arr);
            return [.. res];
        }

        /// <summary>
        /// Create an array
        /// </summary>
        /// <param name="arr">Array</param>
        /// <param name="seen">Seen objects (to prevent an endless recursion)</param>
        /// <param name="maxRecursionDepth">Maximum recursion depth</param>
        /// <returns>Array</returns>
        private static object?[] ToArray2(in IEnumerable arr, in HashSet<object> seen, in int maxRecursionDepth)
        {
            int newMaxDepth = maxRecursionDepth - 1;
            if (newMaxDepth < 0) throw new StackOverflowException();
            seen.Add(arr);
            string jsonValue;
            List<object?> res = [];
            foreach (object? item in arr)
                if (item is null)
                {
                    res.Add(null);
                }
                else if (seen.Contains(item))
                {
                    res.Add($"(skipping already seen item of type {item.GetType()}) to avoid an endless recursion");
                }
                else
                {
                    jsonValue = item.ToJson();
                    if (jsonValue[0] == '{')
                    {
                        res.Add(
                            newMaxDepth == 0
                                ? $"(max. recursion depth reached, skipping object type of {item.GetType()})"
                                : ToOrderedDictionary(new(), item, seen, newMaxDepth)
                                );
                    }
                    else if (jsonValue[0] == '[')
                    {
                        res.Add(
                            newMaxDepth == 0
                                ? $"(max. recursion depth reached, skipping array type of {item.GetType()})"
                                : ToArray2((IEnumerable)item, seen, newMaxDepth)
                                );
                    }
                    else
                    {
                        res.Add(item);
                    }
                }
            seen.Remove(arr);
            return [.. res];
        }

        /// <summary>
        /// Determine if an object is disposable
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>If disposable</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsDisposable<T>(this T obj) => obj is IDisposable || obj is IAsyncDisposable;

        /// <summary>
        /// Determine if an object is a task
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>If the object is a task</returns>
        public static bool IsTask<T>(this T obj) => obj is Task || IsValueTask(obj);

        /// <summary>
        /// Determine if an object is a value task
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>If the object is a value task</returns>
        public static bool IsValueTask<T>(this T obj)
        {
            if (obj is null) return false;
            if (obj is ValueTask) return true;
            Type type = obj.GetType();
            return type.IsValueType && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>);
        }

        /// <summary>
        /// Cast a type
        /// </summary>
        /// <typeparam name="T">Numeric result type</typeparam>
        /// <param name="value">Enumeration value</param>
        /// <returns>Numeric value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static T CastType<T>(this object value)
            => value is T res
                ? res
                : typeof(T).IsEnum
                    ? value switch
                    {
                        string str => (T)EnumHelper.ParseEnum(typeof(T), str),
                        _ => (T)Enum.ToObject(typeof(T), value)
                    }
                    : (T)Convert.ChangeType(value, typeof(T));
    }
}

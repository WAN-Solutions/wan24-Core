using System.Collections;
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
        public static bool In(this object obj, params object?[] objects) => objects.Contains(obj);

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
        [TargetedPatchingOptOut("Tiny method")]
        public static T ConvertType<T>(this object obj) => (T)Convert.ChangeType(obj, typeof(T));

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
            if ((value as ICustomAttributeProvider)?.GetCustomAttributeCached<DisplayTextAttribute>()?.DisplayText is string res) return res;
            Type t = value!.GetType();
            if (t.IsEnum)
            {
                string str = value.ToString() ?? throw new ArgumentException("Not an enumeration value", nameof(value));
                IEnumInfo info = EnumExtensions.GetEnumInfo(t);
                return info.ValueDisplayTexts.ContainsKey(str) ? info.ValueDisplayTexts[str] : str;
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
    }
}

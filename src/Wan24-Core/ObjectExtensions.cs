﻿using System.Reflection;
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
        /// <param name="objs">Object list</param>
        /// <returns>Is within the object list?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool In(this object obj, params object?[] objs) => objs.Contains(obj);

        /// <summary>
        /// Determine if an object is within an object list
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="objs">Object list</param>
        /// <returns>Is within the object list?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool In<T>(this T obj, IEnumerable<T?> objs) => objs.Contains(obj);

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
        public static T EnsureValidState<T>(this T obj, bool state, string? error = null)
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
        public static tReturn Do<tObject, tReturn>(this tObject obj, Func<tObject, tReturn> action) => action(obj);
    }
}

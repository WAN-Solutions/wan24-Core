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
        /// <param name="value">Value</param>
        /// <returns>Display text</returns>
        public static string GetDisplayText(this object value)
        {
            string? res = (value as ICustomAttributeProvider)?.GetCustomAttributeCached<DisplayTextAttribute>()?.DisplayText;
            if (res != null) return res;
            Type t = value.GetType();
            if (t.IsEnum)
            {
                IEnumInfo info = EnumExtensions.GetEnumInfo(value.GetType());
                string str = value.ToString() ?? throw new ArgumentException("Not an enumeration value", nameof(value));
                return info.ValueDisplayTexts.ContainsKey(str) ? info.ValueDisplayTexts[str] : str;
            }
            else
            {
                return value.ToString() ?? value.GetType().ToString();
            }
        }
    }
}

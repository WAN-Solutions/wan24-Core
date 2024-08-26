using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    // Expressions
    public sealed partial class EnumInfo<T>
    {
        /// <summary>
        /// Value lookup (key is the hash code)
        /// </summary>
        internal static readonly FrozenDictionary<int, EnumValue> EnumValueLookup;
        /// <summary>
        /// Get an enumeration value as a string
        /// </summary>
        internal static readonly Func<T, string> AsStringExpression;//FIXME Performance of AsStringExpression not yet satisfying
        /// <summary>
        /// Get the name of an enumeration value
        /// </summary>
        internal static readonly Func<T, string?> AsNameExpression;//FIXME Performance of AsNameExpression not yet satisfying
        /// <summary>
        /// Get an enumeration value as its numeric value
        /// </summary>
        internal static readonly Func<T, object> AsNumericValueExpression;
        /// <summary>
        /// Parse a string as enumeration value
        /// </summary>
        internal static readonly Func<string, bool, string> GetKeyExpression;//FIXME Performance of GetKeyExpression not yet satisfying

        /// <summary>
        /// Parse string helper
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="ignoreCase">If to ignore the string case</param>
        /// <returns>Enumeration value</returns>
        internal static T? ParseStringHelper(in string str, in bool ignoreCase)
        {
            string key = GetKeyExpression(str, ignoreCase);
            return key.Length > 0
                ? KeyValues[key]
                : Enum.TryParse<T>(str, ignoreCase, out T res)
                    ? res
                    : null;
        }

        /// <summary>
        /// Enumeration value
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal readonly record struct EnumValue
        {
            /// <summary>
            /// Value
            /// </summary>
            public readonly T Value;
            /// <summary>
            /// Numeric value
            /// </summary>
            public readonly object NumericValue;
            /// <summary>
            /// Name
            /// </summary>
            public readonly string Name;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="value">Value</param>
            public EnumValue(in T value)
            {
                Value = value;
                Name = value.ToString();
                NumericValue = NumericValues[Name];
            }
        }
    }
}

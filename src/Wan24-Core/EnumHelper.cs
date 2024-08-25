using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Enumeration helper methods
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// <see cref="ParseEnum{T}(in string, in bool)"/>
        /// </summary>
        private static readonly MethodInfoExt ParseEnumMethod;

        /// <summary>
        /// Constructor
        /// </summary>
        static EnumHelper()
            => ParseEnumMethod = typeof(EnumHelper).GetMethodsCached().FirstOrDefault(m => m.Name == nameof(ParseEnum) && m.IsGenericMethod)
                ?? throw new InvalidProgramException();

        /// <summary>
        /// Parse an enumeration value from a string
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="ignoreCase">If to ignore the string case</param>
        /// <returns>Enumeration value</returns>
        public static T ParseEnum<T>(in string value, in bool ignoreCase = false) where T : struct, Enum, IConvertible
            => TryParseEnum<T>(value, out T res, ignoreCase)
                ? res
                : throw new FormatException();

        /// <summary>
        /// Parse an enumeration value from a string
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="result">Enumeration value</param>
        /// <param name="ignoreCase">If to ignore the string case</param>
        /// <returns>If succeed</returns>
        public static bool TryParseEnum<T>(in string value, out T result, in bool ignoreCase = false) where T : struct, Enum, IConvertible
        {
            T? res = EnumInfo<T>.ParseStringExpression(value, ignoreCase);
            if (res.HasValue)
            {
                result = res.Value;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Parse an enumeration value from a string
        /// </summary>
        /// <param name="type">Enumeration type</param>
        /// <param name="value">Value</param>
        /// <param name="ignoreCase">If to ignore the string case</param>
        /// <returns>Enumeration value</returns>
        public static object ParseEnum(in Type type, in string value, in bool ignoreCase = false)
            => ParseEnumMethod.MakeGenericMethod(type).Invoker!(null, [value, ignoreCase]) ?? throw new InvalidProgramException();

        /// <summary>
        /// Parse an enumeration value from a string
        /// </summary>
        /// <param name="type">Enumeration type</param>
        /// <param name="value">Value</param>
        /// <param name="result">Enumeration value</param>
        /// <param name="ignoreCase">If to ignore the string case</param>
        /// <returns>If succeed</returns>
        public static bool TryParseEnum(in Type type, in string value, [NotNullWhen(returnValue: true)] out object? result, in bool ignoreCase = false)
        {
            MethodInfoExt parseMethod = typeof(EnumInfo<>).MakeGenericType(type).GetMethodCached(nameof(EnumInfo<OptInOut>.ParseStringHelper), BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new InvalidProgramException();
            result = parseMethod.Invoker!(null, [value, ignoreCase]);
            return result is not null;
        }
    }
}

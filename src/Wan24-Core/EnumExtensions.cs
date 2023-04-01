using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Enumeration extensions
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Flags value name
        /// </summary>
        public const string FLAGS_NAME = "FLAGS";

        /// <summary>
        /// Get the display text for an enumeration value
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Display text</returns>
        public static string GetDisplayText<T>(this T value) where T : struct, Enum, IConvertible
            => typeof(T).GetField(value.ToString())?.GetCustomAttribute<DisplayTextAttribute>()?.DisplayText ?? value.ToString();

        /// <summary>
        /// Get the display text for an enumeration value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Display text</returns>
        public static string GetEnumDisplayText(this object value)
        {
            Type type = value.GetType();
            if (!type.IsEnum) throw new ArgumentException("Enumeration value expected", nameof(value));
            return type.GetField(value.ToString()!)?.GetCustomAttribute<DisplayTextAttribute>()?.DisplayText ?? value.ToString()!;
        }

        /// <summary>
        /// Remove flags from a mixed enumeration flags value
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Value without flags</returns>
        public static T RemoveFlags<T>(this T value) where T : struct, Enum, IConvertible
            => CastType<T>(value.IsUnsigned()
                ? value.MayContainFlags() ? CastType<ulong>(value) & ~CastType<ulong>(Enum.Parse<T>(FLAGS_NAME)) : CastType<ulong>(value)
                : value.MayContainFlags() ? CastType<long>(value) & ~CastType<long>(Enum.Parse<T>(FLAGS_NAME)) : CastType<long>(value));

        /// <summary>
        /// Get only the flags from a mixed enumeration flags value
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Only flags</returns>
        public static T OnlyFlags<T>(this T value) where T : struct, Enum, IConvertible
            => CastType<T>(value.IsUnsigned()
                ? value.MayContainFlags() ? CastType<ulong>(value) & CastType<ulong>(Enum.Parse<T>(FLAGS_NAME)) : CastType<ulong>(value)
                : value.MayContainFlags() ? CastType<long>(value) & CastType<long>(Enum.Parse<T>(FLAGS_NAME)) : CastType<long>(value));

        /// <summary>
        /// Determine if a mixed enumeration value is a flag
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is a flag?</returns>
        public static bool IsFlag<T>(this T value) where T : struct, Enum, IConvertible => value.MayContainFlags()
            ? value.IsUnsigned()
                ? (CastType<ulong>(value) & CastType<ulong>(Enum.Parse<T>(FLAGS_NAME))) == CastType<ulong>(value)
                : (CastType<long>(value) & CastType<long>(Enum.Parse<T>(FLAGS_NAME))) == CastType<long>(value)
            : false;

        /// <summary>
        /// Determine if a mixed enumeration value is a value (not a flag)
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is a value (not a flag)?</returns>
        public static bool IsValue<T>(this T value) where T : struct, Enum, IConvertible => !value.MayContainFlags() || !value.IsFlag();

        /// <summary>
        /// Determine if an enumeration value is mixed and may contain flags
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>May contain flags?</returns>
        public static bool MayContainFlags<T>(this T value) where T : struct, Enum, IConvertible
            => typeof(T).IsEnum && typeof(T).GetCustomAttribute<FlagsAttribute>() != null && Enum.GetNames<T>().Contains(FLAGS_NAME);

        /// <summary>
        /// Determine if a type is a mixed enumeration (which contains enumeration values and flags)
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Is a mixed enumeration?</returns>
        public static bool IsMixedEnum(this Type type) => type.IsEnum && type.GetCustomAttribute<FlagsAttribute>() != null && Enum.GetNames(type).Contains(FLAGS_NAME);

        /// <summary>
        /// Cast a type
        /// </summary>
        /// <typeparam name="T">Numeric result type</typeparam>
        /// <param name="value">Enumeration value</param>
        /// <returns>Numeric value</returns>
        private static T CastType<T>(object value) where T : struct, IConvertible => typeof(T).IsEnum ? (T)Enum.ToObject(typeof(T), value) : (T)Convert.ChangeType(value, typeof(T));
    }
}

using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Enumeration extensions
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Get enumeration information
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Information</returns>
#pragma warning disable IDE0060 // Remove unused parameter
        [TargetedPatchingOptOut("Tiny method")]
        public static EnumInfo<T> GetInfo<T>(this T value) where T : struct, Enum, IConvertible => new();
#pragma warning restore IDE0060 // Remove unused parameter

        /// <summary>
        /// Get enumeration information
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <returns>Information</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static EnumInfo<T> GetInfo<T>() where T : struct, Enum, IConvertible => new();

        /// <summary>
        /// Get enumeration information
        /// </summary>
        /// <param name="type">Enumeration type</param>
        /// <returns>Information</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static IEnumInfo GetEnumInfo(this Type type) => (Activator.CreateInstance(typeof(EnumInfo<>).MakeGenericType(type)) as IEnumInfo)!;

        /// <summary>
        /// Determine if an enumeration value contains all of the given flags
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Contains all flags?</returns>
        public static bool ContainsAllFlags<T>(this T value, in T flags) where T : struct, Enum, IConvertible
        {
            EnumInfo<T> info = value.GetInfo();
            if (!info.HasFlags) return false;
            if (info.IsUnsignedNumeric)
            {
                ulong numericValue = CastType<ulong>(value),
                    numericFlags = CastType<ulong>(flags);
                return (numericValue & numericFlags) == numericFlags;
            }
            else
            {
                long numericValue = CastType<long>(value),
                    numericFlags = CastType<long>(flags);
                return (numericValue & numericFlags) == numericFlags;
            }
        }

        /// <summary>
        /// Determine if an enumeration value contains any of the given flags
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Contains any flags?</returns>
        public static bool ContainsAnyFlag<T>(this T value, params T[] flags) where T : struct, Enum, IConvertible
        {
            if (flags.Length == 0) return false;
            EnumInfo<T> info = value.GetInfo();
            if (!info.HasFlags) return false;
            if (info.IsUnsignedNumeric)
            {
                ulong numericValue = CastType<ulong>(value),
                    numericFlags = CastType<ulong>(flags[0]);
                for (int i = 1; i < flags.Length; numericFlags |= CastType<ulong>(flags[i]), i++) ;
                return (numericValue & numericFlags) != 0;
            }
            else
            {
                long numericValue = CastType<long>(value),
                    numericFlags = CastType<long>(flags[0]);
                for (int i = 1; i < flags.Length; numericFlags |= CastType<long>(flags[i]), i++) ;
                return (numericValue & numericFlags) != 0;
            }
        }

        /// <summary>
        /// Get contained flags
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Contained flags</returns>
        public static IEnumerable<T> GetContainedFlags<T>(this T value, params T[] flags) where T : struct, Enum, IConvertible
        {
            if (flags.Length == 0) yield break;
            EnumInfo<T> info = value.GetInfo();
            if (!info.HasFlags) yield break;
            if (info.IsUnsignedNumeric)
            {
                ulong numericValue = CastType<ulong>(value),
                    numericFlag;
                foreach (T flag in flags)
                {
                    numericFlag = CastType<ulong>(flag);
                    if ((numericValue & numericFlag) == numericFlag) yield return flag;
                }
            }
            else
            {
                long numericValue = CastType<long>(value),
                    numericFlag;
                foreach (T flag in flags)
                {
                    numericFlag = CastType<long>(flag);
                    if ((numericValue & numericFlag) == numericFlag) yield return flag;
                }
            }
        }

        /// <summary>
        /// Remove flags from a mixed enumeration flags value
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Value without flags</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static T RemoveFlags<T>(this T value) where T : struct, Enum, IConvertible
            => CastType<T>(EnumInfo<T>.IsUnsigned
                ? CastType<ulong>(value) & ~EnumInfo<T>.AllULongFlags
                : CastType<long>(value) & ~EnumInfo<T>.AllLongFlags);

        /// <summary>
        /// Get only the flags from a mixed enumeration flags value
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Only flags</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static T OnlyFlags<T>(this T value) where T : struct, Enum, IConvertible
            => CastType<T>(EnumInfo<T>.IsUnsigned
                ? CastType<ulong>(value) & EnumInfo<T>.AllULongFlags
                : CastType<long>(value) & EnumInfo<T>.AllLongFlags);

        /// <summary>
        /// Determine if a mixed enumeration value is a flag
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is a flag?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsFlag<T>(this T value) where T : struct, Enum, IConvertible => EnumInfo<T>.FlagValues.Contains(value);

        /// <summary>
        /// Determine if a mixed enumeration value is a value (not a flag)
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is a value (not a flag)?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsValue<T>(this T value) where T : struct, Enum, IConvertible => EnumInfo<T>.Values.Contains(value);

        /// <summary>
        /// Determine if an enumeration value is valid
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is valid?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsValid<T>(this T value) where T : struct, Enum, IConvertible => EnumInfo<T>.IsValid(value);

        /// <summary>
        /// Determine if an enumeration value is valid
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Is valid?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsValidEnumerationValue(this object value) => value.GetType().GetEnumInfo().IsValidValue(value);

        /// <summary>
        /// Determine if an enumeration value is valid
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Is valid?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsValidEnumerationValue<T>(this T value) where T : struct, Enum, IConvertible => EnumInfo<T>.IsValid(value);

        /// <summary>
        /// Cast a type
        /// </summary>
        /// <typeparam name="T">Numeric result type</typeparam>
        /// <param name="value">Enumeration value</param>
        /// <returns>Numeric value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static T CastType<T>(object value) where T : struct, IConvertible => typeof(T).IsEnum ? (T)Enum.ToObject(typeof(T), value) : (T)Convert.ChangeType(value, typeof(T));

        /// <summary>
        /// Get the enumeration value as string
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static string AsString<T>(this T value) where T : struct, Enum, IConvertible => EnumInfo<T>.AsStringExpression(value);

        /// <summary>
        /// Get the enumeration value name
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Name or <see langword="null"/>, if the value doesn't exist (or isn't a single value)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static string? AsName<T>(this T value) where T : struct, Enum, IConvertible => EnumInfo<T>.AsNameExpression(value);

        /// <summary>
        /// Get the enumeration value as its numeric value
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Numeric value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static object AsNumericValue<T>(this T value) where T : struct, Enum, IConvertible => EnumInfo<T>.AsNumericValueExpression(value);
    }
}

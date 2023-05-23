namespace wan24.Core
{
    /// <summary>
    /// Enumeration extensions
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Get enumeration informations
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Informations</returns>
#pragma warning disable IDE0060 // Remove unused parameter
        public static EnumInfo<T> GetInfo<T>(this T value) where T : struct, Enum, IConvertible => new();
#pragma warning restore IDE0060 // Remove unused parameter

        /// <summary>
        /// Get enumeration informations
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <returns>Informations</returns>
        public static EnumInfo<T> GetInfo<T>() where T : struct, Enum, IConvertible => new();

        /// <summary>
        /// Get enumeration informations
        /// </summary>
        /// <param name="type">Enumeration type</param>
        /// <returns>Informations</returns>
        public static IEnumInfo GetEnumInfo(this Type type) => (Activator.CreateInstance(typeof(EnumInfo<>).MakeGenericType(type)) as IEnumInfo)!;

        /// <summary>
        /// Get the display text for an enumeration value
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Display text</returns>
        public static string GetDisplayText<T>(this T value) where T : struct, Enum, IConvertible
            => EnumInfo<T>.DisplayTexts.ContainsKey(value.ToString()) ? EnumInfo<T>.DisplayTexts[value.ToString()] : value.ToString();

        /// <summary>
        /// Get the display text for an enumeration value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Display text</returns>
        public static string GetEnumDisplayText(this object value)
        {
            IEnumInfo info = GetEnumInfo(value.GetType());
            string str = value.ToString() ?? throw new ArgumentException("Not an enumeration value", nameof(value));
            return info.ValueDisplayTexts.ContainsKey(str) ? info.ValueDisplayTexts[str] : str;
        }

        /// <summary>
        /// Remove flags from a mixed enumeration flags value
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Value without flags</returns>
        public static T RemoveFlags<T>(this T value) where T : struct, Enum, IConvertible
            => CastType<T>(EnumInfo<T>.IsUnsigned
                ? CastType<ulong>(value) & ~(ulong)EnumInfo<T>.Flags
                : CastType<long>(value) & ~(long)EnumInfo<T>.Flags);

        /// <summary>
        /// Get only the flags from a mixed enumeration flags value
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Only flags</returns>
        public static T OnlyFlags<T>(this T value) where T : struct, Enum, IConvertible
            => CastType<T>(EnumInfo<T>.IsUnsigned
                ? CastType<ulong>(value) & (ulong)EnumInfo<T>.Flags
                : CastType<long>(value) & (long)EnumInfo<T>.Flags);

        /// <summary>
        /// Determine if a mixed enumeration value is a flag
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is a flag?</returns>
        public static bool IsFlag<T>(this T value) where T : struct, Enum, IConvertible => EnumInfo<T>.FlagValues.Contains(value);

        /// <summary>
        /// Determine if a mixed enumeration value is a value (not a flag)
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is a value (not a flag)?</returns>
        public static bool IsValue<T>(this T value) where T : struct, Enum, IConvertible => EnumInfo<T>.Values.Contains(value);

        /// <summary>
        /// Determine if an enumeration value is valid
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is valid?</returns>
        public static bool IsValid<T>(this T value) where T : struct, Enum, IConvertible => EnumInfo<T>.IsValid(value);

        /// <summary>
        /// Cast a type
        /// </summary>
        /// <typeparam name="T">Numeric result type</typeparam>
        /// <param name="value">Enumeration value</param>
        /// <returns>Numeric value</returns>
        public static T CastType<T>(object value) where T : struct, IConvertible => typeof(T).IsEnum ? (T)Enum.ToObject(typeof(T), value) : (T)Convert.ChangeType(value, typeof(T));
    }
}

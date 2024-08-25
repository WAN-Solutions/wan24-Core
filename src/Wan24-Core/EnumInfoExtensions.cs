namespace wan24.Core
{
    /// <summary>
    /// <see cref="EnumInfo{T}"/> extensions
    /// </summary>
    public static class EnumInfoExtensions
    {
        /// <summary>
        /// Get the enumeration value as string
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>String</returns>
        public static string AsString<T>(this T value) where T : struct, Enum, IConvertible => EnumInfo<T>.AsStringExpression(value);

        /// <summary>
        /// Get the enumeration value name
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Name or <see langword="null"/>, if the value doesn't exist (or isn't a single value)</returns>
        public static string? AsName<T>(this T value) where T : struct, Enum, IConvertible => EnumInfo<T>.AsNameExpression(value);

        /// <summary>
        /// Get the enumeration value as its numeric value
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Numeric value</returns>
        public static object AsNumericValue<T>(this T value) where T : struct, Enum, IConvertible => EnumInfo<T>.AsNumericValueExpression(value);
    }
}

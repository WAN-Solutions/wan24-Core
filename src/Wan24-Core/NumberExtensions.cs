namespace wan24.Core
{
    /// <summary>
    /// Number extensions
    /// </summary>
    public static class NumberExtensions
    {
        /// <summary>
        /// Determine if a numeric value is unsigned (works for enumerations, too)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is unsigned?</returns>
        public static bool IsUnsigned<T>(this T value) where T : struct, IConvertible => typeof(T).IsUnsigned();

        /// <summary>
        /// Determine if a numeric value is unsigned (works for enumerations, too)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <returns>Is unsigned?</returns>
        public static bool IsUnsigned<T>() where T : struct, IConvertible => typeof(T).IsUnsigned();

        /// <summary>
        /// Determine if a numeric type is unsigned (works for enumerations, too)
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Is unsigned?</returns>
        public static bool IsUnsigned(this Type type)
            => Activator.CreateInstance(type.IsEnum ? type.GetEnumUnderlyingType() : type) switch
            {
                sbyte => false,
                byte => true,
                short => false,
                ushort => true,
                int => false,
                uint => true,
                long => false,
                ulong => true,
                Half => false,
                float => false,
                double => false,
                decimal => false,
                _ => throw new ArgumentException($"Not a supported numeric type {type}")
            };

        /// <summary>
        /// Determine if a type is numeric
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Is a numeric type?</returns>
        public static bool IsNumeric(this Type type)
        {
            try
            {
                return type.IsValueType && typeof(IConvertible).IsAssignableFrom(type) && Activator.CreateInstance(type) switch
                {
                    sbyte => true,
                    byte => true,
                    short => true,
                    ushort => true,
                    int => true,
                    uint => true,
                    long => true,
                    ulong => true,
                    Half => true,
                    float => true,
                    double => true,
                    decimal => true,
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determine if a type is numeric and unsigned
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Is an unsigned numeric type?</returns>
        public static bool IsNumericAndUnsigned(this Type type)
        {
            try
            {
                object? value;
                return type.IsValueType && typeof(IConvertible).IsAssignableFrom(type) && (value = Activator.CreateInstance(type)) switch
                {
                    sbyte => true,
                    byte => true,
                    short => true,
                    ushort => true,
                    int => true,
                    uint => true,
                    long => true,
                    ulong => true,
                    Half => true,
                    float => true,
                    double => true,
                    decimal => true,
                    _ => false
                } && value switch
                {
                    sbyte => false,
                    byte => true,
                    short => false,
                    ushort => true,
                    int => false,
                    uint => true,
                    long => false,
                    ulong => true,
                    Half => false,
                    float => false,
                    double => false,
                    decimal => false,
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }
    }
}

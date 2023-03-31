namespace wan24.Core
{
    /// <summary>
    /// Number extensions
    /// </summary>
    public static class NumberExtensions
    {
        /// <summary>
        /// Determine if a numeric value is unsigned
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is unsigned?</returns>
        public static bool IsUnsigned<T>(this T value) where T : struct, IConvertible
        {
            try
            {
                Type type = typeof(T).IsEnum ? typeof(T).GetEnumUnderlyingType() : typeof(T);
                object v = Activator.CreateInstance(type)!;
                return v switch
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
                    _ => throw new ArgumentException($"Not a supported numeric type {typeof(T)}")
                };
            }
            catch(Exception ex)
            {
                throw new ArgumentException($"Not a supported numeric type {typeof(T)}", ex);
            }
        }

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

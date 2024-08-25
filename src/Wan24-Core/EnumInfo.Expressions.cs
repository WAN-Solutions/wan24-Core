namespace wan24.Core
{
    // Expressions
    public sealed partial class EnumInfo<T>
    {
        /// <summary>
        /// Get an enumeration value as a string
        /// </summary>
        internal static readonly Func<T, string> AsStringExpression;
        /// <summary>
        /// Get the name of an enumeration value
        /// </summary>
        internal static readonly Func<T, string?> AsNameExpression;
        /// <summary>
        /// Get an enumeration value as its numeric value
        /// </summary>
        internal static readonly Func<T, object> AsNumericValueExpression;
        /// <summary>
        /// Parse a string as enumeration value
        /// </summary>
        internal static readonly Func<string, bool, T?> ParseStringExpression;

        /// <summary>
        /// Parse string helper
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="ignoreCase">If to ignore the string case</param>
        /// <returns>Enumeration value</returns>
        internal static T? ParseStringHelper(in string str, in bool ignoreCase) => ParseStringExpression(str, ignoreCase);
    }
}

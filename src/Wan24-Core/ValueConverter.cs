namespace wan24.Core
{
    /// <summary>
    /// Value conversion (see <see cref="ValueConverterAttribute"/>)
    /// </summary>
    public static class ValueConverter
    {
        /// <summary>
        /// Converter (key is the name)
        /// </summary>
        public static readonly Dictionary<string, Converter_Delegate> Converter = [];

        /// <summary>
        /// Delegate for a converter
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Converted value</returns>
        public delegate object? Converter_Delegate(object? value);
    }
}

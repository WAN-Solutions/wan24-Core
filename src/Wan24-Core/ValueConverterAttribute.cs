namespace wan24.Core
{
    /// <summary>
    /// Attribute for a property that supports value conversion (see <see cref="ValueConverter"/>)
    /// </summary>
    /// <param name="converter">Converter name</param>
    /// <param name="reConverter">Re-converter name</param>
    [AttributeUsage(AttributeTargets.Property)]
    public class ValueConverterAttribute(string converter, string? reConverter = null) : Attribute()
    {
        /// <summary>
        /// Converter name
        /// </summary>
        public string Converter { get; } = converter;

        /// <summary>
        /// Re-converter name
        /// </summary>
        public string? ReConverter { get; } = reConverter;

        /// <summary>
        /// Convert a property value
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="obj">Object</param>
        /// <returns>Converted value</returns>
        public virtual object? Convert(in PropertyInfoExt pi, in object? obj)
        {
            if (!ValueConverter.Converter.TryGetValue(Converter, out ValueConverter.Converter_Delegate? converter))
                throw new InvalidProgramException($"Value converter \"{Converter}\" not found");
            return converter.Invoke(pi.GetValueFast(obj));
        }

        /// <summary>
        /// Re-convert a property value
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="value">Value</param>
        /// <returns>Converted value</returns>
        public virtual object? ReConvert(in PropertyInfoExt pi, in object? value)
        {
            if (ReConverter is null)
                throw new InvalidOperationException();
            if (!ValueConverter.Converter.TryGetValue(Converter, out ValueConverter.Converter_Delegate? converter))
                throw new InvalidProgramException($"Value re-converter \"{ReConverter}\" not found");
            object? res = converter.Invoke(value);
            if(res is not null && !pi.PropertyType.IsAssignableFrom(res.GetType()))
            {
                res.TryDispose();
                throw new InvalidProgramException($"Value re-converter \"{ReConverter}\" converted to {res.GetType()} ({pi.PropertyType} expected)");
            }
            return res;
        }
    }
}

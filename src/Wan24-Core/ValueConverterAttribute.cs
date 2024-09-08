namespace wan24.Core
{
    /// <summary>
    /// Attribute for a property that supports value conversion (see <see cref="ValueConverter"/>)
    /// </summary>
    /// <param name="converter">Converter name</param>
    /// <param name="reConverter">Re-converter name</param>
    /// <param name="targetType"> Target type of <see cref="Convert(in PropertyInfoExt, in object?)"/></param>
    [AttributeUsage(AttributeTargets.Property)]
    public class ValueConverterAttribute(string converter, string? reConverter = null, Type? targetType = null) : Attribute()
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
        /// Target type of <see cref="Convert(in PropertyInfoExt, in object?)"/>
        /// </summary>
        public Type? TargetType { get; } = targetType;

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
            object? res = converter.Invoke(pi.GetValueFast(obj));
            if (res is not null && TargetType is not null && !TargetType.IsAssignableFrom(res.GetType()))
                throw new InvalidProgramException($"Target type mismatch ({res.GetType()}/{TargetType})");
            return res;
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
            if (value is not null && TargetType is not null && !TargetType.IsAssignableFrom(value.GetType()))
                throw new InvalidProgramException($"Target type mismatch ({value.GetType()}/{TargetType})");
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

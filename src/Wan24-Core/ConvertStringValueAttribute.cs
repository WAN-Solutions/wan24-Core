namespace wan24.Core
{
    /// <summary>
    /// Convert as <see cref="string"/> using <see cref="StringValueConverter"/>
    /// </summary>
    /// <param name="converter">Converter name</param>
    public class ConvertStringValueAttribute(string? converter = null) : ValueConverterAttribute(converter ?? string.Empty, targetType: typeof(string))
    {
        /// <inheritdoc/>
        public override object? Convert(in PropertyInfoExt pi, in object? obj)
            => Converter.Length == 0
                ? StringValueConverter.Convert(pi.PropertyType.GetRealType(), obj)
                : StringValueConverter.NamedStringConversion(obj, Converter, pi.PropertyType.GetRealType());

        /// <inheritdoc/>
        public override object? ReConvert(in PropertyInfoExt pi, in object? value)
            => Converter.Length == 0
                ? StringValueConverter.Convert(pi.PropertyType.GetRealType(), value as string)
                : StringValueConverter.NamedObjectConversion((string?)value, Converter, pi.PropertyType.GetRealType());
    }
}

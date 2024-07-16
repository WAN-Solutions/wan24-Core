using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Property information
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="Property">Property</param>
    /// <param name="Getter">Getter</param>
    /// <param name="Setter">Setter</param>
    public sealed record class PropertyInfoExt(in PropertyInfo Property, in Func<object?, object?>? Getter, in Action<object?, object?>? Setter) : ICustomAttributeProvider
    {
        /// <summary>
        /// Property
        /// </summary>
        public PropertyInfo Property { get; } = Property;

        /// <summary>
        /// Property type
        /// </summary>
        public Type PropertyType => Property.PropertyType;

        /// <summary>
        /// Property name
        /// </summary>
        public string Name => Property.Name;

        /// <summary>
        /// Property declaring type
        /// </summary>
        public Type? DeclaringType => Property.DeclaringType;

        /// <summary>
        /// Getter
        /// </summary>
        public Func<object?, object?>? Getter { get; } = Getter;

        /// <summary>
        /// Can read?
        /// </summary>
        public bool CanRead => Getter is not null;

        /// <summary>
        /// If the property has a public getter
        /// </summary>
        public bool HasPublicGetter => Property.GetMethod?.IsPublic ?? false;

        /// <summary>
        /// Setter
        /// </summary>
        public Action<object?, object?>? Setter { get; } = Setter;

        /// <summary>
        /// If the property has a public setter
        /// </summary>
        public bool HasPublicSetter => Property.SetMethod?.IsPublic ?? false;

        /// <summary>
        /// Can write?
        /// </summary>
        public bool CanWrite => Setter is not null;

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public object[] GetCustomAttributes(bool inherit) => ((ICustomAttributeProvider)Property).GetCustomAttributes(inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public object[] GetCustomAttributes(Type attributeType, bool inherit) => ((ICustomAttributeProvider)Property).GetCustomAttributes(attributeType, inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public bool IsDefined(Type attributeType, bool inherit) => ((ICustomAttributeProvider)Property).IsDefined(attributeType, inherit);

        /// <summary>
        /// Get a value converted (see <see cref="ValueConverterAttribute"/>)
        /// </summary>
        /// <param name="obj">Instance</param>
        /// <param name="ignoreMissingConverter">If to ignore a missing converter setup</param>
        /// <returns>Value</returns>
        public object? GetConverted(object? obj, bool ignoreMissingConverter = false)
        {
            if (Getter is null)
                throw new InvalidOperationException("No getter");
            if (Property.GetCustomAttributeCached<ValueConverterAttribute>() is ValueConverterAttribute attr)
            {
                if (attr.Converter is not null)
                {
                    return attr.Convert(this, Getter(obj));
                }
                else if (!ignoreMissingConverter)
                {
                    throw new InvalidOperationException("Missing value converter");
                }
            }
            else if (!ignoreMissingConverter)
            {
                throw new InvalidOperationException($"Missing {nameof(ValueConverterAttribute)}");
            }
            return Getter(obj);
        }

        /// <summary>
        /// Set a value converted (see <see cref="ValueConverterAttribute"/>)
        /// </summary>
        /// <param name="obj">Instance</param>
        /// <param name="value">Value to set</param>
        /// <param name="ignoreMissingConverter">If to ignore a missing converter setup</param>
        public void SetConverted(object? obj, object? value, bool ignoreMissingConverter = false)
        {
            if (Setter is null)
                throw new InvalidOperationException("No setter");
            if (Property.GetCustomAttributeCached<ValueConverterAttribute>() is ValueConverterAttribute attr)
            {
                if (attr.ReConverter is not null)
                {
                    Setter(obj, attr.ReConvert(this, value));
                }
                else if(ignoreMissingConverter)
                {
                    Setter(obj, value);
                }
                else
                {
                    throw new InvalidOperationException("Missing value converter");
                }
            }
            else if(!ignoreMissingConverter)
            {
                throw new InvalidOperationException($"Missing {nameof(ValueConverterAttribute)}");
            }
        }

        /// <summary>
        /// Cast as <see cref="PropertyInfo"/>
        /// </summary>
        /// <param name="pi"><see cref="PropertyInfoExt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator PropertyInfo(in PropertyInfoExt pi) => pi.Property;
    }
}

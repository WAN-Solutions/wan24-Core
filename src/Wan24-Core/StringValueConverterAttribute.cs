﻿using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Attribute for a property which has a value that supports display string conversion
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class StringValueConverterAttribute() : Attribute()
    {
        /// <summary>
        /// Converter name (see <see cref="StringValueConverter"/>)
        /// </summary>
        public string? Converter { get; set; }

        /// <summary>
        /// Convert a property value to a display string
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="obj">Object</param>
        /// <returns>Display string</returns>
        public virtual string? Convert(in PropertyInfoExt pi, in object? obj)
        {
            if (pi.Getter is null) throw new ArgumentException("Property has no getter", nameof(pi));
            object? value = pi.GetValueFast(pi.Property.GetMethod!.IsStatic ? null : obj);
            return Converter is null
                ? StringValueConverter.Convert(value?.GetType() ?? pi.PropertyType, value)
                : StringValueConverter.NamedStringConversion(value, Converter, value?.GetType() ?? pi.PropertyType);
        }

        /// <summary>
        /// Convert a value to a display string
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="pi">Property</param>
        /// <returns>Display string</returns>
        public virtual string? Convert(in object? value, in PropertyInfo pi)
            => Converter is null
                ? StringValueConverter.Convert(value?.GetType() ?? pi.PropertyType, value)
                : StringValueConverter.NamedStringConversion(value, Converter, value?.GetType() ?? pi.PropertyType);

        /// <summary>
        /// Convert a display string to a property value
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="pi">Property</param>
        /// <returns>Object</returns>
        public virtual object? Convert(in string? str, in PropertyInfo pi)
            => Converter is null
                ? StringValueConverter.Convert(pi.PropertyType, str)
                : StringValueConverter.NamedObjectConversion(str, Converter, pi.PropertyType);
    }
}

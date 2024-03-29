﻿using System.Reflection;
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
        /// Setter
        /// </summary>
        public Action<object?, object?>? Setter { get; } = Setter;

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
        /// Cast as <see cref="PropertyInfo"/>
        /// </summary>
        /// <param name="pi"><see cref="PropertyInfoExt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator PropertyInfo(in PropertyInfoExt pi) => pi.Property;
    }
}

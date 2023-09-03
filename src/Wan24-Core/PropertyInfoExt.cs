using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Property informations
    /// </summary>
    public sealed class PropertyInfoExt : ICustomAttributeProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="getter">Getter</param>
        /// <param name="setter">Setter</param>
        public PropertyInfoExt(in PropertyInfo pi, in Func<object?, object?>? getter, in Action<object?, object?>? setter)
        {
            Property = pi;
            Getter = getter;
            Setter = setter;
        }

        /// <summary>
        /// Property
        /// </summary>
        public PropertyInfo Property { get; }

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
        public Func<object?, object?>? Getter { get; }

        /// <summary>
        /// Can read?
        /// </summary>
        public bool CanRead => Getter is not null;

        /// <summary>
        /// Setter
        /// </summary>
        public Action<object?, object?>? Setter { get; }

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
        public static implicit operator PropertyInfo(in PropertyInfoExt pi) => pi.Property;
    }
}

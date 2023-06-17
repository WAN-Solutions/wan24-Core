using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Property informations
    /// </summary>
    public sealed class PropertyInfoExt
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="getter">Getter</param>
        /// <param name="setter">Setter</param>
        public PropertyInfoExt(PropertyInfo pi, Func<object?, object?>? getter, Action<object?, object?>? setter)
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
        /// Getter
        /// </summary>
        public Func<object?, object?>? Getter { get; }

        /// <summary>
        /// Setter
        /// </summary>
        public Action<object?, object?>? Setter { get; }

        /// <summary>
        /// Cast as <see cref="PropertyInfo"/>
        /// </summary>
        /// <param name="pi"><see cref="PropertyInfoExt"/></param>
        public static implicit operator PropertyInfo(PropertyInfoExt pi) => pi.Property;
    }
}

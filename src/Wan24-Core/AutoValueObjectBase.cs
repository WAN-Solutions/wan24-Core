using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Base class for an automatic value object (uses properties with a public getter for building the objects hash code)
    /// </summary>
    /// <typeparam name="T">Final type (should be immutable using private property setters (for serialization purposes))</typeparam>
    public abstract class AutoValueObjectBase<T> : ValueObjectBase<T> where T : AutoValueObjectBase<T>
    {
        /// <summary>
        /// Property infos
        /// </summary>
        private static readonly PropertyInfoExt[] PropertyInfos = typeof(T).GetPropertiesCached(BindingFlags.Public | BindingFlags.Instance);

        /// <summary>
        /// Constructor
        /// </summary>
        protected AutoValueObjectBase() : base() { }

        /// <inheritdoc/>
        protected sealed override IEnumerable<object?> EqualsObjects()
            => from pi in PropertyInfos
               where pi.Getter is not null
               select pi.GetValueFast(this);
    }
}

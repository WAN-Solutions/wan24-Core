using System.Collections.ObjectModel;
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
        /// Value property infos
        /// </summary>
        internal static readonly ReadOnlyCollection<PropertyInfoExt> ValuePropertyInfos =
            (from pi in typeof(T).GetPropertiesCached(BindingFlags.Public | BindingFlags.Instance)
             where (pi.Property.GetMethod?.IsPublic ?? false) &&
                pi.GetCustomAttributeCached<ExcludeValueAttribute>() is null
             orderby pi.Name
             select pi)
            .AsReadOnly();

        /// <summary>
        /// Include property names in the objects hash code calculation?
        /// </summary>
        protected readonly bool IncludePropertyNames;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="includePropertyNames">Include property names in the objects hash code calculation?</param>
        protected AutoValueObjectBase(bool includePropertyNames = true) : base() => IncludePropertyNames = includePropertyNames;

        /// <inheritdoc/>
        protected sealed override IEnumerable<object?> EqualsObjects()
        {
            foreach (PropertyInfoExt pi in ValuePropertyInfos)
            {
                if (IncludePropertyNames) yield return pi.Name;
                yield return pi.GetValueFast(this);
            }
        }
    }
}

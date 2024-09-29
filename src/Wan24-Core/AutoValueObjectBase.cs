using System.Collections.Immutable;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Base class for an automatic value object (uses properties with a public getter for building the objects hash code)
    /// </summary>
    /// <typeparam name="T">Final type (should be immutable using private property setters (for serialization purposes))</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="includePropertyNames">Include property names in the objects hash code calculation?</param>
    public abstract class AutoValueObjectBase<T>(in bool includePropertyNames = true) : ValueObjectBase<T>() where T : AutoValueObjectBase<T>
    {
        /// <summary>
        /// Value property information
        /// </summary>
        internal static readonly ImmutableArray<PropertyInfoExt> ValuePropertyInfos =
            (from pi in typeof(T).GetPropertiesCached(BindingFlags.Public | BindingFlags.Instance)
             where pi.HasPublicGetter &&
                pi.Getter is not null &&
                pi.GetCustomAttributeCached<ExcludeValueAttribute>() is null
             orderby pi.Name
             select pi)
            .AsReadOnly();

        /// <summary>
        /// Include property names in the objects hash code calculation?
        /// </summary>
        protected readonly bool IncludePropertyNames = includePropertyNames;

        /// <inheritdoc/>
        protected sealed override IEnumerable<object?> EqualsObjects()
        {
            foreach (PropertyInfoExt pi in ValuePropertyInfos)
            {
                if (IncludePropertyNames) yield return pi.Name;
                yield return pi.Getter!(this);
            }
        }
    }
}

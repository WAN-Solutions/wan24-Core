using System.Collections;

namespace wan24.Core
{
    /// <summary>
    /// Attribute for properties to include in an <see cref="ObjectMapping"/> (or for a source type which uses opt-in auto-mapping (see <see cref="OptIn"/>) or has a custom target instance 
    /// factory (see <see cref="UseTargetInstanceFactory"/> and <see cref="TargetInstanceFactory(Type, Type)"/>))
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class MapAttribute() : Attribute(), IOptInOut
    {
        /// <summary>
        /// If to require a <see cref="MapAttribute"/> for properties to map
        /// </summary>
        public bool OptIn { get; init; } = true;

        /// <inheritdoc/>
        public OptInOut Opt => OptIn ? OptInOut.OptIn : OptInOut.OptOut;

        /// <summary>
        /// If to allow source properties having a public getter only (when applied to a source type for auto-mapping)
        /// </summary>
        public bool PublicGetterOnly { get; init; }

        /// <summary>
        /// If to allow target properties having a public setter only (when applied to a source type for auto-mapping)
        /// </summary>
        public bool PublicSetterOnly { get; init; }

        /// <summary>
        /// Target object property name
        /// </summary>
        public string? TargetPropertyName { get; init; }

        /// <summary>
        /// If nested object mapping should be applied (the source value will be mapped to the incompatible target value type)
        /// </summary>
        public bool Nested { get; init; }

        /// <summary>
        /// If to clone the source value (see also <see cref="CloneKeys"/> and <see cref="CloneItems"/>; has no effect, if <see cref="Nested"/> is <see langword="true"/>, or 
        /// <see cref="Map{tSource, tTarget}(string, tSource, tTarget)"/> or <see cref="MapAsync{tSource, tTarget}(string, tSource, tTarget, CancellationToken)"/> was implemented 
        /// (see <see cref="CanMap"/> and <see cref="CanMapAsync"/>))
        /// </summary>
        public bool CloneSourceValue { get; init; }

        /// <summary>
        /// If the source value should be cloned as target value (see <see cref="CloneSourceValue"/>)
        /// </summary>
        public bool ShouldCloneSourceValue => CloneSourceValue && !Nested && !CanMap && !CanMapAsync;

        /// <summary>
        /// If not only to apply <see cref="CloneSourceValue"/>, but also clone each items key (of an <see cref="IDictionary"/>; needs <see cref="CloneSourceValue"/> to be 
        /// <see langword="true"/>)
        /// </summary>
        public bool CloneKeys { get; init; }

        /// <summary>
        /// If not only to apply <see cref="CloneSourceValue"/>, but also clone each item (of an <see cref="IList"/>; needs <see cref="CloneSourceValue"/> to be <see langword="true"/>)
        /// </summary>
        public bool CloneItems { get; init; }

        /// <summary>
        /// If to apply any value converter (see <see cref="ValueConverterAttribute"/>; has no effect, if <see cref="Nested"/> or <see cref="CloneSourceValue"/> is <see langword="true"/>, or 
        /// <see cref="Map{tSource, tTarget}(string, tSource, tTarget)"/> or <see cref="MapAsync{tSource, tTarget}(string, tSource, tTarget, CancellationToken)"/> was implemented 
        /// (see <see cref="CanMap"/> and <see cref="CanMapAsync"/>))
        /// </summary>
        public bool ApplyValueConverter { get; init; }

        /// <summary>
        /// Apply an <see cref="EnumMapping"/> (has no effect, if <see cref="ApplyValueConverter"/>, <see cref="Nested"/> or <see cref="CloneSourceValue"/> is <see langword="true"/>, or 
        /// <see cref="Map{tSource, tTarget}(string, tSource, tTarget)"/> or <see cref="MapAsync{tSource, tTarget}(string, tSource, tTarget, CancellationToken)"/> was implemented 
        /// (see <see cref="CanMap"/> and <see cref="CanMapAsync"/>); a mapping has to be defined in <see cref="EnumMapping{tSource, tTarget}"/>, first!)
        /// </summary>
        public bool ApplyEnumMapping { get; init; }

        /// <summary>
        /// If this attribute can map a property using <see cref="Map{tSource, tTarget}(string, tSource, tTarget)"/>
        /// </summary>
        public virtual bool CanMap { get; }

        /// <summary>
        /// If this attribute can map a property using <see cref="MapAsync{tSource, tTarget}(string, tSource, tTarget, CancellationToken)"/>
        /// </summary>
        public virtual bool CanMapAsync { get; }

        /// <summary>
        /// If this attribute implements <see cref="MappingCondition{tSource, tTarget}(string, tSource, tTarget)"/>
        /// </summary>
        public virtual bool HasMappingCondition { get; }

        /// <summary>
        /// If <see cref="TargetInstanceFactory(Type, Type)"/> should be used
        /// </summary>
        public virtual bool UseTargetInstanceFactory { get; init; }

        /// <summary>
        /// If <see cref="ObjectValidator(object)"/> should be used during/after mapping
        /// </summary>
        public virtual bool UseObjectValidator { get; init; }

        /// <summary>
        /// Map a property (if you override this method, override <see cref="CanMap"/>, too, and return <see langword="true"/>)
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        public virtual void Map<tSource, tTarget>(string sourcePropertyName, tSource source, tTarget target)
            => throw new NotImplementedException();

        /// <summary>
        /// Map a property (if you override this method, override <see cref="CanMapAsync"/>, too, and return <see langword="true"/>)
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual Task MapAsync<tSource, tTarget>(string sourcePropertyName, tSource source, tTarget target, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        /// <summary>
        /// Mapping condition
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="mapping">Mapping name</param>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <returns>If the mapping should be applied for the given source and target object</returns>
        public virtual bool MappingCondition<tSource, tTarget>(string mapping, tSource source, tTarget target) => throw new NotImplementedException();

        /// <summary>
        /// Target instance factory (uses <see cref="ObjectMapping.DefaultTargetInstanceCreator(Type, Type)"/> per default)
        /// </summary>
        /// <param name="targetType">Target type</param>
        /// <param name="requestedType">Requested type</param>
        /// <returns>Instance</returns>
        public virtual object TargetInstanceFactory(Type targetType, Type requestedType) => ObjectMapping.DefaultTargetInstanceCreator(targetType, requestedType);

        /// <summary>
        /// Object validator (applied during and after mapping; uses <see cref="ObjectMapping.DefaultObjectInstanceValidator(object)"/> per default)
        /// </summary>
        /// <param name="value">Value to validate</param>
        public virtual void ObjectValidator(object value) => ObjectMapping.DefaultObjectInstanceValidator(value);
    }
}

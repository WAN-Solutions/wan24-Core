namespace wan24.Core
{
    /// <summary>
    /// Attribute for properties to include in an <see cref="ObjectMapping"/> (or for a source type which uses opt-in auto-mapping)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class MapAttribute() : Attribute()
    {
        /// <summary>
        /// Target object property name
        /// </summary>
        public string? TargetPropertyName { get; init; }

        /// <summary>
        /// If nested object mapping should be applied (the source value will be mapped to the incompatible target value type)
        /// </summary>
        public bool Nested { get; init; }

        /// <summary>
        /// If to require a <see cref="MapAttribute"/> for properties to map
        /// </summary>
        public bool OptIn { get; init; } = true;

        /// <summary>
        /// If to allow source properties having a public getter only (when applied to a source type for auto-mapping)
        /// </summary>
        public bool PublicGetterOnly { get; init; }

        /// <summary>
        /// If to allow target properties having a public setter only (when applied to a source type for auto-mapping)
        /// </summary>
        public bool PublicSetterOnly { get; init; }

        /// <summary>
        /// If this attribute can map a property using <see cref="Map{tSource, tTarget}(string, tSource, tTarget)"/>
        /// </summary>
        public virtual bool CanMap { get; }

        /// <summary>
        /// If this attribute can map a property using <see cref="MapAsync{tSource, tTarget}(string, tSource, tTarget, CancellationToken)"/>
        /// </summary>
        public virtual bool CanMapAsync { get; }

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
    }
}

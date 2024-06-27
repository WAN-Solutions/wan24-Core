using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Object mapping
    /// </summary>
    public partial class ObjectMapping
    {
        /// <summary>
        /// Mappings
        /// </summary>
        protected readonly OrderedDictionary<string, object> Mappings = [];

        /// <summary>
        /// Constructor
        /// </summary>
        internal ObjectMapping() { }

        /// <summary>
        /// Source object type
        /// </summary>
        public required Type SourceType { get; init; }

        /// <summary>
        /// Target object type
        /// </summary>
        public required Type TargetType { get; init; }

        /// <summary>
        /// Target object instance factory
        /// </summary>
        public TargetInstanceFactoryDelegate? TargetInstanceFactory { get; set; }

        /// <summary>
        /// Map a source object property value to the target object property having the same name
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <returns>This</returns>
        public ObjectMapping AddMapping(in string sourcePropertyName)
        {
            if (SourceType.GetPropertyCached(sourcePropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is not PropertyInfoExt pi)
                throw new ArgumentException("Source property not found", nameof(sourcePropertyName));
            if (pi.GetCustomAttributeCached<MapAttribute>() is MapAttribute attr)
            {
                if (attr.CanMap)
                {
                    ObjectMapper_Delegate mapper = (source, target) => MapMethod.MakeGenericMethod(source.GetType(), target.GetType()).Invoke(attr, [source, target]);
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new InvalidOperationException("A mapping for the given source property name exists already");
                    return this;
                }
                else if (attr.CanMapAsync)
                {
                    AsyncObjectMapper_Delegate mapper = async (source, target, ct) => await ((Task)AsyncMapMethod.MakeGenericMethod(source.GetType(), target.GetType()).Invoke(attr, [source, target, ct])!).DynamicContext();
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new InvalidOperationException("A mapping for the given source property name exists already");
                    return this;
                }
                if (attr.TargetPropertyName is not null)
                    return AddMapping(sourcePropertyName, attr.TargetPropertyName);
            }
            return AddMapping(sourcePropertyName, sourcePropertyName);
        }

        /// <summary>
        /// Map a source object property value to the given target object property
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="targetPropertyName">Target property name</param>
        /// <returns>This</returns>
        public ObjectMapping AddMapping(in string sourcePropertyName, in string targetPropertyName)
        {
            PropertyInfoExt sp = SourceType.GetPropertyCached(sourcePropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?? throw new ArgumentException("Source property not found", nameof(sourcePropertyName)),
                tp = TargetType.GetPropertyCached(targetPropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?? throw new ArgumentException("Target property not found", nameof(targetPropertyName));
            MapAttribute? attr = sp.GetCustomAttributeCached<MapAttribute>();
            ObjectMapper_Delegate mapper = attr?.Nested ?? false
                ? (source, target) => tp.SetValueFast(target, sp.GetValueFast(source)?.MapObjectTo(tp.PropertyType))
                : (source, target) => tp.SetValueFast(target, sp.GetValueFast(source));
            if (!Mappings.TryAdd(sourcePropertyName, mapper))
                throw new InvalidOperationException("A mapping for the given source property name exists already");
            return this;
        }

        /// <summary>
        /// Map a source object property value to the target object using the given method
        /// </summary>
        /// <typeparam name="tSource">Source object type (must match <see cref="SourceType"/>)</typeparam>
        /// <typeparam name="tTarget">Target object type (must match <see cref="TargetType"/>)</typeparam>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="mapper">Mapper method</param>
        /// <returns>This</returns>
        public ObjectMapping AddMapping<tSource, tTarget>(in string sourcePropertyName, in Mapper_Delegate<tSource, tTarget> mapper)
        {
            if (typeof(tSource) != SourceType) throw new ArgumentException("Source object type mismatch", nameof(tSource));
            if (typeof(tTarget) != TargetType) throw new ArgumentException("Target object type mismatch", nameof(tTarget));
            if (!Mappings.TryAdd(sourcePropertyName, mapper))
                throw new InvalidOperationException("A mapping for the given source property name exists already");
            return this;
        }

        /// <summary>
        /// Map a source object property value to the target object property having the same name (prefer to use asynchronous mapping possibilities, if possible)
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <returns>This</returns>
        public ObjectMapping AddAsyncMapping(in string sourcePropertyName)
        {
            if (SourceType.GetPropertyCached(sourcePropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is not PropertyInfoExt pi)
                throw new ArgumentException("Source property not found", nameof(sourcePropertyName));
            if (pi.GetCustomAttributeCached<MapAttribute>() is MapAttribute attr)
            {
                if (attr.CanMapAsync)
                {
                    AsyncObjectMapper_Delegate mapper = async (source, target, ct) => await ((Task)AsyncMapMethod.MakeGenericMethod(source.GetType(), target.GetType()).Invoke(attr, [source, target, ct])!).DynamicContext();
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new InvalidOperationException("A mapping for the given source property name exists already");
                    return this;
                }
                else if (attr.CanMap)
                {
                    ObjectMapper_Delegate mapper = (source, target) => MapMethod.MakeGenericMethod(source.GetType(), target.GetType()).Invoke(attr, [source, target]);
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new InvalidOperationException("A mapping for the given source property name exists already");
                    return this;
                }
                if (attr.TargetPropertyName is not null)
                    return AddMapping(sourcePropertyName, attr.TargetPropertyName);
            }
            return AddMapping(sourcePropertyName, sourcePropertyName);
        }

        /// <summary>
        /// Map a source object property value to the given target object property (prefer to use asynchronous mapping possibilities, if possible)
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="targetPropertyName">Target property name</param>
        /// <returns>This</returns>
        public ObjectMapping AddAsyncMapping(in string sourcePropertyName, in string targetPropertyName)
        {
            PropertyInfoExt sp = SourceType.GetPropertyCached(sourcePropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?? throw new ArgumentException("Source property not found", nameof(sourcePropertyName)),
                tp = TargetType.GetPropertyCached(targetPropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?? throw new ArgumentException("Target property not found", nameof(targetPropertyName));
            MapAttribute? attr = sp.GetCustomAttributeCached<MapAttribute>();
            if (attr?.Nested ?? false)
            {
                AsyncObjectMapper_Delegate mapper = async (source, target, ct) => tp.SetValueFast(target, sp.GetValueFast(source) is object value ? await value.MapObjectToAsync(tp.PropertyType, ct).DynamicContext() : null);
                if (!Mappings.TryAdd(sourcePropertyName, mapper))
                    throw new InvalidOperationException("A mapping for the given source property name exists already");
            }
            else
            {
                ObjectMapper_Delegate mapper = (source, target) => tp.SetValueFast(target, sp.GetValueFast(source));
                if (!Mappings.TryAdd(sourcePropertyName, mapper))
                    throw new InvalidOperationException("A mapping for the given source property name exists already");
            }
            return this;
        }

        /// <summary>
        /// Map a source object property value to the target object using the given method
        /// </summary>
        /// <typeparam name="tSource">Source object type (must match <see cref="SourceType"/>)</typeparam>
        /// <typeparam name="tTarget">Target object type (must match <see cref="TargetType"/>)</typeparam>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="mapper">Mapper method</param>
        /// <returns>This</returns>
        public ObjectMapping AddAsyncMapping<tSource, tTarget>(in string sourcePropertyName, in AsyncMapper_Delegate<tSource, tTarget> mapper)
        {
            if (!Mappings.TryAdd(sourcePropertyName, mapper))
                throw new InvalidOperationException("A mapping for the given source property name exists already");
            return this;
        }

        /// <summary>
        /// Add automatic mappings from the types mapping configuration (all instance properties)
        /// </summary>
        /// <param name="optIn">If to use opt-in using the <see cref="MapAttribute"/> (if <see langword="null"/>, it'll be <see langword="true"/>, if 
        /// <see cref="SourceType"/> has a <see cref="MapAttribute"/>)</param>
        /// <param name="publicGetterOnly">If to allow source properties having a public getter only</param>
        /// <param name="publicSetterOnly">If to allow target properties having a public setter only</param>
        /// <returns>This</returns>
        public virtual ObjectMapping AddAutoMappings(bool? optIn = null, bool publicGetterOnly = false, bool publicSetterOnly = false)
        {
            optIn ??= SourceType.GetCustomAttribute<MapAttribute>() is not null;
            MapAttribute? attr;
            foreach (PropertyInfoExt pi in from pi in SourceType.GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                           where
                                            // Getter required
                                            pi.Property.GetMethod is not null &&
                                            // Not excluded
                                            pi.GetCustomAttributeCached<NoMapAttribute>() is null &&
                                            // Opt-in
                                            (!optIn.Value || pi.GetCustomAttributeCached<MapAttribute>() is not null) &&
                                            // Public getter required
                                            (!publicGetterOnly || pi.Property.GetMethod.IsPublic)
                                           select pi)
            {
                attr = pi.GetCustomAttributeCached<MapAttribute>();
                if (attr?.CanMap ?? false)
                {
                    AddMapping(pi.Name);
                }
                else if (attr?.TargetPropertyName is string targetPropertyName)
                {
                    if (TargetType.GetPropertyCached(targetPropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is not PropertyInfoExt tp)
                        throw new InvalidDataException($"{SourceType}.{pi.Name} mapped target property {TargetType}.{targetPropertyName} not found");
                    if (tp.Property.SetMethod is null || (publicSetterOnly && tp.Property.SetMethod.IsPublic))
                        throw new InvalidDataException($"{SourceType}.{pi.Name} mapped target property {TargetType}.{targetPropertyName} has no setter");
                    if (!tp.PropertyType.IsAssignableFrom(pi.PropertyType))
                        throw new InvalidDataException($"{SourceType}.{pi.Name} mapped target property {TargetType}.{targetPropertyName} value type is incompatible (manual mapping required)");
                    AddMapping(pi.Name, targetPropertyName);
                }
                else if (
                    TargetType.GetPropertyCached(pi.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is PropertyInfoExt tp &&
                    tp.Property.SetMethod is not null &&
                    (!publicSetterOnly || tp.Property.SetMethod.IsPublic) &&
                    tp.PropertyType.IsAssignableFrom(pi.PropertyType)
                    )
                {
                    AddMapping(pi.Name, pi.Name);
                }
                else if (attr is not null)
                {
                    throw new InvalidDataException($"Can't create maping for source property {SourceType}.{pi.Name} (target property is missing or has no (public) setter or its value type is incompatible)");
                }
            }
            return this;
        }

        /// <summary>
        /// Register this object mapping
        /// </summary>
        /// <returns>This</returns>
        public ObjectMapping Register()
        {
            Registered[(SourceType, TargetType)] = this;
            return this;
        }

        /// <summary>
        /// Apply mappings
        /// </summary>
        /// <typeparam name="tSource">Source object type (must match <see cref="SourceType"/>)</typeparam>
        /// <typeparam name="tTarget">Target object type (must match <see cref="TargetType"/>)</typeparam>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <returns>This</returns>
        public virtual ObjectMapping ApplyMappings<tSource, tTarget>(in tSource source, in tTarget target)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(target);
            foreach (object mapping in Mappings.Values)
                switch (mapping)
                {
                    case Mapper_Delegate<tSource, tTarget> mapper:
                        mapper(source, target);
                        break;
                    case AsyncMapper_Delegate<tSource, tTarget> mapper:
                        mapper(source, target, CancellationToken.None).GetAwaiter().GetResult();
                        break;
                    case ObjectMapper_Delegate mapper:
                        mapper(source, target);
                        break;
                    case AsyncObjectMapper_Delegate mapper:
                        mapper(source, target, CancellationToken.None).GetAwaiter().GetResult();
                        break;
                    default:
                        throw new InvalidDataException($"Invalid mapper type {mapping.GetType()}");
                }
            if (source is IMappingObject mappingObject)
                if (mappingObject.HasSyncHandlers)
                {
                    mappingObject.OnAfterMapping(target);
                }
                else if (mappingObject.HasAsyncHandlers && !mappingObject.HasSyncHandlers)
                {
                    mappingObject.OnAfterMappingAsync(target, CancellationToken.None).GetAwaiter().GetResult();
                }
            if (source is IMappingObject<tTarget> mappingObject2)
                if (mappingObject2.HasSyncHandlers)
                {
                    mappingObject2.OnAfterMapping(target);
                }
                else if (mappingObject2.HasAsyncHandlers && !mappingObject2.HasSyncHandlers)
                {
                    mappingObject2.OnAfterMappingAsync(target, CancellationToken.None).GetAwaiter().GetResult();
                }
            return this;
        }

        /// <summary>
        /// Apply mappings
        /// </summary>
        /// <param name="source">Source object (type must match <see cref="SourceType"/>)</param>
        /// <param name="target">Target object (type must match <see cref="TargetType"/>)</param>
        /// <returns>This</returns>
        public ObjectMapping ApplyObjectMappings(in object source, in object target)
        {
            if (!SourceType.IsAssignableFrom(source.GetType())) throw new ArgumentException("Incompatible type", nameof(source));
            if (!TargetType.IsAssignableFrom(target.GetType())) throw new ArgumentException("Incompatible type", nameof(target));
            ApplyMethod.MakeGenericMethod(SourceType, TargetType).Invoke(this, [source, target]);
            return this;
        }

        /// <summary>
        /// Apply mappings
        /// </summary>
        /// <typeparam name="tSource">Source object type (must match <see cref="SourceType"/>)</typeparam>
        /// <typeparam name="tTarget">Target object type (must match <see cref="TargetType"/>)</typeparam>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task ApplyMappingsAsync<tSource, tTarget>(tSource source, tTarget target, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(target);
            foreach (object mapping in Mappings.Values)
                switch (mapping)
                {
                    case Mapper_Delegate<tSource, tTarget> mapper:
                        mapper(source, target);
                        break;
                    case AsyncMapper_Delegate<tSource, tTarget> mapper:
                        await mapper(source, target, cancellationToken).DynamicContext();
                        break;
                    case ObjectMapper_Delegate mapper:
                        mapper(source, target);
                        break;
                    case AsyncObjectMapper_Delegate mapper:
                        await mapper(source, target, cancellationToken).DynamicContext();
                        break;
                    default:
                        throw new InvalidDataException($"Invalid mapper type {mapping.GetType()}");
                }
            if (source is IMappingObject mappingObject)
                if (mappingObject.HasAsyncHandlers)
                {
                    await mappingObject.OnAfterMappingAsync(target, cancellationToken).DynamicContext();
                }
                else if (mappingObject.HasSyncHandlers)
                {
                    mappingObject.OnAfterMapping(target);
                }
            if (source is IMappingObject<tTarget> mappingObject2)
                if (mappingObject2.HasAsyncHandlers)
                {
                    await mappingObject2.OnAfterMappingAsync(target, cancellationToken).DynamicContext();
                }
                else if (mappingObject2.HasSyncHandlers)
                {
                    mappingObject2.OnAfterMapping(target);
                }
        }

        /// <summary>
        /// Apply mappings
        /// </summary>
        /// <param name="source">Source object (type must match <see cref="SourceType"/>)</param>
        /// <param name="target">Target object (type must match <see cref="TargetType"/>)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task ApplyObjectMappingsAsync(in object source, in object target, in CancellationToken cancellationToken)
        {
            if (!SourceType.IsAssignableFrom(source.GetType())) throw new ArgumentException("Incompatible type", nameof(source));
            if (!TargetType.IsAssignableFrom(target.GetType())) throw new ArgumentException("Incompatible type", nameof(target));
            return (Task)AsyncApplyMethod.MakeGenericMethod(SourceType, TargetType).Invoke(this, [source, target, cancellationToken])!;
        }
    }
}

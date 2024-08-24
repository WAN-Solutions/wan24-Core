using System.Reflection;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Object mapping
    /// </summary>
    public partial class ObjectMapping
    {
        /// <summary>
        /// Mappable property reflection flags
        /// </summary>
        protected const BindingFlags PROPERTY_REFLECTION_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Mappings
        /// </summary>
        protected readonly OrderedDictionary<string, object> Mappings = [];
        /// <summary>
        /// Source object type
        /// </summary>
        protected readonly TypeInfoExt _SourceType = null!;

        /// <summary>
        /// Constructor
        /// </summary>
        internal ObjectMapping() { }

        /// <summary>
        /// Source object type
        /// </summary>
        public required TypeInfoExt SourceType
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _SourceType;
            init
            {
                _SourceType = value;
                SourceMappingOptions = value.Type.GetCustomAttributeCached<MapAttribute>();
            }
        }

        /// <summary>
        /// Target object type
        /// </summary>
        public required TypeInfoExt TargetType { get; init; }

        /// <summary>
        /// Source type mapping options
        /// </summary>
        public MapAttribute? SourceMappingOptions { get; protected init; }

        /// <summary>
        /// Target object instance factory
        /// </summary>
        public TargetInstanceFactoryDelegate? TargetInstanceFactory { get; set; }

        /// <summary>
        /// Map a source object property value to the target object property having the same name
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <returns>This</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ObjectMapping AddMapping(in string sourcePropertyName)
        {
            if (SourceType.Type.GetPropertyCached(sourcePropertyName, PROPERTY_REFLECTION_FLAGS) is not PropertyInfoExt pi)
                throw new MappingException($"Source property \"{SourceType.Type}.{sourcePropertyName}\" not found");
            if (pi.GetCustomAttributeCached<MapAttribute>() is MapAttribute attr)
            {
                if (attr.CanMap)
                {
                    ObjectMapper_Delegate mapper = (source, target) => MapMethod.Method.MakeGenericMethod(source.GetType(), target.GetType()).InvokeFast(attr, [source, target]);
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
                    return this;
                }
                if (attr.CanMapAsync)
                {
                    AsyncObjectMapper_Delegate mapper =
                        async (source, target, ct)
                            => await ((Task)AsyncMapMethod.Method.MakeGenericMethod(source.GetType(), target.GetType()).InvokeFast(attr, [source, target, ct])!).DynamicContext();
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ObjectMapping AddMapping(in string sourcePropertyName, in string targetPropertyName)
        {
            PropertyInfoExt sp = SourceType.Type.GetPropertyCached(sourcePropertyName, PROPERTY_REFLECTION_FLAGS)
                    ?? throw new MappingException($"Source property \"{SourceType.Type}.{sourcePropertyName}\" not found"),
                tp = TargetType.Type.GetPropertyCached(targetPropertyName, PROPERTY_REFLECTION_FLAGS)
                    ?? throw new MappingException($"Target property \"{TargetType.Type}.{targetPropertyName}\" not found");
            if (sp.Getter is null) throw new MappingException($"Source property \"{SourceType.Type}.{sourcePropertyName}\" has no usable getter");
            if (tp.Setter is null) throw new MappingException($"Target property \"{TargetType.Type}.{targetPropertyName}\" has no usable setter");
            if (!CanMapPropertyTo(sp, tp, out MapAttribute? attr))
                throw new MappingException($"{sp.DeclaringType}.{sp.Name} ({sp.PropertyType}) can't be mapped to {tp.DeclaringType}.{tp.Name} ({tp.PropertyType})");
            ObjectMapper_Delegate mapper = attr?.Nested ?? false
                ? (source, target) => tp.Setter(target, sp.Getter(source)?.MapObjectTo(tp.PropertyType))
                : (source, target) => tp.Setter(target, sp.Getter(source));
            if (!Mappings.TryAdd(sourcePropertyName, mapper))
                throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ObjectMapping AddMapping<tSource, tTarget>(in string sourcePropertyName, in Mapper_Delegate<tSource, tTarget> mapper)
        {
            if (typeof(tSource) != SourceType.Type) throw new MappingException($"Source object type from {nameof(tSource)} mismatch ({SourceType.Type} / {typeof(tSource)})");
            if (typeof(tTarget) != TargetType.Type) throw new MappingException($"Target object type from {nameof(tTarget)} mismatch ({TargetType.Type} / {typeof(tTarget)})");
            if (!Mappings.TryAdd(sourcePropertyName, mapper))
                throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
            return this;
        }

        /// <summary>
        /// Map a source object property value to the target object property having the same name (prefer to use asynchronous mapping possibilities, if possible)
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <returns>This</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ObjectMapping AddAsyncMapping(in string sourcePropertyName)
        {
            if (SourceType.Type.GetPropertyCached(sourcePropertyName, PROPERTY_REFLECTION_FLAGS) is not PropertyInfoExt pi)
                throw new MappingException($"Source property \"{SourceType.Type}.{sourcePropertyName}\" not found");
            if (pi.GetCustomAttributeCached<MapAttribute>() is MapAttribute attr)
            {
                if (attr.CanMapAsync)
                {
                    AsyncObjectMapper_Delegate mapper =
                        async (source, target, ct)
                            => await ((Task)AsyncMapMethod.Method.MakeGenericMethod(source.GetType(), target.GetType()).InvokeFast(attr, [source, target, ct])!).DynamicContext();
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
                    return this;
                }
                if (attr.CanMap)
                {
                    ObjectMapper_Delegate mapper = (source, target) => MapMethod.Method.MakeGenericMethod(source.GetType(), target.GetType()).InvokeFast(attr, [source, target]);
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ObjectMapping AddAsyncMapping(in string sourcePropertyName, in string targetPropertyName)
        {
            PropertyInfoExt sp = SourceType.Type.GetPropertyCached(sourcePropertyName, PROPERTY_REFLECTION_FLAGS)
                    ?? throw new MappingException($"Source property \"{SourceType.Type}.{sourcePropertyName}\" not found"),
                tp = TargetType.Type.GetPropertyCached(targetPropertyName, PROPERTY_REFLECTION_FLAGS)
                    ?? throw new MappingException($"Target property \"{TargetType.Type}.{targetPropertyName}\" not found");
            if (sp.Getter is null) throw new MappingException($"Source property \"{SourceType.Type}.{sourcePropertyName}\" has no usable getter");
            if (tp.Setter is null) throw new MappingException($"Target property \"{TargetType.Type}.{targetPropertyName}\" has no usable setter");
            if (!CanMapPropertyTo(sp, tp, out MapAttribute? attr))
                throw new MappingException($"{sp.DeclaringType}.{sp.Name} ({sp.PropertyType}) can't be mapped to {tp.DeclaringType}.{tp.Name} ({tp.PropertyType})");
            if (attr?.Nested ?? false)
            {
                AsyncObjectMapper_Delegate mapper =
                    async (source, target, ct)
                        => tp.Setter(target, sp.Getter(source) is object value ? await value.MapObjectToAsync(tp.PropertyType, ct).DynamicContext() : null);
                if (!Mappings.TryAdd(sourcePropertyName, mapper))
                    throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
            }
            else
            {
                ObjectMapper_Delegate mapper = (source, target) => tp.Setter(target, sp.Getter(source));
                if (!Mappings.TryAdd(sourcePropertyName, mapper))
                    throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ObjectMapping AddAsyncMapping<tSource, tTarget>(in string sourcePropertyName, in AsyncMapper_Delegate<tSource, tTarget> mapper)
        {
            if (!Mappings.TryAdd(sourcePropertyName, mapper))
                throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
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
        public virtual ObjectMapping AddAutoMappings(bool? optIn = null, bool? publicGetterOnly = null, bool? publicSetterOnly = null)
        {
            MapAttribute? attr = SourceMappingOptions;
            optIn ??= attr?.OptIn ?? false;
            publicGetterOnly ??= attr?.PublicGetterOnly ?? false;
            publicSetterOnly ??= attr?.PublicSetterOnly ?? false;
            string targetPropertyName;
            PropertyInfoExt? tp;
            foreach (PropertyInfoExt pi in from pi in SourceType.Type.GetPropertiesCached(PROPERTY_REFLECTION_FLAGS)
                                           where
                                            // Getter exists
                                            pi.Getter is not null &&
                                            pi.Property.GetMethod is not null && 
                                            // Not excluded
                                            pi.GetCustomAttributeCached<NoMapAttribute>() is null &&
                                            // Opt-in
                                            (pi.GetCustomAttributeCached<MapAttribute>()?.OptIn ?? !optIn.Value) &&
                                            // Public getter exists
                                            (!publicGetterOnly.Value || pi.HasPublicGetter)
                                           select pi)
            {
                attr = pi.GetCustomAttributeCached<MapAttribute>();
                targetPropertyName = attr?.TargetPropertyName ?? pi.Name;
                tp = TargetType.Type.GetPropertyCached(targetPropertyName, PROPERTY_REFLECTION_FLAGS);
                if (tp is null || !CanMapTypeTo(pi.PropertyType, tp.Property, attr) || (publicSetterOnly.Value && !tp.HasPublicSetter))
                {
                    if (attr is not null)
                        throw new MappingException(
                            $"Invalid mapping configuration for mapping source object property {SourceType.Type}.{pi.Name} to target object property {TargetType.Type}.{targetPropertyName}"
                            );
                }
                else if(attr?.TargetPropertyName is null)
                {
                    AddMapping(pi.Name);
                }
                else
                {
                    AddMapping(pi.Name, targetPropertyName);
                }
            }
            return this;
        }

        /// <summary>
        /// Register this object mapping
        /// </summary>
        /// <param name="targetType">Different target object type which is compatible with <see cref="TargetType"/></param>
        /// <returns>This</returns>
        public ObjectMapping Register(in Type? targetType = null)
        {
            if (targetType is not null && !TargetType.Type.IsAssignableFrom(targetType))
                throw new MappingException($"Invalid target object type ({targetType} is not assignable to {TargetType.Type})");
            RegisteredMappings[(SourceType.Type, targetType ?? TargetType.Type)] = this;
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
            for(int i = 0, len = Mappings.Count; i < len; i++)
                switch (Mappings[i])
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
                        throw new MappingException($"Invalid mapper type {Mappings[i].GetType()} in object mapping configuration for mapping {SourceType.Type} to {TargetType.Type}");
                }
            if (source is IMappingObject mappingObject)
                if (mappingObject.HasSyncHandlers)
                {
                    mappingObject.OnAfterMapping(target);
                }
                else if (mappingObject.HasAsyncHandlers)
                {
                    mappingObject.OnAfterMappingAsync(target, CancellationToken.None).GetAwaiter().GetResult();
                }
            if (source is IMappingObject<tTarget> mappingObject2)
                if (mappingObject2.HasSyncHandlers)
                {
                    mappingObject2.OnAfterMapping(target);
                }
                else if (mappingObject2.HasAsyncHandlers)
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
            if (!SourceType.Type.IsAssignableFrom(source.GetType())) throw new ArgumentException("Incompatible type", nameof(source));
            if (!TargetType.Type.IsAssignableFrom(target.GetType())) throw new ArgumentException("Incompatible type", nameof(target));
            ApplyMethod.MakeGenericMethod(SourceType.Type, TargetType.Type).Invoker!(this, [source, target]);
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
            for (int i = 0, len = Mappings.Count; i < len; i++)
                switch (Mappings[i])
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
                        throw new MappingException($"Invalid mapper type {Mappings[i].GetType()} in object mapping configuration for mapping {SourceType.Type} to {TargetType.Type}");
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
            if (!SourceType.Type.IsAssignableFrom(source.GetType())) throw new ArgumentException("Incompatible type", nameof(source));
            if (!TargetType.Type.IsAssignableFrom(target.GetType())) throw new ArgumentException("Incompatible type", nameof(target));
            return (Task)AsyncApplyMethod.MakeGenericMethod(SourceType.Type, TargetType.Type).Invoker!(this, [source, target, cancellationToken])!;
        }
    }
}

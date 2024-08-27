using System.Runtime.CompilerServices;

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
        /// Source object type
        /// </summary>
        protected readonly TypeInfoExt _SourceType = null!;
        /// <summary>
        /// If the generic child type has a compiled mapping
        /// </summary>
        protected bool? _HasCompiledMapping = null;
        /// <summary>
        /// Compiled mapping
        /// </summary>
        protected object? _CompiledMapping = null;

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
        public required TypeInfoExt TargetType
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get;
            init;
        }

        /// <summary>
        /// Source type mapping options
        /// </summary>
        public MapAttribute? SourceMappingOptions
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get;
            protected init;
        }

        /// <summary>
        /// Target object instance factory
        /// </summary>
        public TargetInstanceFactoryDelegate? TargetInstanceFactory
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get;
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set;
        }

        /// <summary>
        /// If the generic object mapping has a compiled mapping (see <see cref="ObjectMapping{tSource, tTarget}.CompiledMapping"/> and 
        /// <see cref="ObjectMapping{tSource, tTarget}.CompileMapping"/>)
        /// </summary>
        public bool HasCompiledMapping
        {
            get
            {
                if (!_HasCompiledMapping.HasValue)
                {
                    PropertyInfoExt pi = typeof(ObjectMapping<,>).MakeGenericType(SourceType, TargetType).GetPropertyCached("CompiledMapping")
                        ?? throw new InvalidProgramException();
                    if (pi.Getter is null) throw new InvalidProgramException();
                    _HasCompiledMapping = pi.Getter(this) is not null;
                }
                return _HasCompiledMapping.Value;
            }
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
            if (GetCompiledMapping<tSource, tTarget>() is Action<tSource, tTarget> compiledMapping)
            {
                compiledMapping(source, target);
                return this;
            }
            for (int i = 0, len = Mappings.Count; i < len; i++)
                switch (Mappings[i])
                {
                    case ObjectMapper_Delegate mapper:
                        mapper(source, target);
                        break;
                    case AsyncObjectMapper_Delegate mapper:
                        mapper(source, target, CancellationToken.None).GetAwaiter().GetResult();
                        break;
                    case Mapper_Delegate<tSource, tTarget> mapper:
                        mapper(source, target);
                        break;
                    case AsyncMapper_Delegate<tSource, tTarget> mapper:
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
                    case AsyncObjectMapper_Delegate mapper:
                        await mapper(source, target, cancellationToken).DynamicContext();
                        break;
                    case ObjectMapper_Delegate mapper:
                        mapper(source, target);
                        break;
                    case AsyncMapper_Delegate<tSource, tTarget> mapper:
                        await mapper(source, target, cancellationToken).DynamicContext();
                        break;
                    case Mapper_Delegate<tSource, tTarget> mapper:
                        mapper(source, target);
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public Task ApplyObjectMappingsAsync(in object source, in object target, in CancellationToken cancellationToken)
        {
            if (!SourceType.Type.IsAssignableFrom(source.GetType())) throw new ArgumentException("Incompatible type", nameof(source));
            if (!TargetType.Type.IsAssignableFrom(target.GetType())) throw new ArgumentException("Incompatible type", nameof(target));
            return (Task)AsyncApplyMethod.MakeGenericMethod(SourceType.Type, TargetType.Type).Invoker!(this, [source, target, cancellationToken])!;
        }

        /// <summary>
        /// Get the compiled mapping from the generic child type
        /// </summary>
        /// <typeparam name="tSource">Source object type (must match <see cref="SourceType"/>)</typeparam>
        /// <typeparam name="tTarget">Target object type (must match <see cref="TargetType"/>)</typeparam>
        /// <returns>Compiled mapping</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected virtual Action<tSource, tTarget>? GetCompiledMapping<tSource, tTarget>()
        {
            if (_CompiledMapping is null)
            {
                if (_HasCompiledMapping.HasValue && !_HasCompiledMapping.Value) return null;
                PropertyInfoExt pi = typeof(ObjectMapping<,>).MakeGenericType(SourceType, TargetType).GetPropertyCached("CompiledMapping")
                    ?? throw new InvalidProgramException();
                if (pi.Getter is null) throw new InvalidProgramException();
                _CompiledMapping = pi.Getter(this) as Action<tSource, tTarget>;
                _HasCompiledMapping ??= _CompiledMapping is not null;
            }
            return (Action<tSource, tTarget>?)_CompiledMapping;
        }
    }
}

using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Object mapping
    /// </summary>
    public abstract partial class ObjectMapping
    {
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
        /// If the <see cref="SourceType"/> implements <see cref="IMappingObject"/>
        /// </summary>
        public bool IsMappingObject
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get
            {
                _IsMappingObject ??= typeof(IMappingObject).IsAssignableFrom(SourceType);
                return _IsMappingObject.Value;
            }
        }

        /// <summary>
        /// If the <see cref="SourceType"/> implements <see cref="IMappingObject{T}"/> with the <see cref="TargetType"/> as generic argument
        /// </summary>
        public bool IsMappingObjectExt
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get
            {
                _IsMappingObjectExt ??= typeof(IMappingObject<>).MakeGenericType(TargetType).IsAssignableFrom(SourceType);
                return _IsMappingObjectExt.Value;
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
        } = DefaultTargetInstanceFactory;

        /// <summary>
        /// Object validator (applied during and after mapping)
        /// </summary>
        public Validate_Delegate? ObjectValidator
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get;
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set;
        } = DefaultObjectValidator;

        /// <summary>
        /// If to create a compiled mapping automatic when mappings are being applied
        /// </summary>
        public bool AutoCompile { get; init; } = DefaultAutoCompile;

        /// <summary>
        /// If the generic object mapping has a compiled mapping (see <see cref="ObjectMapping{tSource, tTarget}.CompiledMapping"/> and 
        /// <see cref="ObjectMapping{tSource, tTarget}.CompileMapping"/>)
        /// </summary>
        public bool HasCompiledMapping { get; protected set; }

        /// <summary>
        /// If there's any asynchronous mapping
        /// </summary>
        public bool HasAsyncMapping => Mappings.Values.Any(m => m.Type == MapperType.AnyAsync);

        /// <summary>
        /// If there's any expression mapping
        /// </summary>
        public bool HasExpressionMapping => Mappings.Values.Any(m => m.Type == MapperType.Expression || m.Type == MapperType.GenericExpression);

        /// <summary>
        /// If mappings can be applied asynchronous
        /// </summary>
        public bool CanMapAsync => !HasExpressionMapping;

        /// <summary>
        /// If mappings should be applied asynchronous
        /// </summary>
        public bool ShouldMapAsync => HasAsyncMapping && CanMapAsync;

        /// <summary>
        /// Create a compiled mapping (see <see cref="ObjectMapping{tSource, tTarget}.CompileMapping"/>)
        /// </summary>
        /// <returns>This</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public virtual ObjectMapping CreateCompiledMapping()
        {
            CompileMappingMethod.Invoker!(this, []);
            return this;
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
        public virtual Action<tSource, tTarget>? GetCompiledMapping<tSource, tTarget>()
        {
            if (!SourceType.Type.IsAssignableFrom(typeof(tSource))) throw new ArgumentException("Incompatible type", nameof(tSource));
            if (!TargetType.Type.IsAssignableFrom(typeof(tTarget))) throw new ArgumentException("Incompatible type", nameof(tTarget));
            if (CompiledObjectMapping is null && AutoCompile) CreateCompiledMapping();
            return (Action<tSource, tTarget>?)CompiledObjectMapping;
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
    }
}

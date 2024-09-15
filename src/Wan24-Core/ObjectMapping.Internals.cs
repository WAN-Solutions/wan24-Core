using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Internals
    public partial class ObjectMapping
    {
        /// <summary>
        /// <see cref="ObjectMapping{tSource, tTarget}.CompiledMapping"/> property name
        /// </summary>
        protected const string COMPILED_MAPPING_PROPERTY_NAME = "CompiledMapping";
        /// <summary>
        /// <see cref="ObjectMapping{tSource, tTarget}.CompileMapping"/> method name
        /// </summary>
        protected const string COMPILE_MAPPING_METHOD_NAME = "CompileMapping";

        /// <summary>
        /// <see cref="ApplyMappings{tSource, tTarget}(in tSource, in tTarget)"/> method
        /// </summary>
        protected static readonly MethodInfoExt ApplyMethod;
        /// <summary>
        /// <see cref="ApplyMappingsAsync{tSource, tTarget}(tSource, tTarget, CancellationToken)"/> method
        /// </summary>
        protected static readonly MethodInfoExt AsyncApplyMethod;
        /// <summary>
        /// Registered object mappings
        /// </summary>
        protected static readonly ConcurrentDictionary<(Type SourceType, Type TargetType), ObjectMapping> RegisteredMappings = [];
        /// <summary>
        /// <see cref="CreateSourceValueClone{T}(in T, in MapAttribute?)"/> method
        /// </summary>
        protected static readonly MethodInfoExt CreateCloneMethod;
        /// <summary>
        /// <see cref="CreateSourceValueCloneAsync{T}(T, MapAttribute?, CancellationToken)"/> method
        /// </summary>
        protected static readonly MethodInfoExt CreateCloneAsyncMethod;
        /// <summary>
        /// <see cref="FinalizeSourceValueClone{T}(in T, in T, in MapAttribute)"/> method
        /// </summary>
        protected static readonly MethodInfoExt FinalizeCloneMethod;
        /// <summary>
        /// <see cref="FinalizeSourceValueCloneAsync{T}(T, T, MapAttribute, CancellationToken)"/> method
        /// </summary>
        protected static readonly MethodInfoExt FinalizeCloneAsyncMethod;

        /// <summary>
        /// Mappings
        /// </summary>
        protected readonly FreezableOrderedDictionary<string, MapperInfo> Mappings = [];
        /// <summary>
        /// Source object type
        /// </summary>
        protected readonly TypeInfoExt _SourceType = null!;
        /// <summary>
        /// Target object type
        /// </summary>
        protected readonly TypeInfoExt _TargetType = null!;
        /// <summary>
        /// Compiled mapping (a copy of <see cref="ObjectMapping{tSource, tTarget}.CompiledMapping"/>)
        /// </summary>
        protected object? CompiledObjectMapping = null;
        /// <summary>
        /// <see cref="ObjectMapping{tSource, tTarget}"/> type
        /// </summary>
        protected Type? _GenericObjectMappingType = null;
        /// <summary>
        /// <see cref="ObjectMapping{tSource, tTarget}.CompileMapping"/> method
        /// </summary>
        protected MethodInfoExt? _CompileMappingMethod = null;
        /// <summary>
        /// If the source type implements <see cref="IMappingObject"/>
        /// </summary>
        protected bool? _IsMappingObject = null;
        /// <summary>
        /// If the source type implements <see cref="IMappingObject{T}"/> (with the <see cref="TargetType"/> as generic argument)
        /// </summary>
        protected bool? _IsMappingObjectExt = null;

        /// <summary>
        /// Static constructor
        /// </summary>
        static ObjectMapping()
        {
            ApplyMethod = typeof(ObjectMapping).GetMethodCached(nameof(ApplyMappings), BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidProgramException();
            AsyncApplyMethod = typeof(ObjectMapping).GetMethodCached(nameof(ApplyMappingsAsync), BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidProgramException();
            CreateCloneMethod = typeof(ObjectMapping)
                .GetMethodsCached()
                .FirstOrDefault(m => m.Name == nameof(CreateSourceValueClone) && m.IsGenericMethodDefinition)
                ?? throw new InvalidProgramException();
            CreateCloneAsyncMethod = typeof(ObjectMapping)
                .GetMethodsCached()
                .FirstOrDefault(m => m.Name == nameof(CreateSourceValueCloneAsync) && m.IsGenericMethodDefinition)
                ?? throw new InvalidProgramException();
            FinalizeCloneMethod = typeof(ObjectMapping)
                .GetMethodsCached()
                .FirstOrDefault(m => m.Name == nameof(FinalizeSourceValueClone) && m.IsGenericMethodDefinition)
                ?? throw new InvalidProgramException();
            FinalizeCloneAsyncMethod = typeof(ObjectMapping)
                .GetMethodsCached()
                .FirstOrDefault(m => m.Name == nameof(FinalizeSourceValueCloneAsync) && m.IsGenericMethodDefinition)
                ?? throw new InvalidProgramException();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        internal ObjectMapping() { }

        /// <summary>
        /// <see cref="ObjectMapping{tSource, tTarget}"/> type
        /// </summary>
        protected Type GenericMappingType
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _GenericObjectMappingType ??= typeof(ObjectMapping<,>).MakeGenericType(SourceType, TargetType) ?? throw new InvalidProgramException();
        }

        /// <summary>
        /// <see cref="ObjectMapping{tSource, tTarget}.CompileMapping"/> method
        /// </summary>
        protected MethodInfoExt CompileMappingMethod
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get
            {
                if (_CompileMappingMethod is null)
                {
                    _CompileMappingMethod = GenericMappingType.GetMethodsCached().FirstOrDefault(m => m.Name == COMPILE_MAPPING_METHOD_NAME) ?? throw new InvalidProgramException();
                    if (_CompileMappingMethod.Invoker is null) throw new InvalidProgramException();
                }
                return _CompileMappingMethod;
            }
        }

        /// <summary>
        /// Source property info
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="property">Property</param>
        /// <param name="attribute"><see cref="MapAttribute"/></param>
        protected readonly struct SourcePropertyInfo(in PropertyInfoExt property, in MapAttribute? attribute)
        {
            /// <summary>
            /// Property
            /// </summary>
            public readonly PropertyInfoExt Property = property;
            /// <summary>
            /// Map attribute
            /// </summary>
            public readonly MapAttribute? Attribute = attribute;
        }
    }
}

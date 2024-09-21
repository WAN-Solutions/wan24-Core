using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
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
        /// Validate a condition argument
        /// </summary>
        /// <param name="condition">Condition argument value</param>
        /// <returns>Valid condition argument value (may differ from the given <c>condition</c> argument value)</returns>
        /// <exception cref="ArgumentException">Invalid condition</exception>
        protected virtual object ValidateConditionArgument<tSource, tTarget>(in object condition)
            => condition switch
            {
                Condition_Delegate<object, object> => condition,
                Condition_Delegate<tSource, tTarget> => condition,
                Expression<Func<string, object, object, bool>> => condition,
                Expression<Func<string, tSource, tTarget, bool>> => condition,
                Func<string, object, object, bool> funcCondition => new Condition_Delegate<object, object>(funcCondition),
                Func<string, tSource, tTarget, bool> funcCondition => new Condition_Delegate<tSource, tTarget>(funcCondition),
                _ => throw new ArgumentException($"Invalid condition {condition.ToString() ?? "UNKNOWN"}")
            };

        /// <summary>
        /// Validate a condition argument
        /// </summary>
        /// <param name="condition">Condition argument value</param>
        /// <returns>Valid condition argument value (may differ from the given <c>condition</c> argument value)</returns>
        /// <exception cref="ArgumentException">Invalid condition</exception>
        protected virtual object ValidateConditionArgument(in object condition)
        {
            // Object condition delegate, expression or function
            if (condition is Condition_Delegate<object, object> || condition is Expression<Func<string, object, object, bool>>)
                return condition;
            if (condition is Func<string, object, object, bool> funcCondition)
                return new Condition_Delegate<object, object>(funcCondition);
            // Validate the condition type
            TypeInfoExt type = condition.GetType();
            if (!type.IsGenericType) throw new ArgumentException($"Invalid condition {condition.ToString() ?? "UNKNOWN"}", nameof(condition));
            TypeInfoExt gtd = type.GetGenericTypeDefinition() ?? throw new InvalidProgramException();
            ImmutableArray<Type> gp = type.GetGenericArguments();
            // Types condition delegate, expression or function
            if (gtd.Type == typeof(Condition_Delegate<,>) && gp.Length == 2 && gp[0] == SourceType.Type && gp[1] == TargetType.Type)
            {
                return condition;
            }
            else if (gtd.Type == typeof(Expression<>) && gp.Length == 1 && gp[0].IsGenericType && TypeInfoExt.From(gp[0]).GetGenericTypeDefinition()?.Type == typeof(Func<,,,>))
            {
                gp = ReflectionExtensions.GetCachedGenericArguments(gp[0]);
                if (gp.Length == 4 && gp[0] == typeof(string) && gp[1] == SourceType.Type && gp[2] == TargetType.Type && gp[3] == typeof(bool)) return condition;
            }
            else if (gtd.Type == typeof(Func<,,,>) && gp.Length == 4 && gp[0] == typeof(string) && gp[1] == SourceType.Type && gp[2] == TargetType.Type && gp[3] == typeof(bool))
            {
                return TypeInfoExt.From(typeof(Condition_Delegate<,>)).MakeGenericType(gp[1], gp[2]).GetConstructors()
                    .FirstOrDefault(ci => ci.ParameterCount == 1)
                    ?.Invoker
                    ?.Invoke([condition])
                    ?? throw new InvalidProgramException();
            }
            throw new ArgumentException($"Invalid condition {condition.ToString() ?? "UNKNOWN"}", nameof(condition));
        }

        /// <summary>
        /// Evaluate a condition
        /// </summary>
        /// <typeparam name="tSource">Source object type (must match <see cref="SourceType"/>)</typeparam>
        /// <typeparam name="tTarget">Target object type (must match <see cref="TargetType"/>)</typeparam>
        /// <param name="mapping">Mapping name</param>
        /// <param name="condition">Condition</param>
        /// <param name="source">Source</param>
        /// <param name="target">Target</param>
        /// <returns>If to apply the mapping for the given source and target object</returns>
        protected virtual bool EvaluateCondition<tSource, tTarget>(in string mapping, in object condition, in tSource source, in tTarget target)
        {
            Contract.Assert(source is not null && target is not null);
            return condition switch
            {
                Condition_Delegate<object, object> objectCondition => objectCondition(mapping, source, target),
                Condition_Delegate<tSource, tTarget> genericCondition => genericCondition(mapping, source, target),
                Expression => throw new MappingException("An expression condition can only be applied within a compiled mapping"),
                _ => throw new MappingException($"Invalid mapping condition {condition.ToString()?.ToQuotedLiteral() ?? "\"UNKNOWN\""} for mapping {typeof(tSource)} to {typeof(tTarget)}")
            };
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

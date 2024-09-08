using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Static
    public partial class ObjectMapping
    {
        /// <summary>
        /// If to auto-create missing mappings
        /// </summary>
        public static bool AutoCreate { get; set; } = true;

        /// <summary>
        /// If to auto-compile created mappings
        /// </summary>
        public static bool DefaultAutoCompile { get; set; } = true;

        /// <summary>
        /// Default <see cref="TargetInstanceFactory"/>
        /// </summary>
        public static TargetInstanceFactoryDelegate? DefaultTargetInstanceFactory { get; set; } = DefaultTargetInstanceCreator;

        /// <summary>
        /// Default <see cref="ObjectValidator"/> for target objects (if <see langword="null"/>, no validation will be done during or after mapping)
        /// </summary>
        public static Validate_Delegate? DefaultObjectValidator { get; set; }

        /// <summary>
        /// Registered object mappings
        /// </summary>
        public static IEnumerable<ObjectMapping> RegisteredObjectMappings => RegisteredMappings.Values;

        /// <summary>
        /// Registered object mapping types
        /// </summary>
        public static IEnumerable<(Type SourceType, Type TargetType)> RegisteredMappingTypes => RegisteredMappings.Keys;

        /// <summary>
        /// Create an object mapping
        /// </summary>
        /// <param name="sourceType">Source object type</param>
        /// <param name="targetType">Target object type</param>
        /// <param name="autoCompile">If to auto-compile the created mapping</param>
        /// <returns>Object mapping</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ObjectMapping Create(in Type sourceType, in Type targetType, in bool? autoCompile = null)
            => (ObjectMapping)(typeof(ObjectMapping<,>)
                .MakeGenericType(sourceType, targetType)
                .GetMethodsCached(BindingFlags.Static | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == nameof(Create) && m.ParameterCount == 1 && m.Invoker is not null)?
                .Invoker!(null, [autoCompile ?? DefaultAutoCompile])
                ?? throw new InvalidProgramException("Failed to get create method"));

        /// <summary>
        /// Create many object mappings with automatic mappers
        /// </summary>
        /// <param name="compile">If to compile the mappings</param>
        /// <param name="register">If to register all created mappings</param>
        /// <param name="autoCompile">If to auto-compile the created mapping</param>
        /// <param name="types">Types (set <c>RegisterTargetTypes</c> to register a mapping, even if <c>register</c> was set to <see langword="false"/>)</param>
        /// <returns>Object mappings</returns>
        public static ObjectMapping[] CreateMany(in bool compile, in bool register, bool? autoCompile, params (Type SourceType, Type TargetType, Type[]? RegisterTargetTypes)[] types)
        {
            autoCompile ??= DefaultAutoCompile;
            ObjectMapping[] res = new ObjectMapping[types.Length];
            bool hasTargetTypes;
            for (int i = 0, len = types.Length, j, len2; i < len; i++)
            {
                res[i] = Create(types[i].SourceType, types[i].TargetType, autoCompile).AddAutoMappings();
                if (compile) res[i].CreateCompiledMapping();
                hasTargetTypes = types[i].RegisterTargetTypes is not null;
                if (register || hasTargetTypes) res[i].Register();
                if (hasTargetTypes)
                    for (j = 0, len2 = types[i].RegisterTargetTypes!.Length; j < len2; res[i].Register(types[i].RegisterTargetTypes![j]), j++) ;
            }
            return res;
        }

        /// <summary>
        /// Create many object mappings with automatic mappers
        /// </summary>
        /// <param name="options">Options</param>
        /// <param name="compile">If to compile the mappings</param>
        /// <param name="register">If to register all created mappings</param>
        /// <param name="autoCompile">If to auto-compile the created mapping</param>
        /// <param name="types">Types (set <c>RegisterTargetTypes</c> to register a mapping, even if <c>register</c> was set to <see langword="false"/>)</param>
        /// <returns>Object mappings</returns>
        public static ObjectMapping[] CreateManyParallel(
            in ParallelOptions? options,
            bool compile,
            bool register,
            bool? autoCompile,
            params (Type SourceType, Type TargetType, Type[]? RegisterTargetTypes)[] types
            )
        {
            autoCompile ??= DefaultAutoCompile;
            ObjectMapping[] res = new ObjectMapping[types.Length];
            Parallel.For(fromInclusive: 0, toExclusive: types.Length, options ?? new() { MaxDegreeOfParallelism = Environment.ProcessorCount }, (i) =>
            {
                res[i] = Create(types[i].SourceType, types[i].TargetType, autoCompile).AddAutoMappings();
                if (compile) res[i].CreateCompiledMapping();
                bool hasTargetTypes = types[i].RegisterTargetTypes is not null;
                if (register || hasTargetTypes) res[i].Register();
                if (hasTargetTypes)
                    for (int j = 0, len2 = types[i].RegisterTargetTypes!.Length; j < len2; res[i].Register(types[i].RegisterTargetTypes![j]), j++) ;
            });
            return res;
        }

        /// <summary>
        /// Get a registered object mapping
        /// </summary>
        /// <param name="sourceType">Source object type</param>
        /// <param name="targetType">Target object type</param>
        /// <returns>Object mapping</returns>
        public static ObjectMapping? Get(in Type sourceType, in Type targetType)
            => RegisteredMappings.TryGetValue((sourceType, targetType), out ObjectMapping? res)
                ? res
                : null;

        /// <summary>
        /// Remove a registered object mapping
        /// </summary>
        /// <param name="sourceType">Source object type</param>
        /// <param name="targetType">Target object type</param>
        /// <returns>Removed object mapping</returns>
        public static ObjectMapping? Remove(in Type sourceType, in Type targetType)
            => RegisteredMappings.TryRemove((sourceType, targetType), out ObjectMapping? res)
                ? res
                : null;

        /// <summary>
        /// Determine if a value type can be mapped to a target object property
        /// </summary>
        /// <param name="valueType">Value type</param>
        /// <param name="pi">Target property</param>
        /// <param name="attr">Source property map attribute</param>
        /// <returns>If mapping is possible</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool CanMapTypeTo(in Type valueType, in PropertyInfo pi, in MapAttribute? attr)
            => pi.SetMethod is not null &&
                (
                    (attr?.Nested ?? false) ||
                    (attr?.CanMap ?? false) ||
                    (attr?.CanMapAsync ?? false) ||
                    pi.PropertyType.IsAssignableFrom(valueType)
                );

        /// <summary>
        /// Determine if a source object property can be mapped to a target object property
        /// </summary>
        /// <param name="sourceProperty">Source property</param>
        /// <param name="targetProperty">Target property</param>
        /// <returns>If mapping is possible</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool CanMapPropertyTo(in PropertyInfo sourceProperty, in PropertyInfo targetProperty)
        {
            MapAttribute? attr = sourceProperty.GetCustomAttributeCached<MapAttribute>();
            return CanMapTypeTo(
                (attr?.ApplyValueConverter ?? false) &&
                    sourceProperty.GetCustomAttributeCached<ValueConverterAttribute>() is ValueConverterAttribute converter &&
                    converter.TargetType is not null
                    ? converter.TargetType
                    : sourceProperty.PropertyType,
                targetProperty,
                attr
                );
        }

        /// <summary>
        /// Determine if a source object property can be mapped to a target object property
        /// </summary>
        /// <param name="sourceProperty">Source property</param>
        /// <param name="targetProperty">Target property</param>
        /// <param name="attr">Source property map attribute</param>
        /// <returns>If mapping is possible</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool CanMapPropertyTo(in PropertyInfo sourceProperty, in PropertyInfo targetProperty, out MapAttribute? attr)
        {
            attr = sourceProperty.GetCustomAttributeCached<MapAttribute>();
            return CanMapTypeTo(
                (attr?.ApplyValueConverter ?? false) &&
                    sourceProperty.GetCustomAttributeCached<ValueConverterAttribute>() is ValueConverterAttribute converter &&
                    converter.TargetType is not null
                    ? converter.TargetType
                    : sourceProperty.PropertyType,
                targetProperty,
                attr
                );
        }

        /// <summary>
        /// Determine if a source object property can be mapped to a target object property
        /// </summary>
        /// <param name="sourceProperty">Source property</param>
        /// <param name="targetProperty">Target property</param>
        /// <param name="attr">Source property map attribute</param>
        /// <returns>If mapping is possible</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool CanMapPropertyTo2(in PropertyInfo sourceProperty, in PropertyInfo targetProperty, in MapAttribute? attr)
            => CanMapTypeTo(
                (attr?.ApplyValueConverter ?? false) &&
                    sourceProperty.GetCustomAttributeCached<ValueConverterAttribute>() is ValueConverterAttribute converter &&
                    converter.TargetType is not null
                    ? converter.TargetType
                    : sourceProperty.PropertyType,
                targetProperty,
                attr
                );

        /// <summary>
        /// Default <see cref="TargetInstanceFactory"/>
        /// </summary>
        /// <param name="targetType">Target type</param>
        /// <param name="requestedType">Requested type</param>
        /// <returns>Instance</returns>
        public static object DefaultTargetInstanceCreator(Type targetType, Type requestedType)
        {
            if (!targetType.IsAssignableFrom(requestedType)) throw new InvalidOperationException($"Won't construct {requestedType} (not a {targetType})");
            if (!requestedType.CanConstruct()) throw new InvalidOperationException($"Can't construct {requestedType} (not constructable)");
            BindingFlags binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            ConstructorInfoExt[] cis = [.. TypeInfoExt.From(requestedType).Constructors.Where(c => binding.DoesMatch(c))];
            ConstructorInfoExt? parameterlessConstructor = null;
            for (int i = 0, len = cis.Length; i < len; i++)
            {
                if (cis[i].GetCustomAttributeCached<ObjectMappingConstructorAttribute>() is not null)
                    return cis[i].Constructor.InvokeAuto();
                if (cis[i].ParameterCount < 1 && cis[i].Invoker is not null)
                    parameterlessConstructor = cis[i];
            }
            return parameterlessConstructor?.Invoker!([]) ?? requestedType.ConstructAuto(usePrivate: true);
        }

        /// <summary>
        /// Default <see cref="ObjectValidator"/>
        /// </summary>
        /// <param name="value">Value to validate</param>
        public static void DefaultObjectInstanceValidator(object value)
        {
            ValidationContext vc = new(value)
            {
                DisplayName = "Mapped object"
            };
            Validator.ValidateObject(value, vc);
        }
    }
}

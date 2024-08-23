using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Static
    public partial class ObjectMapping
    {
        /// <summary>
        /// <see cref="ApplyMappings{tSource, tTarget}(in tSource, in tTarget)"/> method
        /// </summary>
        protected static readonly MethodInfoExt ApplyMethod;
        /// <summary>
        /// <see cref="ApplyMappingsAsync{tSource, tTarget}(tSource, tTarget, CancellationToken)"/> method
        /// </summary>
        protected static readonly MethodInfoExt AsyncApplyMethod;
        /// <summary>
        /// <see cref="MapAttribute.Map{tSource, tTarget}(string, tSource, tTarget)"/> method
        /// </summary>
        protected static readonly MethodInfoExt MapMethod;
        /// <summary>
        /// <see cref="MapAttribute.MapAsync{tSource, tTarget}(string, tSource, tTarget, CancellationToken)"/> method
        /// </summary>
        protected static readonly MethodInfoExt AsyncMapMethod;
        /// <summary>
        /// Registered object mappings
        /// </summary>
        protected static readonly ConcurrentDictionary<(Type, Type), ObjectMapping> RegisteredMappings = [];

        /// <summary>
        /// Static constructor
        /// </summary>
        static ObjectMapping()
        {
            ApplyMethod = typeof(ObjectMapping).GetMethodCached(nameof(ApplyMappings), BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidProgramException("Failed to get apply method");
            AsyncApplyMethod = typeof(ObjectMapping).GetMethodCached(nameof(ApplyMappingsAsync), BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidProgramException("Failed to get asynchronous apply method");
            MapMethod = typeof(MapAttribute).GetMethodCached(nameof(MapAttribute.Map), BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidProgramException("Failed to get map method");
            AsyncMapMethod = typeof(MapAttribute).GetMethodCached(nameof(MapAttribute.MapAsync), BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidProgramException("Failed to get asynchronous map method");
        }

        /// <summary>
        /// If to auto-create missing mappings
        /// </summary>
        public static bool AutoCreate { get; set; } = true;

        /// <summary>
        /// Create an object mapping
        /// </summary>
        /// <param name="sourceType">Source object type</param>
        /// <param name="targetType">Target object type</param>
        /// <returns>Object mapping</returns>
        public static ObjectMapping Create(in Type sourceType, in Type targetType)
            => (ObjectMapping)((from mi in typeof(ObjectMapping<,>).MakeGenericType(sourceType, targetType).GetMethodsCached(BindingFlags.Static | BindingFlags.Public)
                                where mi.Name == nameof(Create) &&
                                    mi.Method.IsGenericMethod
                                select mi)
                .FirstOrDefault()
                ?.Invoker!(null, [])
                    ?? throw new InvalidProgramException("Failed to get create method"));

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
                    pi.PropertyType.IsAssignableFrom(valueType) || 
                    (attr?.CanMap ?? false) || 
                    (attr?.CanMapAsync ?? false)
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
            => CanMapTypeTo(sourceProperty.PropertyType, targetProperty, sourceProperty.GetCustomAttributeCached<MapAttribute>());

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
            => CanMapTypeTo(sourceProperty.PropertyType, targetProperty, attr = sourceProperty.GetCustomAttributeCached<MapAttribute>());
    }
}

using System.Collections.Concurrent;
using System.Reflection;

namespace wan24.Core
{
    // Static
    public partial class ObjectMapping
    {
        /// <summary>
        /// <see cref="ApplyMappings{tSource, tTarget}(in tSource, in tTarget)"/> method
        /// </summary>
        protected static readonly MethodInfo ApplyMethod;
        /// <summary>
        /// <see cref="ApplyMappingsAsync{tSource, tTarget}(tSource, tTarget, CancellationToken)"/> method
        /// </summary>
        protected static readonly MethodInfo AsyncApplyMethod;
        /// <summary>
        /// <see cref="MapAttribute.Map{tSource, tTarget}(string, tSource, tTarget)"/> method
        /// </summary>
        protected static readonly MethodInfo MapMethod;
        /// <summary>
        /// <see cref="MapAttribute.MapAsync{tSource, tTarget}(string, tSource, tTarget, CancellationToken)"/> method
        /// </summary>
        protected static readonly MethodInfo AsyncMapMethod;
        /// <summary>
        /// Registered object mappings
        /// </summary>
        protected static readonly ConcurrentDictionary<(Type, Type), ObjectMapping> Registered = [];

        /// <summary>
        /// Static constructor
        /// </summary>
        static ObjectMapping()
        {
            ApplyMethod = typeof(ObjectMapping).GetMethodCached(nameof(ApplyMappings), BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidProgramException("Failed to get apply method");
            AsyncApplyMethod = typeof(ObjectMapping).GetMethodCached(nameof(ApplyMappingsAsync), BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidProgramException("Failed to get apply method");
            MapMethod = typeof(MapAttribute).GetMethodCached(nameof(MapAttribute.Map), BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidProgramException("Failed to get map method");
            AsyncMapMethod = typeof(MapAttribute).GetMethodCached(nameof(MapAttribute.MapAsync), BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidProgramException("Failed to get asynchronous map method");
        }

        /// <summary>
        /// Auto-create missing mappings?
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
                                    mi.IsGenericMethod
                                select mi)
                .FirstOrDefault()
                ?.MakeGenericMethod(sourceType, targetType)
                .Invoke(obj: null, [])
                    ?? throw new InvalidProgramException("Failed to get create method"));

        /// <summary>
        /// Get a registered object mapping
        /// </summary>
        /// <param name="sourceType">Source object type</param>
        /// <param name="targetType">Target object type</param>
        /// <returns>Object mapping</returns>
        public static ObjectMapping? Get(in Type sourceType, in Type targetType)
            => Registered.TryGetValue((sourceType, targetType), out ObjectMapping? res)
                ? res
                : null;

        /// <summary>
        /// Remove a registered object mapping
        /// </summary>
        /// <param name="sourceType">Source object type</param>
        /// <param name="targetType">Target object type</param>
        /// <returns>Removed object mapping</returns>
        public static ObjectMapping? Remove(in Type sourceType, in Type targetType)
            => Registered.TryRemove((sourceType, targetType), out ObjectMapping? res)
                ? res
                : null;
    }
}

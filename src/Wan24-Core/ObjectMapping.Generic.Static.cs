using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Generic static
    public partial class ObjectMapping<tSource, tTarget>
    {
        /// <summary>
        /// Create an object mapping
        /// </summary>
        /// <returns>Object mapping</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ObjectMapping<tSource, tTarget> Create() => new()
        {
            SourceType = TypeInfoExt.From(typeof(tSource)),
            TargetType = TypeInfoExt.From(typeof(tTarget)),
        };

        /// <summary>
        /// Create an object mapping
        /// </summary>
        /// <param name="autoCompile">If to auto-compile the created mapping</param>
        /// <returns>Object mapping</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ObjectMapping<tSource, tTarget> Create(in bool autoCompile) => new()
        {
            SourceType = TypeInfoExt.From(typeof(tSource)),
            TargetType = TypeInfoExt.From(typeof(tTarget)),
            AutoCompile = autoCompile
        };

        /// <summary>
        /// Get a registered object mapping
        /// </summary>
        /// <returns>Object mapping</returns>
        public static ObjectMapping? Get()
            => RegisteredMappings.TryGetValue((typeof(tSource), typeof(tTarget)), out ObjectMapping? res)
                ? res
                : null;

        /// <summary>
        /// Remove a registered object mapping
        /// </summary>
        /// <returns>Removed object mapping</returns>
        public static ObjectMapping? Remove()
            => RegisteredMappings.TryRemove((typeof(tSource), typeof(tTarget)), out ObjectMapping? res)
                ? res
                : null;
    }
}

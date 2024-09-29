using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Lists
    public static partial class ObjectMappingExtensions
    {
        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="sources">Source objects</param>
        /// <returns>Created and mapped target objects</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<tTarget> MapTo<tSource, tTarget>(this IEnumerable<tSource> sources)
            => sources.Select(MapTo<tSource, tTarget>);

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <returns>Created and mapped target objects</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<object> MapObjectsTo(this IEnumerable<object> sources, Type targetType)
            => sources.Select(s => MapObjectTo(s, targetType));

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <returns>Created and mapped target objects</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<object> MapObjectsTo(this IList<object> sources, Type targetType)
        {
            for (int i = 0, len = sources.Count; i < len; i++)
                yield return MapObjectTo(sources[i], targetType);
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <returns>Created and mapped target objects</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<object> MapObjectsTo(this List<object> sources, Type targetType)
        {
            for (int i = 0, len = sources.Count; i < len; i++)
                yield return MapObjectTo(sources[i], targetType);
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <returns>Created and mapped target objects</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<object> MapObjectsTo(this ImmutableArray<object> sources, Type targetType)
        {
            for (int i = 0, len = sources.Length; i < len; i++)
                yield return MapObjectTo(sources[i], targetType);
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <returns>Created and mapped target objects</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<object> MapObjectsTo(this FrozenSet<object> sources, Type targetType)
        {
            for (int i = 0, len = sources.Count; i < len; i++)
                yield return MapObjectTo(sources.Items[i], targetType);
        }
    }
}

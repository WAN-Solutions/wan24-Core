using System.Runtime.CompilerServices;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Extension methods for <see cref="IPriority"/>
    /// </summary>
    public static class PriorityExtensions
    {
        /// <summary>
        /// Prioritize
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Prioritized items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IOrderedEnumerable<T> Prioritize<T>(this IEnumerable<T> enumerable) where T : IPriority => enumerable.OrderBy(static i => i.Priority);

        /// <summary>
        /// Prioritize
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Prioritized items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IOrderedEnumerable<T> Prioritize<T>(this IOrderedEnumerable<T> enumerable) where T : IPriority => enumerable.ThenBy(static i => i.Priority);

        /// <summary>
        /// Prioritize using <see cref="IPriority"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Prioritized items (items which don't implement <see cref="IPriority"/> have priority <c>0</c>)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IOrderedEnumerable<T> PrioritizeAny<T>(this IEnumerable<T> enumerable) => enumerable.OrderBy(static i => i is IPriority p ? p.Priority : 0);

        /// <summary>
        /// Prioritize using <see cref="IPriority"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Prioritized items (items which don't implement <see cref="IPriority"/> have priority <c>0</c>)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IOrderedEnumerable<T> PrioritizeAny<T>(this IOrderedEnumerable<T> enumerable) => enumerable.ThenBy(static i => i is IPriority p ? p.Priority : 0);
    }
}

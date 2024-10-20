using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime;
using System.Runtime.CompilerServices;
using wan24.Core.Enumerables;

namespace wan24.Core
{
    // Enumerate
    public static partial class EnumerableExtensions
    {
        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ArrayEnumerable<T> Enumerate<T>(this T[] arr, int offset = 0, int? length = null) => new(arr, offset, length);

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArrayEnumerable<T> Enumerate<T>(this FrozenSet<T> arr, int offset = 0, int? length = null) => new(arr.Items, offset, length);

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArrayEnumerable<T> Enumerate<T>(this ImmutableArray<T> arr, int offset = 0, int? length = null) => new(arr, offset, length);

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ListEnumerable<tList, tItem> Enumerate<tList, tItem>(this tList arr, int offset = 0, int? length = null) where tList : IList<tItem>
            => new(arr, offset, length);
    }
}

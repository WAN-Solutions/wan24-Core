using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime;

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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this T[] arr, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? arr.Length; i < len; i++) yield return arr[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="filter">Filter</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this T[] arr, Func<T, bool> filter, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? arr.Length; i < len; i++)
                if (filter(arr[i]))
                    yield return arr[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this FrozenSet<T> arr, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? arr.Count; i < len; i++) yield return arr.Items[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="filter">Filter</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this FrozenSet<T> arr, Func<T, bool> filter, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? arr.Count; i < len; i++)
                if (filter(arr.Items[i]))
                    yield return arr.Items[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this List<T> arr, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? arr.Count; i < len; i++) yield return arr[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="filter">Filter</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this List<T> arr, Func<T, bool> filter, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? arr.Count; i < len; i++)
                if (filter(arr[i]))
                    yield return arr[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this IList<T> arr, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? arr.Count; i < len; i++) yield return arr[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="filter">Filter</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this IList<T> arr, Func<T, bool> filter, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? arr.Count; i < len; i++)
                if (filter(arr[i]))
                    yield return arr[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this ImmutableArray<T> arr, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? arr.Length; i < len; i++) yield return arr[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="filter">Filter</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this ImmutableArray<T> arr, Func<T, bool> filter, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? arr.Length; i < len; i++)
                if (filter(arr[i]))
                    yield return arr[i];
        }
    }
}

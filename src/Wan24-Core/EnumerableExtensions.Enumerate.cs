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
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this T[] arr)
        {
            for (int i = 0, len = arr.Length; i < len; i++) yield return arr[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="filter">Filter</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this T[] arr, Func<T, bool> filter)
        {
            for (int i = 0, len = arr.Length; i < len; i++)
                if (filter(arr[i]))
                    yield return arr[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this Memory<T> arr) => Enumerate((ReadOnlyMemory<T>)arr);

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this ReadOnlyMemory<T> arr)
        {
            for (int i = 0, len = arr.Length; i < len; i++) yield return arr.Span[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="filter">Filter</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this Memory<T> arr, Func<T, bool> filter) => Enumerate((ReadOnlyMemory<T>)arr, filter);

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="filter">Filter</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this ReadOnlyMemory<T> arr, Func<T, bool> filter)
        {
            for (int i = 0, len = arr.Length; i < len; i++)
                if (filter(arr.Span[i]))
                    yield return arr.Span[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this FrozenSet<T> arr)
        {
            for (int i = 0, len = arr.Count; i < len; i++) yield return arr.Items[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="filter">Filter</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this FrozenSet<T> arr, Func<T, bool> filter)
        {
            for (int i = 0, len = arr.Count; i < len; i++)
                if (filter(arr.Items[i]))
                    yield return arr.Items[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this List<T> arr)
        {
            for (int i = 0, len = arr.Count; i < len; i++) yield return arr[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="filter">Filter</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this List<T> arr, Func<T, bool> filter)
        {
            for (int i = 0, len = arr.Count; i < len; i++)
                if (filter(arr[i]))
                    yield return arr[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this IList<T> arr)
        {
            for (int i = 0, len = arr.Count; i < len; i++) yield return arr[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="filter">Filter</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this IList<T> arr, Func<T, bool> filter)
        {
            for (int i = 0, len = arr.Count; i < len; i++)
                if (filter(arr[i]))
                    yield return arr[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this ImmutableArray<T> arr)
        {
            for (int i = 0, len = arr.Length; i < len; i++) yield return arr[i];
        }

        /// <summary>
        /// Enumerate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="filter">Filter</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Enumerate<T>(this ImmutableArray<T> arr, Func<T, bool> filter)
        {
            for (int i = 0, len = arr.Length; i < len; i++)
                if (filter(arr[i]))
                    yield return arr[i];
        }
    }
}

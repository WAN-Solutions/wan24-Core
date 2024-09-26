using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime;

namespace wan24.Core
{
    // LINQ
    public static partial class EnumerableExtensions
    {
        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Where<T>(this FrozenSet<T> arr, Func<T, bool> predicate, int offset = 0, int? length = null)
        {
            T item;
            for (int i = offset, len = length ?? arr.Count; i < len; i++)
            {
                item = arr.Items[i];
                if (predicate(item))
                    yield return item;
            }
        }

        /// <summary>
        /// Skip
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="skip">Items to skip</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Skip<T>(this ImmutableArray<T> arr, int skip)
        {
            for (int i = skip, len = arr.Length; i < len; i++)
                yield return arr[i];
        }

        /// <summary>
        /// Skip
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="skip">Items to skip</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Skip<T>(this FrozenSet<T> arr, int skip)
        {
            for (int i = skip, len = arr.Count; i < len; i++)
                yield return arr.Items[i];
        }

        /// <summary>
        /// Take
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="take">Items to take</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Take<T>(this ImmutableArray<T> arr, int take)
        {
            for (int i = 0, len = Math.Min(arr.Length, take); i < len; i++)
                yield return arr[i];
        }

        /// <summary>
        /// Take
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="take">Items to take</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Take<T>(this FrozenSet<T> arr, int take)
        {
            for (int i = 0, len = Math.Min(arr.Count, take); i < len; i++)
                yield return arr.Items[i];
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="selector">Selector</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Result items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<tResult> Select<tItem, tResult>(this FrozenSet<tItem> arr, Func<tItem, tResult> selector, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? arr.Count; i < len; i++)
                yield return selector(arr.Items[i]);
        }

        /// <summary>
        /// Distinct
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
        public static IEnumerable<T> Distinct<T>(this ImmutableArray<T> arr, int offset = 0, int? length = null)
        {
            HashSet<int> seen = [];
            T item;
            for (int i = offset, len = length ?? arr.Length; i < len; i++)
            {
                item = arr[i];
                if (seen.Add(item is null ? 0 : HashCode.Combine(item.GetType(), item)))
                    yield return item;
            }
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        public static IEnumerable<tItem> DistinctBy<tItem, tKey>(this ImmutableArray<tItem> arr, Func<tItem, tKey> keySelector, int offset = 0, int? length = null)
        {
            HashSet<int> seen = [];
            tItem item;
            tKey key;
            for (int i = offset, len = length ?? arr.Length; i < len; i++)
            {
                item = arr[i];
                if (item is null)
                {
                    if (seen.Add(0))
                        yield return item;
                    continue;
                }
                key = keySelector(item);
                if (seen.Add(key is null ? 0 : HashCode.Combine(key.GetType(), key)))
                    yield return item;
            }
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Items</returns>
        public static IEnumerable<tItem> DistinctBy<tItem, tKey>(this FrozenSet<tItem> arr, Func<tItem, tKey> keySelector, int offset = 0, int? length = null)
        {
            HashSet<int> seen = [];
            tItem item;
            tKey key;
            for (int i = offset, len = length ?? arr.Count; i < len; i++)
            {
                item = arr.Items[i];
                if (item is null)
                {
                    if (seen.Add(0))
                        yield return item;
                    continue;
                }
                key = keySelector(item);
                if (seen.Add(key is null ? 0 : HashCode.Combine(key.GetType(), key)))
                    yield return item;
            }
        }

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>First item</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T First<T>(this FrozenSet<T> arr)
        {
            if (arr.Count < 1) throw new InvalidOperationException("Sequence contains no elements");
            return arr.Items[0];
        }

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>First item</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T First<T>(this FrozenSet<T> arr, Func<T, bool> predicate)
        {
            T item;
            for (int i = 0, len = arr.Count; i < len; i++)
            {
                item = arr.Items[i];
                if (predicate(item))
                    return item;
            }
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <summary>
        /// First or the default
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="defaultResult">Default result</param>
        /// <returns>First item or the <c>defaultResult</c></returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T? FirstOrDefault<T>(this FrozenSet<T> arr, T? defaultResult = default)
            => arr.Count < 1 ? defaultResult : arr.Items[0];

        /// <summary>
        /// First or the default
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="defaultResult">Default result</param>
        /// <returns>First item or the <c>defaultResult</c></returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T? FirstOrDefault<T>(this FrozenSet<T> arr, Func<T, bool> predicate, T? defaultResult = default)
        {
            T item;
            for (int i = 0, len = arr.Count; i < len; i++)
            {
                item = arr.Items[i];
                if (predicate(item))
                    return item;
            }
            return defaultResult;
        }

        /// <summary>
        /// All
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>If the predicate returned <see langword="true"/> for all items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool All<T>(this FrozenSet<T> enumerable, in Func<T, bool> predicate, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? enumerable.Count; i < len; i++)
                if (!predicate(enumerable.Items[i]))
                    return false;
            return true;
        }

        /// <summary>
        /// Any
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>If the predicate returned <see langword="true"/> for all items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Any<T>(this FrozenSet<T> enumerable, in Func<T, bool> predicate, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? enumerable.Count; i < len; i++)
                if (predicate(enumerable.Items[i]))
                    return true;
            return false;
        }
    }
}

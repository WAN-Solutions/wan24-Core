using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime;
using wan24.Core.Enumerables;

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
        /// <param name="offset">Array offset</param>
        /// <param name="length">Array length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArrayWhereEnumerable<T> Where<T>(this ImmutableArray<T> arr, Func<T, bool> predicate, int offset = 0, int? length = null)
            => new(arr, predicate, offset, length);

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="offset">Array offset</param>
        /// <param name="length">Array length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArrayWhereEnumerable<T> Where<T>(this FrozenSet<T> arr, Func<T, bool> predicate, int offset = 0, int? length = null)
            => new(arr.Items, predicate, offset, length);

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="offset">Array offset</param>
        /// <param name="length">Array length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ArrayWhereEnumerable<T> Where<T>(this T[] arr, Func<T, bool> predicate, int offset = 0, int? length = null)
            => new(arr, predicate, offset, length);

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="offset">Array offset</param>
        /// <param name="length">Array length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ListWhereEnumerable<tList, tItem> Where<tList, tItem>(this tList arr, Func<tItem, bool> predicate, int offset = 0, int? length = null) where tList : IList<tItem>
            => new(arr, predicate, offset, length);

        /// <summary>
        /// Skip
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="skip">Items to skip</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ArrayEnumerable<T> Skip<T>(this T[] arr, in int skip)
            => skip < arr.Length
                ? new(arr, skip)
                : new([]);

        /// <summary>
        /// Skip
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="skip">Items to skip</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArrayEnumerable<T> Skip<T>(this ImmutableArray<T> arr, in int skip)
            => skip < arr.Length
                ? new(arr, skip)
                : new([]);

        /// <summary>
        /// Skip
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="skip">Items to skip</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArrayEnumerable<T> Skip<T>(this FrozenSet<T> arr, int skip)
            => skip < arr.Count
                ? new(arr.Items, skip)
                : new([]);

        /// <summary>
        /// Skip
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="skip">Items to skip</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ListEnumerable<tList, tItem> Skip<tList, tItem>(this tList arr, int skip) where tList : IList<tItem> => new(arr, skip);

        /// <summary>
        /// Skip while
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArrayEnumerable<T> SkipWhile<T>(this FrozenSet<T> arr, Func<T, bool> predicate)
            => new ImmutableArrayEnumerable<T>(arr.Items).SkipWhile(predicate);

        /// <summary>
        /// Skip while
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArrayEnumerable<T> SkipWhile<T>(this ImmutableArray<T> arr, Func<T, bool> predicate)
            => new ImmutableArrayEnumerable<T>(arr).SkipWhile(predicate);

        /// <summary>
        /// Skip while
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ArrayEnumerable<T> SkipWhile<T>(this T[] arr, Func<T, bool> predicate)
            => new ArrayEnumerable<T>(arr).SkipWhile(predicate);

        /// <summary>
        /// Skip while
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ListEnumerable<tList, tItem> SkipWhile<tList, tItem>(this tList arr, Func<tItem, bool> predicate) where tList : IList<tItem>
            => new ListEnumerable<tList, tItem>(arr).SkipWhile(predicate);

        /// <summary>
        /// Take
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="take">Items to take</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ArrayEnumerable<T> Take<T>(this T[] arr, int take)
            => take < 1
                ? new([])
                : new(arr, count: take);

        /// <summary>
        /// Take
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="take">Items to take</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArrayEnumerable<T> Take<T>(this ImmutableArray<T> arr, int take)
            => take < 1
                ? new([])
                : new(arr, count: take);

        /// <summary>
        /// Take
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="take">Items to take</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArrayEnumerable<T> Take<T>(this FrozenSet<T> arr, int take)
            => take < 1
                ? new([])
                : new(arr.Items, count: take);

        /// <summary>
        /// Take
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="take">Items to take</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ListEnumerable<tList, tItem> Take<tList, tItem>(this tList arr, int take) where tList : IList<tItem> => new(arr, count: take);

        /// <summary>
        /// Take while
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArrayEnumerable<T> TakeWhile<T>(this FrozenSet<T> arr, Func<T, bool> predicate)
            => new ImmutableArrayEnumerable<T>(arr.Items).TakeWhile(predicate);

        /// <summary>
        /// Take while
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArrayEnumerable<T> TakeWhile<T>(this ImmutableArray<T> arr, Func<T, bool> predicate)
            => new ImmutableArrayEnumerable<T>(arr).TakeWhile(predicate);

        /// <summary>
        /// Take while
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ArrayEnumerable<T> TakeWhile<T>(this T[] arr, Func<T, bool> predicate)
            => new ArrayEnumerable<T>(arr).TakeWhile(predicate);

        /// <summary>
        /// Take while
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ListEnumerable<tList, tItem> TakeWhile<tList, tItem>(this tList arr, Func<tItem, bool> predicate) where tList : IList<tItem>
            => new ListEnumerable<tList, tItem>(arr).TakeWhile(predicate);

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="selector">Selector</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArraySelectEnumerable<tItem, tResult> Select<tItem, tResult>(this FrozenSet<tItem> arr, Func<tItem, tResult> selector, int offset = 0, int? length = null)
            => new(arr.Items, selector, offset, length);

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="selector">Selector</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArraySelectEnumerable<tItem, tResult> Select<tItem, tResult>(this ImmutableArray<tItem> arr, Func<tItem, tResult> selector, int offset = 0, int? length = null)
            => new(arr, selector, offset, length);

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="selector">Selector</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ArraySelectEnumerable<tItem, tResult> Select<tItem, tResult>(this tItem[] arr, Func<tItem, tResult> selector, int offset = 0, int? length = null)
            => new(arr, selector, offset, length);

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="selector">Selector</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ListSelectEnumerable<tList, tItem, tResult> Select<tList, tItem, tResult>(this tList arr, Func<tItem, tResult> selector, int offset = 0, int? length = null)
            where tList : IList<tItem>
            => new(arr, selector, offset, length);

        /// <summary>
        /// Count
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>Number of items</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int Count<T>(this FrozenSet<T> arr) => arr.Count;

        /// <summary>
        /// Count
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>Number of items</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int Count<T>(this ImmutableArray<T> arr) => arr.Length;

        /// <summary>
        /// Count
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>Number of items</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int Count<T>(this T[] arr) => arr.Length;

        /// <summary>
        /// Count
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>Number of items</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int Count<tList, tItem>(this tList arr) where tList : IList<tItem> => arr.Count;

        /// <summary>
        /// Count
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Number of items for which the predicate returned <see langword="true"/></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int Count<T>(this FrozenSet<T> arr, Func<T, bool> predicate) => new ImmutableArrayEnumerable<T>(arr.Items).Count(predicate);

        /// <summary>
        /// Count
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Number of items for which the predicate returned <see langword="true"/></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int Count<T>(this ImmutableArray<T> arr, Func<T, bool> predicate) => new ImmutableArrayEnumerable<T>(arr).Count(predicate);

        /// <summary>
        /// Count
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Number of items for which the predicate returned <see langword="true"/></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int Count<T>(this T[] arr, Func<T, bool> predicate) => new ArrayEnumerable<T>(arr).Count(predicate);

        /// <summary>
        /// Count
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Number of items for which the predicate returned <see langword="true"/></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int Count<tList, tItem>(this tList arr, Func<tItem, bool> predicate) where tList : IList<tItem>
            => new ListEnumerable<tList, tItem>(arr).Count(predicate);

        /// <summary>
        /// Contains
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="item">Item</param>
        /// <returns>If the item is contained</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Contains<T>(this FrozenSet<T> arr, T item) => new ImmutableArrayEnumerable<T>(arr.Items).Contains(item);

        /// <summary>
        /// Contains
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="item">Item</param>
        /// <returns>If the item is contained</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Contains<T>(this ImmutableArray<T> arr, T item) => new ImmutableArrayEnumerable<T>(arr).Contains(item);

        /// <summary>
        /// Contains
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="item">Item</param>
        /// <returns>If the item is contained</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Contains<T>(this T[] arr, T item) => new ArrayEnumerable<T>(arr).Contains(item);

        /// <summary>
        /// Contains
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="item">Item</param>
        /// <returns>If the item is contained</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Contains<tList, tItem>(this tList arr, tItem item) where tList : IList<tItem> => new ListEnumerable<tList, tItem>(arr).Contains(item);

        /// <summary>
        /// Contains at least
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="count">Count</param>
        /// <returns>If contains at least <c>count</c></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool ContainsAtLeast<T>(this FrozenSet<T> arr, int count) => new ImmutableArrayEnumerable<T>(arr.Items).ContainsAtLeast(count);

        /// <summary>
        /// Contains at least
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="count">Count</param>
        /// <returns>If contains at least <c>count</c></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool ContainsAtLeast<T>(this ImmutableArray<T> arr, int count) => new ImmutableArrayEnumerable<T>(arr).ContainsAtLeast(count);

        /// <summary>
        /// Contains at least
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="count">Count</param>
        /// <returns>If contains at least <c>count</c></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool ContainsAtLeast<T>(this T[] arr, int count) => new ArrayEnumerable<T>(arr).ContainsAtLeast(count);

        /// <summary>
        /// Contains at least
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="count">Count</param>
        /// <returns>If contains at least <c>count</c></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool ContainsAtLeast<tList, tItem>(this tList arr, int count) where tList : IList<tItem> => new ListEnumerable<tList, tItem>(arr).ContainsAtLeast(count);

        /// <summary>
        /// Contains at most
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="count">Count</param>
        /// <returns>If contains at most <c>count</c></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool ContainsAtMost<T>(this FrozenSet<T> arr, int count) => new ImmutableArrayEnumerable<T>(arr.Items).ContainsAtMost(count);

        /// <summary>
        /// Contains at most
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="count">Count</param>
        /// <returns>If contains at most <c>count</c></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool ContainsAtMost<T>(this ImmutableArray<T> arr, int count) => new ImmutableArrayEnumerable<T>(arr).ContainsAtMost(count);

        /// <summary>
        /// Contains at most
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="count">Count</param>
        /// <returns>If contains at most <c>count</c></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool ContainsAtMost<T>(this T[] arr, int count) => new ArrayEnumerable<T>(arr).ContainsAtMost(count);

        /// <summary>
        /// Contains at most
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="count">Count</param>
        /// <returns>If contains at most <c>count</c></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool ContainsAtMost<tList, tItem>(this tList arr, int count) where tList : IList<tItem> => new ListEnumerable<tList, tItem>(arr).ContainsAtMost(count);

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Distinct<T>(this ImmutableArray<T> arr, int offset = 0, int? length = null)
            => new ImmutableArrayEnumerable<T>(arr, offset, length).Distinct();

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Distinct<T>(this FrozenSet<T> arr, int offset = 0, int? length = null)
            => new ImmutableArrayEnumerable<T>(arr.Items, offset, length).Distinct();

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> Distinct<T>(this T[] arr, int offset = 0, int? length = null)
            => new ArrayEnumerable<T>(arr, offset, length).Distinct();

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<tItem> Distinct<tList, tItem>(this tList arr, int offset = 0, int? length = null) where tList : IList<tItem>
            => new ListEnumerable<tList, tItem>(arr, offset, length).Distinct();

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<tItem> DistinctBy<tItem, tKey>(this ImmutableArray<tItem> arr, Func<tItem, tKey> keySelector, int offset = 0, int? length = null)
            => new ImmutableArrayEnumerable<tItem>(arr, offset, length).DistinctBy(keySelector);

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<tItem> DistinctBy<tItem, tKey>(this FrozenSet<tItem> arr, Func<tItem, tKey> keySelector, int offset = 0, int? length = null)
            => new ImmutableArrayEnumerable<tItem>(arr.Items, offset, length).DistinctBy(keySelector);

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<tItem> DistinctBy<tItem, tKey>(this tItem[] arr, Func<tItem, tKey> keySelector, int offset = 0, int? length = null)
            => new ArrayEnumerable<tItem>(arr, offset, length).DistinctBy(keySelector);

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Enumerable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<tItem> DistinctBy<tList, tItem, tKey>(this tList arr, Func<tItem, tKey> keySelector, int offset = 0, int? length = null) where tList : IList<tItem>
            => new ListEnumerable<tList, tItem>(arr, offset, length).DistinctBy(keySelector);

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>First item</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T First<T>(this FrozenSet<T> arr) => First(arr.Items);

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>First item</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T First<T>(this FrozenSet<T> arr, Func<T, bool> predicate) => First(arr.Items, predicate);

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
        public static T First<T>(this ImmutableArray<T> arr)
        {
            if (arr.Length < 1) throw new InvalidOperationException("Sequence contains no elements");
            return arr[0];
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
        public static T First<T>(this ImmutableArray<T> arr, Func<T, bool> predicate)
        {
            T item;
            for (int i = 0, len = arr.Length; i < len; i++)
            {
                item = arr[i];
                if (predicate(item))
                    return item;
            }
            throw new InvalidOperationException("Sequence contains no elements");
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
        public static T First<T>(this T[] arr)
        {
            if (arr.Length < 1) throw new InvalidOperationException("Sequence contains no elements");
            return arr[0];
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
        public static T First<T>(this T[] arr, Func<T, bool> predicate)
        {
            T item;
            for (int i = 0, len = arr.Length; i < len; i++)
            {
                item = arr[i];
                if (predicate(item))
                    return item;
            }
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>First item</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tItem First<tList, tItem>(this tList arr) where tList : IList<tItem>
        {
            if (arr.Count < 1) throw new InvalidOperationException("Sequence contains no elements");
            return arr[0];
        }

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>First item</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tItem First<tList, tItem>(this tList arr, Func<tItem, bool> predicate) where tList : IList<tItem>
        {
            tItem item;
            for (int i = 0, len = arr.Count; i < len; i++)
            {
                item = arr[i];
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
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T? FirstOrDefault<T>(this FrozenSet<T> arr, T? defaultResult = default) => FirstOrDefault(arr.Items, defaultResult);

        /// <summary>
        /// First or the default
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="defaultResult">Default result</param>
        /// <returns>First item or the <c>defaultResult</c></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T? FirstOrDefault<T>(this FrozenSet<T> arr, Func<T, bool> predicate, T? defaultResult = default) => FirstOrDefault(arr.Items, predicate, defaultResult);

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
        public static T? FirstOrDefault<T>(this ImmutableArray<T> arr, T? defaultResult = default)
            => arr.Length < 1 ? defaultResult : arr[0];

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
        public static T? FirstOrDefault<T>(this ImmutableArray<T> arr, Func<T, bool> predicate, T? defaultResult = default)
        {
            T item;
            for (int i = 0, len = arr.Length; i < len; i++)
            {
                item = arr[i];
                if (predicate(item))
                    return item;
            }
            return defaultResult;
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
        public static T? FirstOrDefault<T>(this T[] arr, T? defaultResult = default)
            => arr.Length < 1 ? defaultResult : arr[0];

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
        public static T? FirstOrDefault<T>(this T[] arr, Func<T, bool> predicate, T? defaultResult = default)
        {
            T item;
            for (int i = 0, len = arr.Length; i < len; i++)
            {
                item = arr[i];
                if (predicate(item))
                    return item;
            }
            return defaultResult;
        }

        /// <summary>
        /// First or the default
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="defaultResult">Default result</param>
        /// <returns>First item or the <c>defaultResult</c></returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tItem? FirstOrDefault<tList, tItem>(this tList arr, tItem? defaultResult = default) where tList : IList<tItem>
            => arr.Count < 1 ? defaultResult : arr[0];

        /// <summary>
        /// First or the default
        /// </summary>
        /// <typeparam name="tList">List typoe</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="defaultResult">Default result</param>
        /// <returns>First item or the <c>defaultResult</c></returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tItem? FirstOrDefault<tList, tItem>(this tList arr, Func<tItem, bool> predicate, tItem? defaultResult = default) where tList : IList<tItem>
        {
            tItem item;
            for (int i = 0, len = arr.Count; i < len; i++)
            {
                item = arr[i];
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
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool All<T>(this FrozenSet<T> enumerable, in Func<T, bool> predicate, int offset = 0, int? length = null) => All(enumerable.Items, predicate, offset, length);

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
        public static bool All<T>(this ImmutableArray<T> enumerable, in Func<T, bool> predicate, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? enumerable.Length; i < len; i++)
                if (!predicate(enumerable[i]))
                    return false;
            return true;
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
        public static bool All<T>(this T[] enumerable, in Func<T, bool> predicate, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? enumerable.Length; i < len; i++)
                if (!predicate(enumerable[i]))
                    return false;
            return true;
        }

        /// <summary>
        /// All
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>If the predicate returned <see langword="true"/> for all items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool All<tList, tItem>(this tList enumerable, in Func<tItem, bool> predicate, int offset = 0, int? length = null) where tList : IList<tItem>
        {
            for (int i = offset, len = length ?? enumerable.Count; i < len; i++)
                if (!predicate(enumerable[i]))
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
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Any<T>(this FrozenSet<T> enumerable, in Func<T, bool> predicate, int offset = 0, int? length = null) => Any(enumerable.Items, predicate, offset, length);

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
        public static bool Any<T>(this ImmutableArray<T> enumerable, in Func<T, bool> predicate, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? enumerable.Length; i < len; i++)
                if (predicate(enumerable[i]))
                    return true;
            return false;
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
        public static bool Any<T>(this T[] enumerable, in Func<T, bool> predicate, int offset = 0, int? length = null)
        {
            for (int i = offset, len = length ?? enumerable.Length; i < len; i++)
                if (predicate(enumerable[i]))
                    return true;
            return false;
        }

        /// <summary>
        /// Any
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>If the predicate returned <see langword="true"/> for all items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Any<tList, tItem>(this tList enumerable, in Func<tItem, bool> predicate, int offset = 0, int? length = null) where tList : IList<tItem>
        {
            for (int i = offset, len = length ?? enumerable.Count; i < len; i++)
                if (predicate(enumerable[i]))
                    return true;
            return false;
        }

        /// <summary>
        /// Any
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>If the enumerable contains any items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Any<T>(this FrozenSet<T> enumerable) => enumerable.Count > 0;

        /// <summary>
        /// Any
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>If the enumerable contains any items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Any<T>(this ImmutableArray<T> enumerable) => enumerable.Length > 0;

        /// <summary>
        /// Any
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>If the enumerable contains any items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Any<T>(this T[] enumerable) => enumerable.Length > 0;

        /// <summary>
        /// Any
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>If the enumerable contains any items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Any<tList, tItem>(this tList enumerable) where tList : IList<tItem> => enumerable.Count > 0;
    }
}

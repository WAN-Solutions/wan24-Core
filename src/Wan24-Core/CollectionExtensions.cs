using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Collection extensions
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Add items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="collection">Collection</param>
        /// <param name="items">Items</param>
        /// <returns>Collection</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICollection<T> AddRange<T>(this ICollection<T> collection, params T[] items) => AddRange(collection, (IEnumerable<T>)items);

        /// <summary>
        /// Add items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="collection">Collection</param>
        /// <param name="items">Items</param>
        /// <returns>Collection</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICollection<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (T item in items) collection.Add(item);
            return collection;
        }

        /// <summary>
        /// Add items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="collection">Collection</param>
        /// <param name="items">Items</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task AddRangeAsync<T>(this ICollection<T> collection, IAsyncEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            await foreach (T item in items.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) collection.Add(item);
        }

        /// <summary>
        /// Move the item at the index up
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="collection">Collection</param>
        /// <param name="index">Index</param>
        /// <returns>Collection</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ICollection<T> MoveIndexUp<T>(this ICollection<T> collection, in int index)
        {
            if (index < 1 || index >= collection.Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (collection is IList<T> list)
            {
                list.MoveIndexUp(index);
                return collection;
            }
            int prevIndex = index - 1;
            List<T> items = [.. collection];
            T item = items[index],
                prevItem = items[prevIndex];
            items[prevIndex] = item;
            items[index] = prevItem;
            collection.Clear();
            collection.AddRange(items);
            return collection;
        }

        /// <summary>
        /// Move the item at the index down
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="collection">Collection</param>
        /// <param name="index">Index</param>
        /// <returns>Collection</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ICollection<T> MoveIndexDown<T>(this ICollection<T> collection, in int index)
        {
            if (index < 0 || index >= collection.Count - 1) throw new ArgumentOutOfRangeException(nameof(index));
            if (collection is IList<T> list)
            {
                list.MoveIndexDown(index);
                return collection;
            }
            int nextIndex = index + 1;
            List<T> items = [.. collection];
            T item = items[index],
                nextItem = items[nextIndex];
            items[nextIndex] = item;
            items[index] = nextItem;
            collection.Clear();
            collection.AddRange(items);
            return collection;
        }

        /// <summary>
        /// Move an item up
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="collection">Collection</param>
        /// <param name="item">Item</param>
        /// <returns>Collection</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICollection<T> MoveItemUp<T>(this ICollection<T> collection, in T item) => MoveIndexUp(collection, collection.ElementIndex(item));

        /// <summary>
        /// Move an item up
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="collection">Collection</param>
        /// <param name="item">Item</param>
        /// <returns>Collection</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICollection<T> MoveItemDown<T>(this ICollection<T> collection, in T item) => MoveIndexDown(collection, collection.ElementIndex(item));

        /// <summary>
        /// Create an array
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="firstItem">First item</param>
        /// <param name="followingItems">Following items</param>
        /// <returns>Array</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T[] CreateArrayWith<T>(this T firstItem, params T[] followingItems) => [firstItem, .. followingItems];

        /// <summary>
        /// Create a list
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="firstItem">First item</param>
        /// <param name="followingItems">Following items</param>
        /// <returns>List</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static List<T> CreateListWith<T>(this T firstItem, params T[] followingItems) => [firstItem, .. followingItems];

        /// <summary>
        /// Create a hash set
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="firstItem">First item</param>
        /// <param name="followingItems">Following items</param>
        /// <returns>Hash set</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static HashSet<T> CreateHashSetWith<T>(this T firstItem, params T[] followingItems) => [firstItem, .. followingItems];

        /// <summary>
        /// Create a dictionary
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="item">Item</param>
        /// <param name="moreItems">More items</param>
        /// <returns>Dictionary</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Dictionary<tKey, tValue> CreateDictionaryWith<tKey,tValue>(this KeyValuePair<tKey, tValue> item, params KeyValuePair<tKey, tValue>[] moreItems)
            where tKey: notnull
        {
            Dictionary<tKey, tValue> res = new(moreItems.Length + 1);
            res.AddRange([item, ..moreItems]);
            return res;
        }
    }
}

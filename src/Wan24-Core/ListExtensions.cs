using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// List extensions
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Move the item at the index up
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="list">List</param>
        /// <param name="index">Index</param>
        /// <returns>List</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static IList<T> MoveIndexUp<T>(this IList<T> list, in int index)
        {
            if (index < 1 || index >= list.Count) throw new ArgumentOutOfRangeException(nameof(index));
            int prevIndex = index - 1;
            T item = list[index],
                prevItem = list[prevIndex];
            list[prevIndex] = item;
            list[index] = prevItem;
            return list;
        }

        /// <summary>
        /// Move the item at the index down
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="list">List</param>
        /// <param name="index">Index</param>
        /// <returns>List</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static IList<T> MoveIndexDown<T>(this IList<T> list, in int index)
        {
            if (index < 0 || index >= list.Count - 1) throw new ArgumentOutOfRangeException(nameof(index));
            int nextIndex = index + 1;
            T item = list[index],
                nextItem = list[nextIndex];
            list[nextIndex] = item;
            list[index] = nextItem;
            return list;
        }

        /// <summary>
        /// Move an item up
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="list">List</param>
        /// <param name="item">Item</param>
        /// <returns>List</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static IList<T> MoveItemUp<T>(this IList<T> list, in T item) => MoveIndexUp(list, list.ElementIndex(item));

        /// <summary>
        /// Move an item up
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="list">List</param>
        /// <param name="item">Item</param>
        /// <returns>List</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static IList<T> MoveItemDown<T>(this IList<T> list, in T item) => MoveIndexDown(list, list.ElementIndex(item));
    }
}

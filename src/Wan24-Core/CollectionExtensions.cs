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
        public static ICollection<T> AddRange<T>(this ICollection<T> collection, params T[] items) => AddRange(collection, items.AsEnumerable());

        /// <summary>
        /// Add items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="collection">Collection</param>
        /// <param name="items">Items</param>
        /// <returns>Collection</returns>
        public static ICollection<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (T item in items) collection.Add(item);
            return collection;
        }
    }
}

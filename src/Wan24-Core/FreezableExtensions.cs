namespace wan24.Core
{
    /// <summary>
    /// Freezable extensions
    /// </summary>
    public static class FreezableExtensions
    {
        /// <summary>
        /// Get as <see cref="FreezableDictionary{tKey, tValue}"/>
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="items">Items</param>
        /// <returns>Dictionary</returns>
        public static FreezableDictionary<tKey, tValue> ToFreezableDictionary<tKey, tValue>(this IEnumerable<KeyValuePair<tKey, tValue>> items) where tKey : notnull
            => new(items);

        /// <summary>
        /// Get as <see cref="FreezableList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="items">Items</param>
        /// <returns>List</returns>
        public static FreezableList<T> ToFreezableList<T>(this IEnumerable<T> items)
            => new(items);

        /// <summary>
        /// Get as <see cref="FreezableSet{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="items">Items</param>
        /// <returns>Set</returns>
        public static FreezableSet<T> ToFreezableSet<T>(this IEnumerable<T> items)
            => new(items);

        /// <summary>
        /// Get as <see cref="FreezableOrderedDictionary{tKey, tValue}"/>
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="items">Items</param>
        /// <returns>Dictionary</returns>
        public static FreezableOrderedDictionary<tKey, tValue> ToFreezableOrderedDictionary<tKey, tValue>(this IEnumerable<KeyValuePair<tKey, tValue>> items) where tKey : notnull
            => new(items);
    }
}

namespace wan24.Core.Caching
{
    /// <summary>
    /// Event arguments for cache item events
    /// </summary>
    public class CacheItemEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="item">Item</param>
        public CacheItemEventArgs(ICacheItem item) : base() => Item = item;

        /// <summary>
        /// Item
        /// </summary>
        public ICacheItem Item { get; }
    }
}

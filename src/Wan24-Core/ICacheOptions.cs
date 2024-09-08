namespace wan24.Core
{
    /// <summary>
    /// Interface for cache options
    /// </summary>
    public interface ICacheOptions
    {
        /// <summary>
        /// Max. cacheable item size
        /// </summary>
        int MaxItemSize { get; }
    }
}

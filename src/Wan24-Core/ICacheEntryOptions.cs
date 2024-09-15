namespace wan24.Core
{
    /// <summary>
    /// Interface for cache entry options
    /// </summary>
    public interface ICacheEntryOptions
    {
        /// <summary>
        /// Item size (should be at last <c>1</c>)
        /// </summary>
        int Size { get; set; }
    }
}

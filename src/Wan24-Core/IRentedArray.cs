using System.Buffers;

namespace wan24.Core
{
    /// <summary>
    /// Interface for a rented array
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
    public interface IRentedArray<T> : IArray<T>, IMemoryOwner<T>
    {
        /// <summary>
        /// Pool
        /// </summary>
        ArrayPool<T> Pool { get; }
        /// <summary>
        /// Clear the array when returning?
        /// </summary>
        bool Clear { get; set; }
        /// <summary>
        /// Create a non-rented copy of the array
        /// </summary>
        /// <returns>Copy</returns>
        T[] GetCopy();
    }
}

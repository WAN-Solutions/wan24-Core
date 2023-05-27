using System.Collections;

namespace wan24.Core
{
    /// <summary>
    /// Interface for an array
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
    public interface IArray<T> : IEnumerable<T>, IEnumerable, IEquatable<Memory<T>>
    {
        /// <summary>
        /// Get/set an element
        /// </summary>
        /// <param name="offset">Index</param>
        /// <returns>Element</returns>
        T this[int offset] { get; set; }
        /// <summary>
        /// Get a range
        /// </summary>
        /// <param name="range">Range</param>
        /// <returns>Range memory</returns>
        Memory<T> this[Range range] { get; }
        /// <summary>
        /// Get a range
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        /// <returns>Range memory</returns>
        Memory<T> this[Index start, Index end] { get; }
        /// <summary>
        /// Length
        /// </summary>
        int Length { get; }
        /// <summary>
        /// Length
        /// </summary>
        long LongLength { get; }
        /// <summary>
        /// Array
        /// </summary>
        T[] Array { get; }
        /// <summary>
        /// Span
        /// </summary>
        Span<T> Span { get; }
        /// <summary>
        /// Memory
        /// </summary>
        Memory<T> Memory { get; }
    }
}

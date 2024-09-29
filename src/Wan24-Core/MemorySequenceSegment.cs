using System.Buffers;

namespace wan24.Core
{
    /// <summary>
    /// Memory sequence segment
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public sealed class MemorySequenceSegment<T> : ReadOnlySequenceSegment<T>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="memory">Memory</param>
        public MemorySequenceSegment(in ReadOnlyMemory<T> memory) : base() => Memory = memory;

        /// <summary>
        /// Append memory
        /// </summary>
        /// <param name="memory">Memory</param>
        /// <returns>Segment</returns>
        public MemorySequenceSegment<T> Append(in ReadOnlyMemory<T> memory)
        {
            MemorySequenceSegment<T> res = new(memory)
            {
                RunningIndex = RunningIndex + Memory.Length
            };
            Next = res;
            return res;
        }
    }
}

using System.Buffers;

namespace wan24.Core
{
    /// <summary>
    /// Memory owner
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="memory">Memory</param>
    public class MemoryOwner<T>(in Memory<T> memory) : BasicDisposableBase(), IMemoryOwner<T>
    {
        /// <summary>
        /// Memory
        /// </summary>
        public virtual Memory<T> Memory { get; } = memory;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) { }
    }
}

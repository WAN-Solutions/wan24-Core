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

        /// <summary>
        /// If to clear the <see cref="Memory"/> when disposing (<see cref="Settings.ClearBuffers"/> isn't used here!)
        /// </summary>
        public virtual bool Clear { get; set; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (Clear)
            {
                Memory<T> mem = Memory;
                if (mem is Memory<byte> byteArr) byteArr.Span.Clean();
                else mem.Span.Clear();
            }
        }
    }
}

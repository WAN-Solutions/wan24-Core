using System.Buffers;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="MemoryPool{T}"/> extensions
    /// </summary>
    public static class MemoryPoolExtensions
    {
        /// <summary>
        /// Rent a clean memory
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="pool">Memory pool</param>
        /// <param name="len">Length</param>
        /// <returns>Rented clean memory</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IMemoryOwner<T> RentClean<T>(this MemoryPool<T> pool, in int len)
        {
            IMemoryOwner<T> res = pool.Rent(len);
            try
            {
                if(res.Memory is Memory<byte> bytes)
                {
                    bytes.Span.Clean();
                }
                else
                {
                    res.Memory.Span.Clear();
                }
                return res;
            }
            catch
            {
                res.Dispose();
                throw;
            }
        }
    }
}

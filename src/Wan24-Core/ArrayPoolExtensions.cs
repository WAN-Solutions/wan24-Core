﻿using System.Buffers;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="ArrayPool{T}"/> extensions
    /// </summary>
    public static class ArrayPoolExtensions
    {
        /// <summary>
        /// Rent a clean array
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="pool">Array pool</param>
        /// <param name="len">Length</param>
        /// <returns>Rented clean array</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T[] RentClean<T>(this ArrayPool<T> pool, in int len)
        {
            T[] res = pool.Rent(len);
            try
            {
                res.AsSpan(0, len).Clear();
                return res;
            }
            catch
            {
                pool.Return(res);
                throw;
            }
        }
    }
}

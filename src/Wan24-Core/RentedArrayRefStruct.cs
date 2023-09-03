using System.Buffers;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Pool rented array (returns the array to the pool, when disposed)
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    [StructLayout(LayoutKind.Auto)]
    public ref struct RentedArrayRefStruct<T>
    {
        /// <summary>
        /// Array pool
        /// </summary>
        public readonly ArrayPool<T> Pool;
        /// <summary>
        /// Rented array
        /// </summary>
        public readonly T[] Array;
        /// <summary>
        /// Length
        /// </summary>
        public readonly int Length;
        /// <summary>
        /// Span
        /// </summary>
        public readonly Span<T> Span;
        /// <summary>
        /// Clear the rented array before returning?
        /// </summary>
        public bool Clear = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Length</param>
        /// <param name="pool">Pool</param>
        /// <param name="clean">Clean the rented array?</param>
        public RentedArrayRefStruct(in int len, in ArrayPool<T>? pool = null, in bool clean = true)
        {
            if (len < 1) throw new ArgumentOutOfRangeException(nameof(len));
            Pool = pool ?? ArrayPool<T>.Shared;
            Array = clean ? Pool.RentClean(len) : Pool.Rent(len);
            Span = Array.AsSpan(0, len);
            Length = len;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        /// <param name="arr">Rented array</param>
        /// <param name="len">Length</param>
        /// <param name="clean">Clean the rented array?</param>
        public RentedArrayRefStruct(in ArrayPool<T> pool, in T[] arr, int? len = null, in bool clean = false)
        {
            Pool = pool;
            Array = arr;
            len ??= arr.Length;
            Length = len.Value;
            if (len < 1 || len > arr.Length)
            {
                Span = arr.AsSpan();
                Dispose();
                throw new ArgumentOutOfRangeException(nameof(len));
            }
            Span = arr.AsSpan(0, len.Value);
            if (clean) System.Array.Clear(arr, 0, len.Value);
        }

        /// <summary>
        /// Get/set an item
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Item</returns>
        public readonly T this[in int offset]
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get
            {
                if (offset < 0 || offset >= Length) throw new ArgumentOutOfRangeException(nameof(offset));
                return Array[offset];
            }
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set
            {
                if (offset < 0 || offset >= Length) throw new ArgumentOutOfRangeException(nameof(offset));
                Array[offset] = value;
            }
        }

        /// <summary>
        /// Get as span
        /// </summary>
        /// <param name="range">Range</param>
        /// <returns>Span</returns>
        public readonly Span<T> this[in Range range]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Span[range];
        }

        /// <summary>
        /// Get as span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        /// <returns>Span</returns>
        public readonly Span<T> this[in Index start, in Index end]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Span[new Range(start, end)];
        }

        /// <summary>
        /// Get a copy of the array
        /// </summary>
        /// <returns></returns>
        public readonly T[] GetCopy()
        {
            T[] res = new T[Length];
            System.Array.Copy(Array, 0, res, 0, Length);
            return res;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override readonly bool Equals(object? obj) => Array.Equals(obj);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override readonly int GetHashCode() => Array.GetHashCode();

        /// <summary>
        /// Dispose
        /// </summary>
        public readonly void Dispose()
        {
            if (Clear)
            {
                bool clear = true;
                if (Array is byte[] byteArr)
                {
                    clear = false;
                    byteArr.AsSpan(0, Length).Clean();
                }
                else if (Array is char[] charArr)
                {
                    clear = false;
                    charArr.Clear();
                }
                Pool.Return(Array, clearArray: clear);
            }
            else
            {
                Pool.Return(Array, clearArray: false);
            }
        }
    }
}

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
        /// Length
        /// </summary>
        public readonly int Length;
        /// <summary>
        /// As span
        /// </summary>
        public readonly Span<T> Span;
        /// <summary>
        /// As memory
        /// </summary>
        public readonly Memory<T> Memory;
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
            Length = len;
            Array = clean ? Pool.RentClean(len) : Pool.Rent(len);
            Memory = Array.AsMemory(0, len);
            Span = Memory.Span;
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
            len ??= arr.Length;
            if (len < 1 || len > arr.Length)
            {
                pool.Return(arr, clearArray: clean);
                throw new ArgumentOutOfRangeException(nameof(len));
            }
            Pool = pool;
            Length = len.Value;
            Array = arr;
            if (clean) System.Array.Clear(arr, 0, len.Value);
            Memory = Array.AsMemory(0, len.Value);
            Span = Memory.Span;
        }

        /// <summary>
        /// Get/set an item
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Item</returns>
        public readonly T this[int offset]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array[offset];
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set => Array[offset] = value;
        }

        /// <summary>
        /// Get as memory
        /// </summary>
        /// <param name="range">Range</param>
        /// <returns>Memory</returns>
        public readonly Memory<T> this[Range range]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Memory[range];
        }

        /// <summary>
        /// Get as memory
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        /// <returns>Memory</returns>
        public readonly Memory<T> this[Index start, Index end]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Memory[new Range(start, end)];
        }

        /// <summary>
        /// Rented array
        /// </summary>
        public T[] Array { get; private set; }

        /// <summary>
        /// Get a copy of the array
        /// </summary>
        /// <returns></returns>
        public readonly T[] GetCopy()
        {
            if (Length == 0) return System.Array.Empty<T>();
            T[] res = new T[Length];
            Span.CopyTo(res.AsSpan(0, Length));
            return res;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override readonly bool Equals(object? obj) => Memory.Equals(obj);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public readonly bool Equals(Memory<T> other) => Memory.Equals(other);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override readonly int GetHashCode() => Memory.GetHashCode();

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            T[] arr = Array;
            Array = System.Array.Empty<T>();
            if (Clear)
            {
                bool clear = true;
                if (arr is byte[] byteArr)
                {
                    clear = false;
                    byteArr.AsSpan(0, Length).Clean();
                }
                else if (arr is char[] charArr)
                {
                    clear = false;
                    charArr.Clear();
                }
                Pool.Return(arr, clearArray: clear);
            }
            else
            {
                Pool.Return(arr, clearArray: false);
            }
        }
    }
}

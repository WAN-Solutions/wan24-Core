using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Pool rented array (returns the array to the pool, when disposed)
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public sealed class RentedArray<T> : DisposableBase, IRentedArray<T>
    {
        /// <summary>
        /// Rented array
        /// </summary>
        private T[] _Array;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Length</param>
        /// <param name="pool">Pool</param>
        /// <param name="clean">Clean the rented array?</param>
        public RentedArray(in int len, in ArrayPool<T>? pool = null, in bool clean = true) : base()
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(len, 1);
            Pool = pool ?? ArrayPool<T>.Shared;
            Length = len;
            _Array = clean ? Pool.RentClean(len) : Pool.Rent(len);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        /// <param name="arr">Rented array</param>
        /// <param name="len">Length</param>
        /// <param name="clean">Clean the rented array?</param>
        public RentedArray(in ArrayPool<T> pool, in T[] arr, int? len = null, in bool clean = false) : base()
        {
            Pool = pool;
            _Array = arr;
            len ??= arr.Length;
            Length = len.Value;
            if (len < 1 || len > arr.Length)
            {
                Dispose();
                throw new ArgumentOutOfRangeException(nameof(len));
            }
            if (clean) System.Array.Clear(arr, 0, len ?? arr.Length);
        }

        /// <inheritdoc/>
        public ArrayPool<T> Pool { get; }

        /// <inheritdoc/>
        public int Length { get; }

        /// <inheritdoc/>
        public long LongLength
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Length;
        }

        /// <inheritdoc/>
        public T this[int offset]
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get
            {
                if (offset < 0 || offset >= Length) throw new ArgumentOutOfRangeException(nameof(offset));
                return _Array[offset];
            }
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set
            {
                if (offset < 0 || offset >= Length) throw new ArgumentOutOfRangeException(nameof(offset));
                _Array[offset] = value;
            }
        }

        /// <inheritdoc/>
        public Memory<T> this[Range range] => Memory[range];

        /// <inheritdoc/>
        public Memory<T> this[Index start, Index end] => Memory[new Range(start, end)];

        /// <inheritdoc/>
        public T[] Array => IfUndisposed(_Array);

        /// <inheritdoc/>
        public Span<T> Span => Memory.Span;

        /// <inheritdoc/>
        public Memory<T> Memory => Array.AsMemory(0, Length);

        /// <inheritdoc/>
        public bool Clear { get; set; }

        /// <inheritdoc/>
        public T[] GetCopy()
        {
            EnsureUndisposed();
            T[] res = new T[Length];
            System.Array.Copy(_Array, 0, res, 0, Length);
            return res;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override bool Equals([NotNullWhen(true)] object? obj) => Memory.Equals(obj);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public bool Equals(Memory<T> other) => Memory.Equals(other);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override int GetHashCode() => Array.GetHashCode();

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => Array.Take(Length).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => Array.Take(Length).GetEnumerator();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            T[] arr = _Array;
            _Array = [];
            if (Clear)
            {
                bool clear = true;
                if (arr is byte[] byteArr)
                {
                    clear = false;
                    byteArr.AsSpan(0, Length).Clean();
                }
                else if(arr is char[] charArr)
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

        /// <summary>
        /// Cast as array
        /// </summary>
        /// <param name="arr">Array (may be longer than <see cref="Length"/>!)</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator T[](in RentedArray<T> arr) => arr._Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Span<T>(in RentedArray<T> arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Memory<T>(in RentedArray<T> arr) => arr.Memory;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator int(in RentedArray<T> arr) => arr.Length;
    }
}

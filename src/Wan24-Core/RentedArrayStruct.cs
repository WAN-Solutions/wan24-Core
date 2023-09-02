using System.Buffers;
using System.Collections;
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
    public struct RentedArrayStruct<T> : IRentedArray<T>
    {
        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        private readonly object SyncObject = new();
        /// <summary>
        /// Rented array
        /// </summary>
        private T[] _Array;
        /// <summary>
        /// Is disposed?
        /// </summary>
        private bool IsDisposed = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Length</param>
        /// <param name="pool">Pool</param>
        /// <param name="clean">Clean the rented array?</param>
        public RentedArrayStruct(in int len, in ArrayPool<T>? pool = null, in bool clean = true)
        {
            if (len < 1) throw new ArgumentOutOfRangeException(nameof(len));
            Pool = pool ?? ArrayPool<T>.Shared;
            Length = len;
            LongLength = len;
            _Array = clean ? Pool.RentClean(len) : Pool.Rent(len);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        /// <param name="arr">Rented array</param>
        /// <param name="len">Length</param>
        /// <param name="clean">Clean the rented array?</param>
        public RentedArrayStruct(in ArrayPool<T> pool, in T[] arr, int? len = null, in bool clean = false)
        {
            len ??= arr.Length;
            if (len < 1 || len > arr.Length)
            {
                pool.Return(arr, clearArray: clean);
                throw new ArgumentOutOfRangeException(nameof(len));
            }
            Pool = pool;
            Length = len.Value;
            LongLength = len.Value;
            _Array = arr;
            if (clean) System.Array.Clear(arr, 0, len.Value);
        }

        /// <inheritdoc/>
        public ArrayPool<T> Pool { get; }

        /// <inheritdoc/>
        public int Length { get; }

        /// <inheritdoc/>
        public long LongLength { get; }

        /// <inheritdoc/>
        public readonly T this[int offset]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => IfUndisposed(_Array[offset]);
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set
            {
                lock (SyncObject)
                {
                    EnsureUndisposed();
                    _Array[offset] = value;
                }
            }
        }

        /// <inheritdoc/>
        public readonly Memory<T> this[Range range]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get
            {
                lock (SyncObject)
                {
                    EnsureUndisposed();
                    return Memory[range];
                }
            }
        }

        /// <inheritdoc/>
        public readonly Memory<T> this[Index start, Index end]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get
            {
                lock (SyncObject)
                {
                    EnsureUndisposed();
                    return Memory[new Range(start, end)];
                }
            }
        }

        /// <inheritdoc/>
        public readonly T[] Array => IfUndisposed(_Array);

        /// <inheritdoc/>
        public readonly Span<T> Span
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array.AsSpan(0, Length);
        }

        /// <inheritdoc/>
        public readonly Memory<T> Memory
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array.AsMemory(0, Length);
        }

        /// <inheritdoc/>
        public bool Clear { get; set; } = false;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public readonly IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_Array).GetEnumerator();

        /// <inheritdoc/>
        readonly IEnumerator IEnumerable.GetEnumerator() => _Array.GetEnumerator();

        /// <summary>
        /// Ensure an undisposed object state
        /// </summary>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private readonly void EnsureUndisposed()
        {
            lock (SyncObject) if (!IsDisposed) return;
            throw new ObjectDisposedException(ToString());
        }

        /// <summary>
        /// Return a value if not disposing/disposed
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private readonly tValue IfUndisposed<tValue>(in tValue value)
        {
            EnsureUndisposed();
            return value;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (SyncObject)
            {
                if (IsDisposed) return;
                IsDisposed = true;
            }
            T[] arr = _Array;
            _Array = System.Array.Empty<T>();
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

        /// <summary>
        /// Cast as array
        /// </summary>
        /// <param name="arr">Array (may be longer than <see cref="Length"/>!)</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator T[](in RentedArrayStruct<T> arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Span<T>(in RentedArrayStruct<T> arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Memory<T>(in RentedArrayStruct<T> arr) => arr.Memory;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(in RentedArrayStruct<T> arr) => arr.Length;

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in RentedArrayStruct<T> left, in RentedArrayStruct<T> right) => left.Equals(right);

        /// <summary>
        /// Not equal
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in RentedArrayStruct<T> left, in RentedArrayStruct<T> right) => !left.Equals(right);
    }
}

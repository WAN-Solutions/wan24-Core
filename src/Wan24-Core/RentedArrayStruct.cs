using System.Buffers;
using System.Collections;
using System.Security.Cryptography;

namespace wan24.Core
{
    /// <summary>
    /// Pool rented array (returns the array to the pool, when disposed)
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
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
        public RentedArrayStruct(int len, ArrayPool<T>? pool = null, bool clean = true)
        {
            if (len < 1) throw new ArgumentOutOfRangeException(nameof(len));
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
        public RentedArrayStruct(ArrayPool<T> pool, T[] arr, int? len = null, bool clean = false)
        {
            len ??= arr.Length;
            if (len < 1 || len > arr.Length)
            {
                pool.Return(arr, clearArray: clean);
                throw new ArgumentOutOfRangeException(nameof(len));
            }
            Pool = pool;
            Length = len.Value;
            _Array = arr;
            if (clean) System.Array.Clear(arr, 0, len ?? arr.Length);
        }

        /// <inheritdoc/>
        public ArrayPool<T> Pool { get; }

        /// <inheritdoc/>
        public int Length { get; }

        /// <inheritdoc/>
        public readonly long LongLength => Length;

        /// <inheritdoc/>
        public readonly T this[int offset]
        {
            get => IfUndisposed(_Array[offset]);
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
        public readonly Span<T> Span => Memory.Span;

        /// <inheritdoc/>
        public readonly Memory<T> Memory => Array.AsMemory(0, Length);

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
        public override readonly bool Equals(object? obj) => Memory.Equals(obj);

        /// <inheritdoc/>
        public readonly bool Equals(Memory<T> other) => Memory.Equals(other);

        /// <inheritdoc/>
        public override readonly int GetHashCode() => Memory.GetHashCode();

        /// <inheritdoc/>
        public readonly IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_Array).GetEnumerator();

        /// <inheritdoc/>
        readonly IEnumerator IEnumerable.GetEnumerator() => _Array.GetEnumerator();

        /// <summary>
        /// Ensure an undisposed object state
        /// </summary>
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
        private readonly tValue IfUndisposed<tValue>(tValue value)
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
                    RandomNumberGenerator.Fill(byteArr.AsSpan(0, Length));
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
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cast as array
        /// </summary>
        /// <param name="arr">Array (may be longer than <see cref="Length"/>!)</param>
        public static implicit operator T[](RentedArrayStruct<T> arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator Span<T>(RentedArrayStruct<T> arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator Memory<T>(RentedArrayStruct<T> arr) => arr.Memory;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator int(RentedArrayStruct<T> arr) => arr.Length;

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals?</returns>
        public static bool operator ==(RentedArrayStruct<T> left, RentedArrayStruct<T> right) => left.Equals(right);

        /// <summary>
        /// Not equal
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equal?</returns>
        public static bool operator !=(RentedArrayStruct<T> left, RentedArrayStruct<T> right) => !left.Equals(right);
    }
}

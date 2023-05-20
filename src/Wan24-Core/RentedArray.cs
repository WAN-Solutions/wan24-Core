using System.Buffers;
using System.Collections;

namespace wan24.Core
{
    /// <summary>
    /// Pool rented array (returns the array to the pool, when disposed)
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public sealed class RentedArray<T> : DisposableBase, IEnumerable<T>, IEnumerable, IEquatable<Memory<byte>>
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
        public RentedArray(int len, ArrayPool<T>? pool = null, bool clean = true) : base()
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
        public RentedArray(ArrayPool<T> pool, T[] arr, int? len = null, bool clean = false) : base()
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

        /// <summary>
        /// Pool
        /// </summary>
        public ArrayPool<T> Pool { get; }

        /// <summary>
        /// Length
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Get/set a byte
        /// </summary>
        /// <param name="offset">Index</param>
        /// <returns>Byte</returns>
        public T this[int offset]
        {
            get => IfUndisposed(_Array[offset]);
            set => IfUndisposed(() => _Array[offset] = value);
        }

        /// <summary>
        /// Get a range
        /// </summary>
        /// <param name="range">Range</param>
        /// <returns>Range memory</returns>
        public Memory<T> this[Range range] => IfUndisposed(() => Memory[range]);

        /// <summary>
        /// Get a range
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        /// <returns>Range memory</returns>
        public Memory<T> this[Index start, Index end] => IfUndisposed(() => Memory[new Range(start, end)]);

        /// <summary>
        /// Array (may be longer than <see cref="Length"/>!)
        /// </summary>
        public T[] Array => IfUndisposed(_Array);

        /// <summary>
        /// Span
        /// </summary>
        public Span<T> Span => Memory.Span;

        /// <summary>
        /// Memory
        /// </summary>
        public Memory<T> Memory => Array.AsMemory(0, Length);

        /// <summary>
        /// Clear the array when returning?
        /// </summary>
        public bool Clear { get; set; }

        /// <summary>
        /// Create a non-rented copy of the array
        /// </summary>
        /// <returns>Copy</returns>
        public T[] GetCopy()
        {
            if (Length == 0) return System.Array.Empty<T>();
            T[] res = new T[Length];
            Span.CopyTo(res.AsSpan(0, Length));
            return res;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Memory.Equals(obj);

        /// <inheritdoc/>
        public bool Equals(Memory<byte> other) => Memory.Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => Memory.GetHashCode();

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_Array).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => _Array.GetEnumerator();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            T[] arr = _Array;
            _Array = System.Array.Empty<T>();
            Pool.Return(arr, clearArray: Clear);
        }

        /// <summary>
        /// Cast as array
        /// </summary>
        /// <param name="arr">Array (may be longer than <see cref="Length"/>!)</param>
        public static implicit operator T[](RentedArray<T> arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator Span<T>(RentedArray<T> arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator Memory<T>(RentedArray<T> arr) => arr.Memory;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator int(RentedArray<T> arr) => arr.Length;
    }
}

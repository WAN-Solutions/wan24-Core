#if !NO_UNSAFE
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

//TODO PinnedArraySimple
//TODO PinnedArrayRef

namespace wan24.Core
{
    /// <summary>
    /// Pinned array
    /// </summary>
    /// <typeparam name="T">Pointer type</typeparam>
    public unsafe readonly struct PinnedArray<T> : IDisposable, IList<T> where T : struct
    {
        /// <summary>
        /// Array pin
        /// </summary>
        private readonly GCHandle ArrayPin;
        /// <summary>
        /// Array
        /// </summary>
        public readonly T[] Array;
        /// <summary>
        /// Array pointer
        /// </summary>
#pragma warning disable CS8500 // Takes the address of, gets the size of, or declares a pointer to a managed type
        public readonly T* ArrayPtr;
#pragma warning restore CS8500 // Takes the address of, gets the size of, or declares a pointer to a managed type

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="array">Array</param>
        public PinnedArray(in T[] array)
        {
            Array = array;
            ArrayPin = GCHandle.Alloc(array, GCHandleType.Pinned);
#pragma warning disable CS8500 // Takes the address of, gets the size of, or declares a pointer to a managed type
            ArrayPtr = (T*)ArrayPin.AddrOfPinnedObject();
#pragma warning restore CS8500 // Takes the address of, gets the size of, or declares a pointer to a managed type
        }

        /// <summary>
        /// Get/set an array element
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Element</returns>
        public T this[int offset]
        {
            get
            {
                if (offset < 0 || offset >= Array.Length) throw new ArgumentOutOfRangeException(nameof(offset));
                return ArrayPtr[offset];
            }
            set
            {
                if (offset < 0 || offset >= Array.Length) throw new ArgumentOutOfRangeException(nameof(offset));
                ArrayPtr[offset] = value;
            }
        }

        /// <summary>
        /// Is disposed?
        /// </summary>
        public bool IsDisposed => !ArrayPin.IsAllocated;

        /// <summary>
        /// Array pointer
        /// </summary>
        public IntPtr Pointer => ArrayPin.AddrOfPinnedObject();

        /// <inheritdoc/>
        public int Count => Array.Length;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public int IndexOf(T item) => Array.IndexOf(item);

        /// <inheritdoc/>
        public void Insert(int index, T item) => ((IList<T>)Array).Insert(index, item);

        /// <inheritdoc/>
        public void RemoveAt(int index) => ((IList<T>)Array).RemoveAt(index);

        /// <inheritdoc/>
        public void Add(T item) => ((ICollection<T>)Array).Add(item);

        /// <inheritdoc/>
        public void Clear() => Array.AsSpan().Clear();

        /// <inheritdoc/>
        public bool Contains(T item) => Array.Contains(item);

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex) => ((ICollection<T>)Array).CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public bool Remove(T item) => ((ICollection<T>)Array).Remove(item);

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Array).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => Array.GetEnumerator();

        /// <inheritdoc/>
        public void Dispose()
        {
            if (ArrayPin.IsAllocated) ArrayPin.Free();
        }

        /// <inheritdoc/>
        public override int GetHashCode() => Array.GetHashCode();

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj) => Array.Equals(obj);

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="pin">Pin</param>
#pragma warning disable CS8500 // Takes the address of, gets the size of, or declares a pointer to a managed type
        public static implicit operator T*(in PinnedArray<T> pin) => pin.ArrayPtr;
#pragma warning restore CS8500 // Takes the address of, gets the size of, or declares a pointer to a managed type

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="pin">Pin</param>
        public static implicit operator IntPtr(in PinnedArray<T> pin) => pin.Pointer;

        /// <summary>
        /// Cast as array
        /// </summary>
        /// <param name="pin">Pin</param>
        public static implicit operator T[](in PinnedArray<T> pin) => pin.Array;

        /// <summary>
        /// Cast as length
        /// </summary>
        /// <param name="pin">Pin</param>
        public static implicit operator int(in PinnedArray<T> pin) => pin.Array.Length;

        /// <summary>
        /// Cast as length
        /// </summary>
        /// <param name="pin">Pin</param>
        public static implicit operator long(in PinnedArray<T> pin) => pin.Array.LongLength;

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Are equal?</returns>
        public static bool operator ==(in PinnedArray<T> a, in PinnedArray<T> b) => a.Equals(b);

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Are not equal?</returns>
        public static bool operator !=(in PinnedArray<T> a, in PinnedArray<T> b) => !(a == b);
    }
}
#endif

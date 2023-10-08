using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Equatable array helper
    /// </summary>
    public static class EquatableArray
    {
        /// <summary>
        /// Create from an array
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="array">Array</param>
        /// <returns>Equatable array</returns>
        public static EquatableArray<T> From<T>(T[] array) => array;

        /// <summary>
        /// Get as equatable array
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="array">Array</param>
        /// <returns>Equatable array</returns>
        public static EquatableArray<T> AsEquatableArray<T>(this T[] array) => array;
    }

    /// <summary>
    /// Equatable array (validates sequence validity, not object references)
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
    public readonly struct EquatableArray<T> : ICloneable, IList, IStructuralComparable, IStructuralEquatable, IEquatable<T[]>, IEquatable<EquatableArray<T>>
    {
        /// <summary>
        /// Hosted array
        /// </summary>
        public readonly T[] Array;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="array">Hosted array</param>
        public EquatableArray(T[] array) => Array = array;

        /// <inheritdoc/>
        public T this[int index]
        {
            get => Array[index];
            set => Array[index] = value;
        }

        /// <inheritdoc/>
        public T this[Index index]
        {
            get => Array[index];
            set => Array[index] = value;
        }

        /// <inheritdoc/>
        object? IList.this[int index] 
        { 
            get => Array[index];
            set
            {
                if (value is not T v) throw new ArgumentException("Element type mismatch", nameof(value));
                Array[index] = v;
            }
        }

        /// <inheritdoc/>
        public bool IsFixedSize => true;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        int ICollection.Count => Array.Length;

        /// <summary>
        /// Length as 32 bit integer
        /// </summary>
        public int Length => Array.Length;

        /// <summary>
        /// Length as 64 bit integer
        /// </summary>
        public long LongLength => Array.LongLength;

        /// <inheritdoc/>
        public bool IsSynchronized => false;

        /// <inheritdoc/>
        public object SyncRoot => Array.SyncRoot;

        /// <inheritdoc/>
        public override int GetHashCode() => Array.GetHashCode();

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is not null && obj is T[] arr && arr.Length == Array.Length && arr.SequenceEqual(Array);

        /// <inheritdoc/>
        public bool Equals([NotNullWhen(true)] T[]? other) => other is not null && other.Length == Array.Length && other.SequenceEqual(Array);

        /// <inheritdoc/>
        public bool Equals(EquatableArray<T> other) => other.Array.Length == Array.Length && other.Array.SequenceEqual(Array);

        /// <inheritdoc/>
        int IList.Add(object? value) => ((IList)Array).Add(value);

        /// <inheritdoc/>
        public void Clear()
        {
            if (Array is byte[] bytes) bytes.Clear();
            else if (Array is char[] characters) characters.Clear();
            else Array.AsSpan().Clear();
        }

        /// <summary>
        /// Get a clone of the hosted array
        /// </summary>
        /// <returns>Clone</returns>
        public T[] CloneArray() => Array.CloneArray();

        /// <inheritdoc/>
        object ICloneable.Clone() => Array.CloneArray();

        /// <inheritdoc/>
        public int CompareTo(object? other, IComparer comparer) => ((IStructuralComparable)Array).CompareTo(other, comparer);

        /// <summary>
        /// Determine if a value is contained
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>If contained</returns>
        public bool Contains(T value) => Array.Contains(value);

        /// <inheritdoc/>
        public bool Contains(object? value) => value is T v && Array.Contains(v);

        /// <inheritdoc/>
        public void CopyTo(Array array, int index) => Array.CopyTo(array, index);

        /// <inheritdoc/>
        public bool Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer) => ((IStructuralEquatable)Array).Equals(other, comparer);

        /// <inheritdoc/>
        public IEnumerator GetEnumerator() => Array.GetEnumerator();

        /// <inheritdoc/>
        public int GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)Array).GetHashCode(comparer);

        /// <summary>
        /// Get the index of a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Index or <c>-1</c>, if not found</returns>
        public int IndexOf(T value) => Array.IndexOf(value);

        /// <inheritdoc/>
        public int IndexOf(object? value) => value is T v ? Array.IndexOf(v) : -1;

        /// <inheritdoc/>
        void IList.Insert(int index, object? value) => ((IList)Array).Insert(index, value);

        /// <inheritdoc/>
        void IList.Remove(object? value) => ((IList)Array).Remove(value);

        /// <inheritdoc/>
        void IList.RemoveAt(int index) => ((IList)Array).RemoveAt(index);

        /// <summary>
        /// Cast as array
        /// </summary>
        /// <param name="arr">Equatable array</param>
        public static implicit operator T[](EquatableArray<T> arr) => arr.Array;

        /// <summary>
        /// Cast as equatable array
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator EquatableArray<T>(T[] arr) => new(arr);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If equals</returns>
        public static bool operator ==(EquatableArray<T> a, EquatableArray<T> b) => a.Equals(b);

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If not equals</returns>
        public static bool operator !=(EquatableArray<T> a, EquatableArray<T> b) => !(a == b);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If equals</returns>
        public static bool operator ==(EquatableArray<T> a, T[] b) => a.Equals(b);

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If not equals</returns>
        public static bool operator !=(EquatableArray<T> a, T[] b) => !(a == b);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If equals</returns>
        public static bool operator ==(T[] a, EquatableArray<T> b) => b.Equals(a);

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If not equals</returns>
        public static bool operator !=(T[] a, EquatableArray<T> b) => !(a == b);
    }
}

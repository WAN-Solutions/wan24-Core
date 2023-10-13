using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;

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
        [TargetedPatchingOptOut("Just a method adapter")]
        public static EquatableArray<T> From<T>(in T[] array) => new(array);

        /// <summary>
        /// Get as equatable array
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="array">Array</param>
        /// <returns>Equatable array</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static EquatableArray<T> AsEquatableArray<T>(this T[] array) => new(array);
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
        public EquatableArray(in T[] array) => Array = array;

        /// <inheritdoc/>
        public T this[int index]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
            get => Array[index];
            [TargetedPatchingOptOut("Just a method adapter")]
            set => Array[index] = value;
        }

        /// <inheritdoc/>
        public T this[Index index]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
            get => Array[index];
            [TargetedPatchingOptOut("Just a method adapter")]
            set => Array[index] = value;
        }

        /// <inheritdoc/>
        object? IList.this[int index] 
        {
            [TargetedPatchingOptOut("Just a method adapter")]
            get => Array[index];
            [TargetedPatchingOptOut("Just a method adapter")]
            set
            {
                if (value is not T v) throw new ArgumentException("Element type mismatch", nameof(value));
                Array[index] = v;
            }
        }

        /// <inheritdoc/>
        public bool IsFixedSize
        {
            [TargetedPatchingOptOut("Tiny method")]
            get => true;
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            [TargetedPatchingOptOut("Tiny method")]
            get => false;
        }

        /// <inheritdoc/>
        int ICollection.Count
        {
            [TargetedPatchingOptOut("Just a method adapter")]
            get => Array.Length;
        }

        /// <summary>
        /// Length as 32 bit integer
        /// </summary>
        public int Length
        {
            [TargetedPatchingOptOut("Just a method adapter")]
            get => Array.Length;
        }

        /// <summary>
        /// Length as 64 bit integer
        /// </summary>
        public long LongLength
        {
            [TargetedPatchingOptOut("Just a method adapter")]
            get => Array.LongLength;
        }

        /// <inheritdoc/>
        public bool IsSynchronized
        {
            [TargetedPatchingOptOut("Tiny method")]
            get => false;
        }

        /// <inheritdoc/>
        public object SyncRoot
        {
            [TargetedPatchingOptOut("Just a method adapter")]
            get => Array.SyncRoot;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override int GetHashCode() => Array.GetHashCode();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is not null && 
            (
                (obj is EquatableArray<T> eArr && eArr.Array.Length == Array.Length && eArr.Array.SequenceEqual(Array)) ||
                (obj is T[] arr && arr.Length == Array.Length && arr.SequenceEqual(Array))
            );

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public bool Equals([NotNullWhen(true)] T[]? other) => other is not null && other.Length == Array.Length && other.SequenceEqual(Array);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public bool Equals(EquatableArray<T> other) => other.Array.Length == Array.Length && other.Array.SequenceEqual(Array);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        int IList.Add(object? value) => ((IList)Array).Add(value);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
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
        [TargetedPatchingOptOut("Just a method adapter")]
        public T[] CloneArray() => Array.CloneArray();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        object ICloneable.Clone() => Array.CloneArray();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public int CompareTo(object? other, IComparer comparer) => ((IStructuralComparable)Array).CompareTo(other, comparer);

        /// <summary>
        /// Determine if a value is contained
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>If contained</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public bool Contains(in T value) => Array.Contains(value);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public bool Contains(object? value) => value is T v && Array.Contains(v);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public void CopyTo(Array array, int index) => Array.CopyTo(array, index);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public bool Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer) => ((IStructuralEquatable)Array).Equals(other, comparer);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public IEnumerator GetEnumerator() => Array.GetEnumerator();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public int GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)Array).GetHashCode(comparer);

        /// <summary>
        /// Get the index of a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Index or <c>-1</c>, if not found</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public int IndexOf(in T value) => Array.IndexOf(value);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public int IndexOf(object? value) => value is T v ? Array.IndexOf(v) : -1;

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        void IList.Insert(int index, object? value) => ((IList)Array).Insert(index, value);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        void IList.Remove(object? value) => ((IList)Array).Remove(value);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        void IList.RemoveAt(int index) => ((IList)Array).RemoveAt(index);

        /// <summary>
        /// Cast as array
        /// </summary>
        /// <param name="arr">Equatable array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator T[](in EquatableArray<T> arr) => arr.Array;

        /// <summary>
        /// Cast as equatable array
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator EquatableArray<T>(in T[] arr) => new(arr);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If equals</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in EquatableArray<T> a, in EquatableArray<T> b) => a.Equals(b);

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If not equals</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in EquatableArray<T> a, in EquatableArray<T> b) => !(a == b);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If equals</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in EquatableArray<T> a, in T[] b) => a.Equals(b);

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If not equals</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in EquatableArray<T> a, in T[] b) => !(a == b);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If equals</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in T[] a, in EquatableArray<T> b) => b.Equals(a);

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If not equals</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in T[] a, in EquatableArray<T> b) => !(a == b);
    }
}

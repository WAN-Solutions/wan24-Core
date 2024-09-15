using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Equatable array helper
    /// </summary>
    public static class EquatableImmutableArray
    {
        /// <summary>
        /// Create from an array
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="array">Array</param>
        /// <returns>Equatable array</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static EquatableImmutableArray<T> From<T>(in ImmutableArray<T> array) => new(array);

        /// <summary>
        /// Get as equatable array
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="array">Array</param>
        /// <returns>Equatable array</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static EquatableImmutableArray<T> AsEquatableArray<T>(this ImmutableArray<T> array) => new(array);
    }

    /// <summary>
    /// Equatable array (validates sequence validity, not object references)
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct EquatableImmutableArray<T> : IReadOnlyList<T>, IStructuralComparable, IStructuralEquatable, IEquatable<T[]>, IEquatable<EquatableImmutableArray<T>>
    {
        /// <summary>
        /// Hash code
        /// </summary>
        public readonly int HashCode;
        /// <summary>
        /// Hosted array
        /// </summary>
        public readonly ImmutableArray<T> Array;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="array">Hosted array</param>
        public EquatableImmutableArray(in ImmutableArray<T> array)
        {
            Array = array;
            int hashCode = array.Length.GetHashCode();
            for (int i = 0, len = array.Length; i < len; hashCode ^= array[i]?.GetHashCode() ?? 0, i++) ;
            HashCode = hashCode;
        }

        /// <inheritdoc/>
        public T this[int index]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
            get => Array[index];
        }

        /// <inheritdoc/>
        public T this[Index index]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
            get => Array[index];
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
            get => true;
        }

        /// <summary>
        /// Length as 32 bit integer
        /// </summary>
        public int Length
        {
            [TargetedPatchingOptOut("Just a method adapter")]
            get => Array.Length;
        }

        /// <inheritdoc/>
        public bool IsSynchronized
        {
            [TargetedPatchingOptOut("Tiny method")]
            get => true;
        }

        /// <inheritdoc/>
        public object SyncRoot
        {
            [TargetedPatchingOptOut("Just a method adapter")]
            get => Array;
        }

        /// <inheritdoc/>
        int IReadOnlyCollection<T>.Count => Array.Length;

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override int GetHashCode() => HashCode;

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is not null &&
            (
                (obj is EquatableImmutableArray<T> eArr && eArr.Array.Length == Array.Length && eArr.Array.SequenceEqual(Array)) ||
                (obj is T[] arr && arr.Length == Array.Length && arr.SequenceEqual(Array))
            );

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public bool Equals([NotNullWhen(true)] T[]? other) => other is not null && other.Length == Array.Length && other.SequenceEqual(Array);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public bool Equals(EquatableImmutableArray<T> other) => other.Array.Length == Array.Length && other.Array.SequenceEqual(Array);

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
        public bool Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer) => ((IStructuralEquatable)Array).Equals(other, comparer);

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
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)Array).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Array).GetEnumerator();

        /// <summary>
        /// Cast as array
        /// </summary>
        /// <param name="arr">Equatable array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator ImmutableArray<T>(in EquatableImmutableArray<T> arr) => arr.Array;

        /// <summary>
        /// Cast as equatable array
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator EquatableImmutableArray<T>(in ImmutableArray<T> arr) => new(arr);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If equals</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in EquatableImmutableArray<T> a, in EquatableImmutableArray<T> b) => a.Equals(b);

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If not equals</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in EquatableImmutableArray<T> a, in EquatableImmutableArray<T> b) => !(a == b);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If equals</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in EquatableImmutableArray<T> a, in ImmutableArray<T> b) => a.Equals(b);

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If not equals</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in EquatableImmutableArray<T> a, in ImmutableArray<T> b) => !(a == b);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If equals</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in ImmutableArray<T> a, in EquatableImmutableArray<T> b) => b.Equals(a);

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If not equals</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in ImmutableArray<T> a, in EquatableImmutableArray<T> b) => !(a == b);

        /// <summary>
        /// Equality comparer
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        public sealed class EqualityComparer() : IEqualityComparer<EquatableImmutableArray<T>>
        {
            /// <inheritdoc/>
            public bool Equals(EquatableImmutableArray<T> x, EquatableImmutableArray<T> y) => x == y;

            /// <inheritdoc/>
            public int GetHashCode([DisallowNull] EquatableImmutableArray<T> obj) => obj.HashCode;
        }
    }
}

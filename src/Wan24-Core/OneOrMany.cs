using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// One or many items (can cast implicit from/to a single or all items; casts from <see langword="null"/> to an empty structure; casts from an empty structure to 
    /// <see langword="default"/>)
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    [StructLayout(LayoutKind.Auto)]
    public readonly struct OneOrMany<T>
        : ICollection, IEnumerable, IList, IStructuralComparable, IStructuralEquatable, ICloneable, ICollection<T>, IEnumerable<T>, IList<T>
    {
        /// <summary>
        /// If <see cref="Items"/> is an empty array
        /// </summary>
        public readonly bool IsEmpty;
        /// <summary>
        /// If <see cref="Items"/> has exactly one item
        /// </summary>
        public readonly bool IsOne;
        /// <summary>
        /// If <see cref="Items"/> has many (=more than one) items
        /// </summary>
        public readonly bool IsMany;
        /// <summary>
        /// Items
        /// </summary>
        public readonly T[] Items;

        /// <summary>
        /// Constructor
        /// </summary>
        public OneOrMany()
        {
            Items = [];
            IsEmpty = true;
            IsOne = false;
            IsMany = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Items</param>
        public OneOrMany(params T[] items)
        {
            Contract.Assert(items is not null);
            Items = items;
            switch (items.Length)
            {
                case 0:
                    IsEmpty = true;
                    IsOne = false;
                    IsMany = false;
                    break;
                case 1:
                    IsEmpty = false;
                    IsOne = true;
                    IsMany = false;
                    break;
                default:
                    IsEmpty = false;
                    IsOne = false;
                    IsMany = true;
                    break;
            }
        }

        /// <inheritdoc/>
        object? IList.this[int index]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Items[index];
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set => ((IList)Items)[index] = value;
        }

        /// <inheritdoc/>
        public T this[int index]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Items[index];
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set => Items[index] = value;
        }

        /// <inheritdoc/>
        public int Count
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Items.Length;
        }

        /// <summary>
        /// <see cref="Items"/> <see cref="Array.Length"/>
        /// </summary>
        public int Length
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Items.Length;
        }

        /// <summary>
        /// <see cref="Items"/> <see cref="Array.LongLength"/>
        /// </summary>
        public long LongLength
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Items.LongLength;
        }

        /// <inheritdoc/>
        public bool IsSynchronized => false;

        /// <inheritdoc/>
        public object SyncRoot
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Items.SyncRoot;
        }

        /// <inheritdoc/>
        public bool IsFixedSize => true;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <summary>
        /// Throw an exception, if <see cref="IsEmpty"/>
        /// </summary>
        /// <returns><see langword="false"/></returns>
        /// <exception cref="InvalidOperationException"><see cref="IsEmpty"/></exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool ThrowIfEmpty() => IsEmpty ? throw new InvalidOperationException("Structure is empty") : false;

        /// <summary>
        /// Ensure not <see cref="IsEmpty"/>
        /// </summary>
        /// <param name="throwIfEmpty">If to throw an exception, if <see cref="IsEmpty"/></param>
        /// <returns>If not <see cref="IsEmpty"/></returns>
        /// <exception cref="InvalidOperationException">If <see cref="IsEmpty"/> and <c>throwIfEmpty</c> was <see langword="true"/></exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool EnsureNotEmpty(in bool throwIfEmpty = true) => !(throwIfEmpty ? ThrowIfEmpty() : IsEmpty);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        int IList.Add(object? value) => ((IList)Items).Add(value);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Clear() => Array.Clear(Items);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        object ICloneable.Clone() => new OneOrMany<T>((T[])Items.Clone());

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int CompareTo(object? other, IComparer comparer) => ((IStructuralComparable)Items).CompareTo(other, comparer);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Contains(object? value) => ((IList)Items).Contains(value);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void CopyTo(Array array, int index) => Items.CopyTo(array, index);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Equals(object? other, IEqualityComparer comparer) => ((IStructuralEquatable)Items).Equals(other, comparer);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)Items).GetHashCode(comparer);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int IndexOf(object? value) => ((IList)Items).IndexOf(value);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        void IList.Insert(int index, object? value) => ((IList)Items).Insert(index, value);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        void IList.Remove(object? value) => ((IList)Items).Remove(value);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        void IList.RemoveAt(int index) => ((IList)Items).RemoveAt(index);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        void ICollection<T>.Add(T item) => ((ICollection<T>)Items).Add(item);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Contains(T item) => Items.Contains(item);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        bool ICollection<T>.Remove(T item) => ((ICollection<T>)Items).Remove(item);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        void IList<T>.RemoveAt(int index) => ((IList<T>)Items).RemoveAt(index);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Items).GetEnumerator();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int IndexOf(T item) => Items.IndexOf(item);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        void IList<T>.Insert(int index, T item) => ((IList<T>)Items).Insert(index, item);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override bool Equals([NotNullWhen(returnValue: true)] object? obj) => Items.Equals(obj);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override int GetHashCode() => Items.GetHashCode();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override string? ToString() => Items.ToString();

        /// <summary>
        /// Cast as the first item or the <see langword="default"/>
        /// </summary>
        /// <param name="oom"><see cref="OneOrMany{T}"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator T?(in OneOrMany<T> oom) => oom.IsEmpty ? default : oom.Items[0];

        /// <summary>
        /// Cast as items
        /// </summary>
        /// <param name="oom"><see cref="OneOrMany{T}"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator T[](in OneOrMany<T> oom) => oom.Items;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="oom"><see cref="OneOrMany{T}"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Span<T>(in OneOrMany<T> oom) => oom.Items;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="oom"><see cref="OneOrMany{T}"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ReadOnlySpan<T>(in OneOrMany<T> oom) => oom.Items;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="oom"><see cref="OneOrMany{T}"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Memory<T>(in OneOrMany<T> oom) => oom.Items;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="oom"><see cref="OneOrMany{T}"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ReadOnlyMemory<T>(in OneOrMany<T> oom) => oom.Items;

        /// <summary>
        /// Cast from an item
        /// </summary>
        /// <param name="item">Item</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator OneOrMany<T>(in T item) => item is null ? [] : new(item);

        /// <summary>
        /// Cast from items
        /// </summary>
        /// <param name="items">Items</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator OneOrMany<T>(in T[] items) => new(items);

        /// <summary>
        /// Cast from items
        /// </summary>
        /// <param name="items">Items</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator OneOrMany<T>(in Span<T> items) => new([.. items]);

        /// <summary>
        /// Cast from items
        /// </summary>
        /// <param name="items">Items</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator OneOrMany<T>(in ReadOnlySpan<T> items) => new([.. items]);

        /// <summary>
        /// Cast from items
        /// </summary>
        /// <param name="items">Items</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator OneOrMany<T>(in Memory<T> items) => new([.. items.Span]);

        /// <summary>
        /// Cast from items
        /// </summary>
        /// <param name="items">Items</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator OneOrMany<T>(in ReadOnlyMemory<T> items) => new([.. items.Span]);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>If equal</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool operator ==(in OneOrMany<T> left, in OneOrMany<T> right) => left.Equals(right);

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>If not equal</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool operator !=(in OneOrMany<T> left, in OneOrMany<T> right) => !(left == right);

        /// <summary>
        /// Is lower than
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>If lower</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool operator <(in OneOrMany<T> left, in OneOrMany<T> right) => left.Items.Length < right.Items.Length;

        /// <summary>
        /// Is greater than
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>If greater</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool operator >(in OneOrMany<T> left, in OneOrMany<T> right) => left.Items.Length > right.Items.Length;
    }
}

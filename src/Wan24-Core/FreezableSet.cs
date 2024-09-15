using System.Collections;
using System.Collections.Frozen;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Freezable set
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    public class FreezableSet<T> : ISet<T>, IReadOnlySet<T>, IReadOnlyCollection<T>
    {
        /// <summary>
        /// Initial capacity
        /// </summary>
        protected readonly int InitialCapacity = 0;
        /// <summary>
        /// Set
        /// </summary>
        protected readonly HashSet<T> Set;
        /// <summary>
        /// Frozen set
        /// </summary>
        protected FrozenSet<T>? _Frozen = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public FreezableSet() => Set = [];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public FreezableSet(in int capacity)
        {
            InitialCapacity = capacity;
            Set = new(capacity);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Items</param>
        public FreezableSet(in IEnumerable<T> items) => Set = new(items);

        /// <summary>
        /// Frozen set
        /// </summary>
        public FrozenSet<T>? Frozen => _Frozen;

        /// <summary>
        /// If frozen
        /// </summary>
        public bool IsFrozen
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen is not null;
        }

        /// <inheritdoc/>
        public int Count
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen?.Count ?? Set.Count;
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen is not null;
        }

        /// <summary>
        /// Freeze
        /// </summary>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Freeze()
        {
            if (_Frozen is not null) return;
            _Frozen = Set.ToFrozenSet();
            Set.Clear();
        }

        /// <summary>
        /// Unfreeze
        /// </summary>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Unfreeze()
        {
            if (_Frozen is null) return;
            Set.EnsureCapacity(Math.Max(InitialCapacity, _Frozen.Count));
            Set.AddRange(_Frozen);
            _Frozen = null;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Add(T item)
        {
            EnsureNotFrozen();
            return Set.Add(item);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Clear()
        {
            EnsureNotFrozen();
            Set.Clear();
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Contains(T item) => _Frozen?.Contains(item) ?? Set.Contains(item);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (_Frozen is null)
            {
                Set.CopyTo(array, arrayIndex);
            }
            else
            {
                _Frozen.CopyTo(array, arrayIndex);
            }
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void ExceptWith(IEnumerable<T> other)
        {
            EnsureNotFrozen();
            Set.ExceptWith(other);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public IEnumerator<T> GetEnumerator() => (_Frozen as IEnumerable<T>)?.GetEnumerator() ?? Set.GetEnumerator();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void IntersectWith(IEnumerable<T> other)
        {
            EnsureNotFrozen();
            Set.IntersectWith(other);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool IsProperSubsetOf(IEnumerable<T> other) => _Frozen?.IsProperSubsetOf(other) ?? Set.IsProperSubsetOf(other);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool IsProperSupersetOf(IEnumerable<T> other) => _Frozen?.IsProperSupersetOf(other) ?? Set.IsProperSupersetOf(other);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool IsSubsetOf(IEnumerable<T> other) => _Frozen?.IsSubsetOf(other) ?? Set.IsSubsetOf(other);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool IsSupersetOf(IEnumerable<T> other) => _Frozen?.IsSupersetOf(other) ?? Set.IsSupersetOf(other);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Overlaps(IEnumerable<T> other) => _Frozen?.Overlaps(other) ?? Set.Overlaps(other);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Remove(T item)
        {
            EnsureNotFrozen();
            return Set.Remove(item);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool SetEquals(IEnumerable<T> other) => _Frozen?.SetEquals(other) ?? Set.SetEquals(other);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            EnsureNotFrozen();
            Set.SymmetricExceptWith(other);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void UnionWith(IEnumerable<T> other)
        {
            EnsureNotFrozen();
            Set.UnionWith(other);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        void ICollection<T>.Add(T item)
        {
            EnsureNotFrozen();
            ((ICollection<T>)Set).Add(item);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Ensure not frozen
        /// </summary>
        /// <param name="throwIfFrozen">If to throw an exception, if frozen</param>
        /// <returns>If not frozen</returns>
        /// <exception cref="InvalidOperationException">Is frozen</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected bool EnsureNotFrozen(in bool throwIfFrozen = true)
        {
            if (_Frozen is null) return true;
            if (throwIfFrozen) throw new InvalidOperationException();
            return false;
        }

        /// <summary>
        /// Cast as <see cref="Count"/>
        /// </summary>
        /// <param name="set">Set</param>
        public static implicit operator int(in FreezableSet<T> set) => set.Count;
    }
}

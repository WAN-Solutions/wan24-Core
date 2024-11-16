using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime;
using System.Diagnostics;

namespace wan24.Core
{
    /// <summary>
    /// Freezable list
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public class FreezableList<T>
        : ICollection<T>,
            IEnumerable<T>,
            IEnumerable,
            IList<T>,
            IReadOnlyCollection<T>,
            IReadOnlyList<T>,
            ICollection,
            IList
    {
        /// <summary>
        /// Initial capacity
        /// </summary>
        protected readonly int InitialCapacity = 0;
        /// <summary>
        /// List
        /// </summary>
        protected readonly List<T> List;
        /// <summary>
        /// Frozen list
        /// </summary>
        protected ImmutableArray<T>? _Frozen = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public FreezableList() => List = [];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Items</param>
        public FreezableList(in IEnumerable<T> items) => List = new(items);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public FreezableList(in int capacity)
        {
            InitialCapacity = capacity;
            List = new(capacity);
        }

        /// <inheritdoc/>
        public T this[int index]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen.HasValue ? _Frozen.Value[index] : List[index];
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set
            {
                EnsureNotFrozen();
                List[index] = value;
            }
        }

        /// <inheritdoc/>
        object? IList.this[int index]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => this[index];
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set
            {
                EnsureNotFrozen();
                ((IList)List)[index] = value;
            }
        }

        /// <summary>
        /// Frozen
        /// </summary>
        public ImmutableArray<T>? Frozen => _Frozen;

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
            get => _Frozen?.Length ?? List.Count;
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

        /// <inheritdoc/>
        public bool IsSynchronized
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen is not null;
        }

        /// <inheritdoc/>
        public object SyncRoot
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (_Frozen as IList)?.SyncRoot ?? ((IList)List).SyncRoot;
        }

        /// <inheritdoc/>
        public bool IsFixedSize
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
            _Frozen = List.ToImmutableArray();
            List.Clear();
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
            List.Capacity = Math.Max(InitialCapacity, _Frozen.Value.Length);
            List.AddRange(_Frozen);
            _Frozen = null;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Add(T item)
        {
            EnsureNotFrozen();
            List.Add(item);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int Add(object? value)
        {
            EnsureNotFrozen();
            return ((IList)List).Add(value);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Clear()
        {
            EnsureNotFrozen();
            List.Clear();
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Contains(T item) => _Frozen?.Contains(item) ?? List.Contains(item);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Contains(object? value) => (_Frozen as IList)?.Contains(value) ?? ((IList)List).Contains(value);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (_Frozen is null)
            {
                List.CopyTo(array, arrayIndex);
            }
            else
            {
                ((ICollection<T>)_Frozen).CopyTo(array, arrayIndex);
            }
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void CopyTo(Array array, int index)
        {
            if (_Frozen is null)
            {
                ((ICollection)List).CopyTo(array, index);
            }
            else
            {
                ((ICollection)_Frozen).CopyTo(array, index);
            }
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public IEnumerator<T> GetEnumerator() => (_Frozen as IEnumerable<T>)?.GetEnumerator() ?? List.GetEnumerator();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int IndexOf(T item) => _Frozen?.IndexOf(item) ?? List.IndexOf(item);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int IndexOf(object? value) => (_Frozen as IList)?.IndexOf(value) ?? ((IList)List).IndexOf(value);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Insert(int index, T item)
        {
            EnsureNotFrozen();
            List.Insert(index, item);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Insert(int index, object? value)
        {
            EnsureNotFrozen();
            ((IList)List).Insert(index, value);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Remove(T item)
        {
            EnsureNotFrozen();
            return List.Remove(item);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Remove(object? value)
        {
            EnsureNotFrozen();
            ((IList)List).Remove(value);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void RemoveAt(int index)
        {
            EnsureNotFrozen();
            List.RemoveAt(index);
        }

        /// <summary>
        /// Remove a range
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        public void RemoveRange(int index, int count)
        {
            EnsureNotFrozen();
            List.RemoveRange(index, count);
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
        /// <param name="list">List</param>
        public static implicit operator int(in FreezableList<T> list) => list.Count;
    }
}

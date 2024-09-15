using System.Collections;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Freezable dictionary
    /// </summary>
    /// <typeparam name="tKey">Key type</typeparam>
    /// <typeparam name="tValue">Value type</typeparam>
    public class FreezableDictionary<tKey, tValue> : IDictionary<tKey, tValue>, IReadOnlyDictionary<tKey, tValue>, IDictionary
        where tKey : notnull
    {
        /// <summary>
        /// Dictionary
        /// </summary>
        protected readonly Dictionary<tKey, tValue> Dictionary;
        /// <summary>
        /// Initial capacity
        /// </summary>
        protected readonly int InitialCapacity = 0;
        /// <summary>
        /// Frozen dictionary
        /// </summary>
        protected FrozenDictionary<tKey, tValue>? _Frozen = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public FreezableDictionary() => Dictionary = [];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public FreezableDictionary(in int capacity)
        {
            InitialCapacity = capacity;
            Dictionary = new(capacity);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Items</param>
        public FreezableDictionary(in IEnumerable<KeyValuePair<tKey, tValue>> items) => Dictionary = new(items);

        /// <inheritdoc/>
        public tValue this[tKey key]
        {
            get => _Frozen is null? Dictionary[key] : _Frozen[key];
            set
            {
                EnsureNotFrozen();
                Dictionary[key] = value;
            }
        }

        /// <summary>
        /// Frozen dictionary
        /// </summary>
        public FrozenDictionary<tKey, tValue>? Frozen => _Frozen;

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
        public ICollection<tKey> Keys
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (ICollection<tKey>?)_Frozen?.Keys ?? Dictionary.Keys;
        }

        /// <inheritdoc/>
        public ICollection<tValue> Values
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (ICollection<tValue>?)_Frozen?.Values ?? Dictionary.Values;
        }

        /// <inheritdoc/>
        public int Count
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen?.Count ?? Dictionary.Count;
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
        IEnumerable<tKey> IReadOnlyDictionary<tKey, tValue>.Keys
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Keys;
        }

        /// <inheritdoc/>
        IEnumerable<tValue> IReadOnlyDictionary<tKey, tValue>.Values
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Values;
        }

        /// <inheritdoc/>
        bool IDictionary.IsFixedSize
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen is not null;
        }

        /// <inheritdoc/>
        ICollection IDictionary.Keys
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (ICollection?)_Frozen?.Keys ?? Dictionary.Keys;
        }

        /// <inheritdoc/>
        ICollection IDictionary.Values
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (ICollection?)_Frozen?.Values ?? Dictionary.Values;
        }

        /// <inheritdoc/>
        bool ICollection.IsSynchronized
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen is not null;
        }

        /// <inheritdoc/>
        object ICollection.SyncRoot
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (_Frozen as ICollection)?.SyncRoot ?? ((ICollection)Dictionary).SyncRoot;
        }

        /// <inheritdoc/>
        object? IDictionary.this[object key]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen is null ? ((IDictionary)Dictionary)[key] : ((IDictionary)_Frozen)[key];
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set
            {
                EnsureNotFrozen();
                ((IDictionary)Dictionary)[key] = value;
            }
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
            _Frozen = Dictionary.ToFrozenDictionary();
            Dictionary.Clear();
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
            Dictionary.EnsureCapacity(Math.Max(InitialCapacity, _Frozen.Count));
            Dictionary.AddRange(_Frozen);
            _Frozen = null;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Add(tKey key, tValue value)
        {
            EnsureNotFrozen();
            Dictionary.Add(key, value);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Add(KeyValuePair<tKey, tValue> item)
        {
            EnsureNotFrozen();
            ((IDictionary<tKey, tValue>)Dictionary).Add(item);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Clear()
        {
            EnsureNotFrozen();
            Dictionary.Clear();
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Contains(KeyValuePair<tKey, tValue> item) => _Frozen?.Contains(item) ?? Dictionary.Contains(item);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool ContainsKey(tKey key) => _Frozen?.ContainsKey(key) ?? Dictionary.ContainsKey(key);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void CopyTo(KeyValuePair<tKey, tValue>[] array, int arrayIndex)
        {
            if (_Frozen is null)
            {
                ((IDictionary<tKey, tValue>)Dictionary).CopyTo(array, arrayIndex);
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
        public IEnumerator<KeyValuePair<tKey, tValue>> GetEnumerator() => (_Frozen as IEnumerable<KeyValuePair<tKey, tValue>>)?.GetEnumerator() ?? Dictionary.GetEnumerator();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Remove(tKey key)
        {
            EnsureNotFrozen();
            return Dictionary.Remove(key);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Remove(KeyValuePair<tKey, tValue> item)
        {
            EnsureNotFrozen();
            return ((IDictionary<tKey, tValue>)Dictionary).Remove(item);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool TryGetValue(tKey key, [MaybeNullWhen(false)] out tValue value) => _Frozen is null ? Dictionary.TryGetValue(key, out value) : _Frozen.TryGetValue(key, out value);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        void IDictionary.Add(object key, object? value)
        {
            EnsureNotFrozen();
            ((IDictionary)Dictionary).Add(key, value);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        bool IDictionary.Contains(object key) => (_Frozen as IDictionary)?.Contains(key) ?? ((IDictionary)Dictionary).Contains(key);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        IDictionaryEnumerator IDictionary.GetEnumerator() => (_Frozen as IDictionary)?.GetEnumerator() ?? Dictionary.GetEnumerator();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        void IDictionary.Remove(object key)
        {
            EnsureNotFrozen();
            ((IDictionary)Dictionary).Remove(key);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        void ICollection.CopyTo(Array array, int index)
        {
            if (_Frozen is null)
            {
                ((ICollection)Dictionary).CopyTo(array, index);
            }
            else
            {
                ((ICollection)_Frozen).CopyTo(array, index);
            }
        }

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
        /// <param name="dict">Dictionary</param>
        public static implicit operator int(in FreezableDictionary<tKey, tValue> dict) => dict.Count;
    }
}

using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace wan24.Core
{
    /// <summary>
    /// Freezable ordered dictionary
    /// </summary>
    /// <typeparam name="tKey">Key type</typeparam>
    /// <typeparam name="tValue">Value type</typeparam>
    public class FreezableOrderedDictionary<tKey, tValue> : OrderedDictionary<tKey, tValue>, IDictionary, IReadOnlyDictionary<tKey, tValue>
        where tKey : notnull
    {
        /// <summary>
        /// Initial capacity
        /// </summary>
        protected readonly int InitialCapacity = 0;
        /// <summary>
        /// Frozen key/value pairs
        /// </summary>
        protected ImmutableArray<KeyValuePair<tKey, tValue>>? _Frozen = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public FreezableOrderedDictionary() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public FreezableOrderedDictionary(in int capacity) : base(capacity) => InitialCapacity = capacity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Items</param>
        public FreezableOrderedDictionary(in IEnumerable<KeyValuePair<tKey, tValue>> items) : base(items) { }

        /// <summary>
        /// Frozen items
        /// </summary>
        public ImmutableArray<KeyValuePair<tKey, tValue>>? Frozen => _Frozen;

        /// <summary>
        /// If frozen
        /// </summary>
        public bool IsFrozen
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen.HasValue;
        }

        /// <inheritdoc/>
        public override ICollection<tKey> Keys
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen?.Select(i => i.Key).ToArray() ?? Items.Select(i => i.Key).ToArray();
        }

        /// <inheritdoc/>
        public override ICollection<tValue> Values
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen?.Select(i => i.Value).ToArray() ?? Items.Select(i => i.Value).ToArray();
        }

        /// <inheritdoc/>
        ICollection IDictionary.Keys
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen?.Select(i => i.Key).ToArray() ?? Items.Select(i => i.Key).ToArray();
        }

        /// <inheritdoc/>
        ICollection IDictionary.Values
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen?.Select(i => i.Value).ToArray() ?? Items.Select(i => i.Value).ToArray();
        }

        /// <inheritdoc/>
        IEnumerable<tKey> IReadOnlyDictionary<tKey, tValue>.Keys
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen?.Select(i => i.Key).ToArray() ?? Items.Select(i => i.Key).ToArray();
        }

        /// <inheritdoc/>
        IEnumerable<tValue> IReadOnlyDictionary<tKey, tValue>.Values
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen?.Select(i => i.Value).ToArray() ?? Items.Select(i => i.Value).ToArray();
        }

        /// <inheritdoc/>
        public override int Count
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen?.Length ?? Items.Count;
        }

        /// <inheritdoc/>
        public override bool IsReadOnly
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen.HasValue;
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            protected set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override bool IsFixedSize
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen.HasValue;
        }

        /// <inheritdoc/>
        public override bool IsSynchronized
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen.HasValue;
        }

        /// <inheritdoc/>
        public override object SyncRoot
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => this;
        }

        /// <inheritdoc/>
        public override tValue this[tKey key]
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen.HasValue ? _Frozen.Value[EnsureExistingKey(key)].Value : Items[EnsureExistingKey(key)].Value;
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set
            {
                EnsureWritable();
                int index = IndexOfKey(key);
                if (index == -1)
                {
                    Items.Add(new(key, value));
                    RaiseOnAdded(Count, key, value);
                }
                else
                {
                    tValue prev = Items[index].Value;
                    Items[index] = new(key, value);
                    RaiseOnUpdated(index, key, prev);
                }
            }
        }

        /// <inheritdoc/>
        public override tValue this[int index]
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Frozen.HasValue ? _Frozen.Value[EnsureValidIndex(index)].Value : Items[EnsureValidIndex(index)].Value;
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set
            {
                EnsureWritable();
                index = EnsureValidIndex(index);
                KeyValuePair<tKey, tValue> prev = Items[index];
                Items[index] = new(Items[index].Key, value);
                RaiseOnUpdated(index, prev.Key, prev.Value);
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
            _Frozen = Items.ToImmutableArray();
            Items.Clear();
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
            Items.Capacity = Math.Max(InitialCapacity, _Frozen.Value.Length);
            Items.AddRange(_Frozen);
            _Frozen = null;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void SwapIndex(int index, int targetIndex)
        {
            EnsureWritable();
            EnsureValidIndex(index);
            EnsureValidIndex(targetIndex);
            if (index == targetIndex) return;
            KeyValuePair<tKey, tValue> item = Items[index],
                targetItem = Items[targetIndex];
            Items[targetIndex] = item;
            Items[index] = targetItem;
            RaiseOnUpdated(index, targetItem.Key, targetItem.Value);
            RaiseOnUpdated(targetIndex, item.Key, item.Value);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void MoveIndexUp(int index)
        {
            EnsureWritable();
            if (index < 1 || index >= Items.Count) throw new IndexOutOfRangeException();
            int prevIndex = index - 1;
            KeyValuePair<tKey, tValue> item = Items[index],
                prevItem = Items[prevIndex];
            Items[prevIndex] = item;
            Items[index] = prevItem;
            RaiseOnUpdated(prevIndex, item.Key, item.Value);
            RaiseOnUpdated(index, prevItem.Key, prevItem.Value);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void MoveIndexDown(int index)
        {
            EnsureWritable();
            if (index < 0 || index >= Items.Count - 1) throw new IndexOutOfRangeException();
            int nextIndex = index + 1;
            KeyValuePair<tKey, tValue> item = Items[index],
                nextItem = Items[nextIndex];
            Items[nextIndex] = item;
            Items[index] = nextItem;
            RaiseOnUpdated(index, nextItem.Key, nextItem.Value);
            RaiseOnUpdated(nextIndex, item.Key, item.Value);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void Add(tKey key, tValue value)
        {
            EnsureWritable();
            Items.Add(new(EnsureFreshKey(key), value));
            RaiseOnAdded(Count, key, value);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void Add(KeyValuePair<tKey, tValue> item)
        {
            EnsureWritable();
            EnsureFreshKey(item.Key);
            Items.Add(item);
            RaiseOnAdded(Count, item.Key, item.Value);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void Add(object key, object? value)
        {
            EnsureWritable();
            tKey k = (tKey)EnsureValidKey(key);
            EnsureFreshKey(k);
            Items.Add(new(k, (tValue)EnsureValidValue(value)!));
            RaiseOnAdded(Count, k, (tValue)EnsureValidValue(value)!);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void Insert(int index, tKey key, tValue value)
        {
            EnsureWritable();
            if (index < 0 || index > Items.Count) throw new IndexOutOfRangeException();
            if (index == Items.Count)
            {
                Add(key, value);
                return;
            }
            EnsureFreshKey(key);
            KeyValuePair<tKey, tValue> prev = Items[index];
            Items.Insert(index, new(key, value));
            RaiseOnUpdated(index, prev.Key, prev.Value);
            RaiseOnAdded(index, key, value);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void ReplaceAt(int index, tKey key, tValue value)
        {
            EnsureWritable();
            KeyValuePair<tKey, tValue> prev = Items[index];
            Items[EnsureValidIndex(index)] = new(key, value);
            RaiseOnRemoved(index, prev.Key, prev.Value);
            RaiseOnAdded(index, key, value);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override bool Contains(KeyValuePair<tKey, tValue> item)
        {
            int index = IndexOfKey(item.Key);
            return index != -1 && IsEqual(_Frozen.HasValue ? _Frozen.Value[index] : Items[index].Value, item.Value);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override KeyValuePair<tKey, tValue> GetAt(int index)
        {
            if (index < 0 || index >= (_Frozen?.Length ?? Items.Count)) throw new ArgumentOutOfRangeException(nameof(index));
            return _Frozen.HasValue ? _Frozen.Value[index] : Items[index];
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override int IndexOfKey(tKey key)
        {
            for (int i = 0, len = (_Frozen?.Length ?? Items.Count); i < len; i++)
                if (IsEqual(_Frozen.HasValue ? _Frozen.Value[i].Key : Items[i].Key, key))
                    return i;
            return -1;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override int IndexOfValue(tValue value)
        {
            for (int i = 0, len = (_Frozen?.Length ?? Items.Count); i < len; i++)
                if (IsEqual(_Frozen.HasValue ? _Frozen.Value[i].Value : Items[i].Value, value))
                    return i;
            return -1;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override bool Remove(KeyValuePair<tKey, tValue> item)
        {
            EnsureWritable();
            int index = IndexOfKey(item.Key);
            if (index == -1 || IsEqual(Items[index].Value, item.Value)) return false;
            RemoveAt(index);
            return true;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void RemoveAt(int index)
        {
            EnsureWritable();
            KeyValuePair<tKey, tValue> prev = Items[index];
            Items.RemoveAt(EnsureValidIndex(index));
            RaiseOnRemoved(index, prev.Key, prev.Value);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void Clear()
        {
            EnsureWritable();
            KeyValuePair<tKey, tValue>[] prev = [.. Items];
            Items.Clear();
            for (int i = 0, len = prev.Length; i < len; i++) RaiseOnRemoved(i, prev[i].Key, prev[i].Value);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void CopyTo(KeyValuePair<tKey, tValue>[] array, int arrayIndex)
        {
            if (arrayIndex < 0 || arrayIndex > array.Length) throw new IndexOutOfRangeException();
            if (array.Length - arrayIndex < (_Frozen?.Length ?? Items.Count)) throw new OverflowException();
            for (int i = 0, len = (_Frozen?.Length ?? Items.Count); i < len; array[i + arrayIndex] = _Frozen.HasValue ? _Frozen.Value[i] : Items[i], i++) ;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void CopyTo(Array array, int index)
        {
            if (index < 0 || index > array.Length) throw new IndexOutOfRangeException();
            if (array.Length - index < (_Frozen?.Length ?? Items.Count)) throw new OverflowException();
            for (int i = 0, len = (_Frozen?.Length ?? Items.Count); i < len; array.SetValue(_Frozen.HasValue ? _Frozen.Value[i] : Items[i], i + index), i++) ;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override bool TryGetValue(tKey key, [MaybeNullWhen(false)] out tValue value)
        {
            value = default;
            int index = IndexOfKey(key);
            if (index != -1) value = _Frozen.HasValue ? _Frozen.Value[index].Value : Items[index].Value;
            return index != -1;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override OrderedDictionary<tKey, tValue> AsReadOnly() => new(this);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ArgumentNullException.ThrowIfNull(info);
            info.AddValue(nameof(Items.Capacity), Items.Capacity);
            info.AddValue(nameof(Keys), (tKey[])Keys);
            info.AddValue(nameof(Values), (tValue)Values);
        }

        /// <inheritdoc/>
        public override void OnDeserialization(object? sender)
        {
            EnsureNotFrozen();
            if (Info is null) throw new SerializationException("No serialization info");
            Items.Capacity = (int)(Info.GetValue(nameof(Items.Capacity), typeof(int)) ?? throw new SerializationException($"Failed to deserialize {nameof(Items.Capacity)} ({typeof(int)})"));
            tKey[] keys = (tKey[])(Info.GetValue(nameof(Keys), typeof(tKey[])) ?? throw new SerializationException($"Failed to deserialize {nameof(Keys)} ({typeof(tKey[])})"));
            tValue[] values = (tValue[])(Info.GetValue(nameof(Values), typeof(tValue[])) ?? throw new SerializationException($"Failed to deserialize {nameof(Values)} ({typeof(tValue[])})"));
            if (keys.Length != values.Length) throw new SerializationException("Keys/values count mismatch");
            for (int i = 0; i < keys.Length; Add(keys[i], values[i]), i++) ;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override IEnumerator<KeyValuePair<tKey, tValue>> GetEnumerator()
            => _Frozen.HasValue ? ((IEnumerable<KeyValuePair<tKey, tValue>>)_Frozen.Value).GetEnumerator() : Items.GetEnumerator();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
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
            if (throwIfFrozen) throw new InvalidOperationException("Dictionary is frozen");
            return false;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected override void EnsureWritable() => EnsureNotFrozen();
    }
}

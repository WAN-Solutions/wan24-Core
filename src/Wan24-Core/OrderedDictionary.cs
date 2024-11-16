using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace wan24.Core
{
    /// <summary>
    /// Ordered dictionary
    /// </summary>
    /// <typeparam name="tKey">Key type</typeparam>
    /// <typeparam name="tValue">Value type</typeparam>
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class OrderedDictionary<tKey, tValue> : IOrderedDictionary<tKey, tValue> where tKey : notnull
    {
        /// <summary>
        /// Items
        /// </summary>
        protected readonly List<KeyValuePair<tKey, tValue>> Items;
        /// <summary>
        /// Serialization info
        /// </summary>
        protected readonly SerializationInfo? Info = null;
        /// <summary>
        /// Streaming context
        /// </summary>
        protected readonly StreamingContext? Context = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public OrderedDictionary() : this(capacity: 0) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public OrderedDictionary(in int capacity) => Items = new(capacity);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Items</param>
        public OrderedDictionary(in IEnumerable<KeyValuePair<tKey, tValue>> items) => Items = new(items);

        /// <summary>
        /// Constructor (creates a read-only dictionary)
        /// </summary>
        /// <param name="dict">Ordered dictionary</param>
        public OrderedDictionary(in OrderedDictionary<tKey, tValue> dict) : this(capacity: 0)
        {
            Items.AddRange(dict.Items);
            IsReadOnly = true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected OrderedDictionary(in SerializationInfo info, in StreamingContext context) : this(capacity: 0)
        {
            Info = info;
            Context = context;
        }

        /// <inheritdoc/>
        public virtual ICollection<tKey> Keys => Items.Select(i => i.Key).ToArray();

        /// <inheritdoc/>
        public virtual ICollection<tValue> Values => Items.Select(i => i.Value).ToArray();

        /// <inheritdoc/>
        ICollection IDictionary.Keys => Items.Select(i => i.Key).ToArray();

        /// <inheritdoc/>
        ICollection IDictionary.Values => Items.Select(i => i.Value).ToArray();

        /// <inheritdoc/>
        IEnumerable<tKey> IReadOnlyDictionary<tKey, tValue>.Keys => Items.Select(i => i.Key).ToArray();

        /// <inheritdoc/>
        IEnumerable<tValue> IReadOnlyDictionary<tKey, tValue>.Values => Items.Select(i => i.Value).ToArray();

        /// <inheritdoc/>
        public virtual int Count => Items.Count;

        /// <inheritdoc/>
        public virtual bool IsReadOnly { get; protected set; }

        /// <inheritdoc/>
        public virtual bool IsFixedSize => IsReadOnly;

        /// <inheritdoc/>
        public virtual bool IsSynchronized => false;

        /// <inheritdoc/>
        public virtual object SyncRoot => this;

        /// <inheritdoc/>
        public virtual tValue this[tKey key]
        {
            get => Items[EnsureExistingKey(key)].Value;
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
        public virtual tValue this[int index]
        {
            get => Items[EnsureValidIndex(index)].Value;
            set
            {
                EnsureWritable();
                index = EnsureValidIndex(index);
                KeyValuePair<tKey, tValue> prev = Items[index];
                Items[index] = new(Items[index].Key, value);
                RaiseOnUpdated(index, prev.Key, prev.Value);
            }
        }

        /// <inheritdoc/>
        public virtual object? this[object key]
        {
            get => this[(tKey)EnsureValidKey(key)];
            set
            {
                EnsureWritable();
                this[(tKey)EnsureValidKey(key)] = (tValue)EnsureValidValue(value)!;
            }
        }

        /// <inheritdoc/>
        object? IOrderedDictionary.this[int index]
        {
            get => this[index];
            set
            {
                EnsureWritable();
                this[EnsureValidIndex(index)] = (tValue)EnsureValidValue(value)!;
            }
        }

        /// <inheritdoc/>
        public virtual void SwapIndex(int index, int targetIndex)
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
        public virtual void MoveIndexUp(int index)
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
        public virtual void MoveIndexDown(int index)
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
        public virtual void Add(tKey key, tValue value)
        {
            EnsureWritable();
            Items.Add(new(EnsureFreshKey(key), value));
            RaiseOnAdded(Count, key, value);
        }

        /// <inheritdoc/>
        public virtual void Add(KeyValuePair<tKey, tValue> item)
        {
            EnsureWritable();
            EnsureFreshKey(item.Key);
            Items.Add(item);
            RaiseOnAdded(Count, item.Key, item.Value);
        }

        /// <inheritdoc/>
        public virtual void Add(object key, object? value)
        {
            EnsureWritable();
            tKey k = (tKey)EnsureValidKey(key);
            EnsureFreshKey(k);
            Items.Add(new(k, (tValue)EnsureValidValue(value)!));
            RaiseOnAdded(Count, k, (tValue)EnsureValidValue(value)!);
        }

        /// <inheritdoc/>
        public virtual void Insert(int index, tKey key, tValue value)
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
        public virtual void Insert(int index, object key, object? value)
        {
            EnsureWritable();
            Insert(EnsureValidIndex(index), (tKey)EnsureValidKey(key), (tValue)EnsureValidValue(value)!);
        }

        /// <inheritdoc/>
        public virtual void ReplaceAt(int index, tKey key, tValue value)
        {
            EnsureWritable();
            KeyValuePair<tKey, tValue> prev = Items[index];
            Items[EnsureValidIndex(index)] = new(key, value);
            RaiseOnRemoved(index, prev.Key, prev.Value);
            RaiseOnAdded(index, key, value);
        }

        /// <inheritdoc/>
        public virtual bool Contains(KeyValuePair<tKey, tValue> item)
        {
            int index = IndexOfKey(item.Key);
            return index != -1 && IsEqual(Items[index].Value, item.Value);
        }

        /// <inheritdoc/>
        public virtual bool Contains(object key) => key is not null && typeof(tKey).IsAssignableFrom(key.GetType()) && ContainsKey((tKey)key);

        /// <inheritdoc/>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public virtual bool ContainsKey(tKey key) => IndexOfKey(key) != -1;

        /// <inheritdoc/>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public virtual bool ContainsValue(tValue value) => IndexOfValue(value) != -1;

        /// <inheritdoc/>
        public virtual KeyValuePair<tKey, tValue> GetAt(int index)
        {
            if (index < 0 || index >= Items.Count) throw new ArgumentOutOfRangeException(nameof(index));
            return Items[index];
        }

        /// <inheritdoc/>
        public virtual int IndexOfKey(tKey key)
        {
            for (int i = 0; i < Items.Count; i++)
                if (IsEqual(Items[i].Key, key))
                    return i;
            return -1;
        }

        /// <inheritdoc/>
        public virtual int IndexOfValue(tValue value)
        {
            for (int i = 0; i < Items.Count; i++)
                if (IsEqual(Items[i].Value, value))
                    return i;
            return -1;
        }

        /// <inheritdoc/>
        public virtual bool Remove(tKey key)
        {
            EnsureWritable();
            int index = IndexOfKey(key);
            if (index == -1) return false;
            RemoveAt(index);
            return true;
        }

        /// <inheritdoc/>
        public virtual bool Remove(KeyValuePair<tKey, tValue> item)
        {
            EnsureWritable();
            int index = IndexOfKey(item.Key);
            if (index == -1 || IsEqual(Items[index].Value, item.Value)) return false;
            RemoveAt(index);
            return true;
        }

        /// <inheritdoc/>
        public virtual void Remove(object key)
        {
            EnsureWritable();
            if (!Remove((tKey)EnsureValidKey(key))) throw new KeyNotFoundException();
        }

        /// <inheritdoc/>
        public virtual void RemoveAt(int index)
        {
            EnsureWritable();
            KeyValuePair<tKey, tValue> prev = Items[index];
            Items.RemoveAt(EnsureValidIndex(index));
            RaiseOnRemoved(index, prev.Key, prev.Value);
        }

        /// <inheritdoc/>
        public virtual void Clear()
        {
            EnsureWritable();
            KeyValuePair<tKey, tValue>[] prev = [.. Items];
            Items.Clear();
            for (int i = 0, len = prev.Length; i < len; i++) RaiseOnRemoved(i, prev[i].Key, prev[i].Value);
        }

        /// <inheritdoc/>
        public virtual void CopyTo(KeyValuePair<tKey, tValue>[] array, int arrayIndex)
        {
            if (arrayIndex < 0 || arrayIndex > array.Length) throw new IndexOutOfRangeException();
            if (array.Length - arrayIndex < Items.Count) throw new OverflowException();
            for (int i = 0; i < Items.Count; array[i + arrayIndex] = Items[i], i++) ;
        }

        /// <inheritdoc/>
        public virtual void CopyTo(Array array, int index)
        {
            if (index < 0 || index > array.Length) throw new IndexOutOfRangeException();
            if (array.Length - index < Items.Count) throw new OverflowException();
            for (int i = 0; i < Items.Count; array.SetValue(Items[i], i + index), i++) ;
        }

        /// <inheritdoc/>
        public virtual bool TryGetValue(tKey key, [MaybeNullWhen(false)] out tValue value)
        {
            value = default;
            int index = IndexOfKey(key);
            if (index != -1) value = Items[index].Value;
            return index != -1;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual OrderedDictionary<tKey, tValue> AsReadOnly() => new(this);

        /// <inheritdoc/>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ArgumentNullException.ThrowIfNull(info);
            info.AddValue(nameof(Items.Capacity), Items.Capacity);
            info.AddValue(nameof(IsReadOnly), IsReadOnly);
            info.AddValue(nameof(Keys), (tKey[])Keys);
            info.AddValue(nameof(Values), (tValue)Values);
        }

        /// <inheritdoc/>
        public virtual void OnDeserialization(object? sender)
        {
            if (Info is null) throw new SerializationException("No serialization info");
            Items.Capacity = (int)(Info.GetValue(nameof(Items.Capacity), typeof(int)) ?? throw new SerializationException($"Failed to deserialize {nameof(Items.Capacity)} ({typeof(int)})"));
            tKey[] keys = (tKey[])(Info.GetValue(nameof(Keys), typeof(tKey[])) ?? throw new SerializationException($"Failed to deserialize {nameof(Keys)} ({typeof(tKey[])})"));
            tValue[] values = (tValue[])(Info.GetValue(nameof(Values), typeof(tValue[])) ?? throw new SerializationException($"Failed to deserialize {nameof(Values)} ({typeof(tValue[])})"));
            if (keys.Length != values.Length) throw new SerializationException("Keys/values count mismatch");
            for (int i = 0; i < keys.Length; Add(keys[i], values[i]), i++) ;
            IsReadOnly = (bool)(Info.GetValue(nameof(IsReadOnly), typeof(bool)) ?? throw new SerializationException($"Failed to deserialize {nameof(IsReadOnly)} ({typeof(bool)})"));
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual IEnumerator<KeyValuePair<tKey, tValue>> GetEnumerator() => Items.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

        /// <inheritdoc/>
        IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator(GetEnumerator());

        IDictionaryEnumerator IOrderedDictionary.GetEnumerator() => new DictionaryEnumerator(GetEnumerator());

        /// <summary>
        /// Determine if two objects are equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Are equal?</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected virtual bool IsEqual(object? a, object? b) => (a is null && b is null) || (a is not null && a.Equals(b)) || (b is not null && b.Equals(a));

        /// <summary>
        /// Ensure a valid key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Key</returns>
        /// <exception cref="ArgumentException">If the key is invalid</exception>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected virtual object EnsureValidKey(object key)
            => key is null || !typeof(tKey).IsAssignableFrom(key.GetType()) ? throw new ArgumentException("Invalid key", nameof(key)) : key;

        /// <summary>
        /// Ensure a valid value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        /// <exception cref="ArgumentException">If the value is invalid</exception>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected virtual object? EnsureValidValue(object? value)
            => value is not null && !typeof(tValue).IsAssignableFrom(value.GetType()) ? throw new ArgumentException("Invalid value", nameof(value)) : value;

        /// <summary>
        /// Ensure a valid index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Index</returns>
        /// <exception cref="IndexOutOfRangeException">If the index is invalid</exception>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected virtual int EnsureValidIndex(int index) => index < 0 || index >= Items.Count ? throw new IndexOutOfRangeException() : index;

        /// <summary>
        /// Ensure writable
        /// </summary>
        /// <exception cref="NotSupportedException">If read-only</exception>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected virtual void EnsureWritable()
        {
            if (IsReadOnly) throw new NotSupportedException();
        }

        /// <summary>
        /// Ensure a fresh (non-existing) key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Key</returns>
        /// <exception cref="ArgumentException">If the key exists</exception>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected virtual tKey EnsureFreshKey(tKey key) => IndexOfKey(key) == -1 ? key : throw new ArgumentException("Key exists", nameof(key));

        /// <summary>
        /// Ensure an existing key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Index</returns>
        /// <exception cref="KeyNotFoundException">If the key doesn't exist</exception>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected virtual int EnsureExistingKey(tKey key)
        {
            int index = IndexOfKey(key);
            if (index == -1) throw new KeyNotFoundException();
            return index;
        }

        /// <summary>
        /// Delegate for an <see cref="OrderedDictionary{tKey, tValue}"/> event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        public delegate void OrderedDictionary_Delegate(OrderedDictionary<tKey, tValue> sender, OrderedDictionaryEventArgs e);

        /// <summary>
        /// Raised when added a key/value pair
        /// </summary>
        public event OrderedDictionary_Delegate? OnAdded;
        /// <summary>
        /// Raise the <see cref="OnAdded"/> event
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        protected virtual void RaiseOnAdded(in int index, in tKey key, in tValue value) => OnAdded?.Invoke(this, new(index, key, value));

        /// <summary>
        /// Raised when updated a value (the event handler will get the previous index, key and value)
        /// </summary>
        public event OrderedDictionary_Delegate? OnUpdated;
        /// <summary>
        /// Raise the <see cref="OnUpdated"/> event
        /// </summary>
        /// <param name="index">Previous index</param>
        /// <param name="key">Previous key</param>
        /// <param name="value">Previous value</param>
        protected virtual void RaiseOnUpdated(in int index, in tKey key, in tValue value) => OnUpdated?.Invoke(this, new(index, key, value));

        /// <summary>
        /// Raised when removed a key/value pair
        /// </summary>
        public event OrderedDictionary_Delegate? OnRemoved;
        /// <summary>
        /// Raise the <see cref="OnRemoved"/> event
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        protected virtual void RaiseOnRemoved(in int index, in tKey key, in tValue value) => OnRemoved?.Invoke(this, new(index, key, value));

        /// <summary>
        /// Cast as item count
        /// </summary>
        /// <param name="dict">Dictionary</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(in OrderedDictionary<tKey, tValue> dict) => dict.Count;

        /// <summary>
        /// Event arguments
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="index">Index</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public class OrderedDictionaryEventArgs(in int index, in tKey key, in tValue value) : EventArgs()
        {
            /// <summary>
            /// Index
            /// </summary>
            public int Index { get; } = index;

            /// <summary>
            /// Key
            /// </summary>
            public tKey Key { get; } = key;

            /// <summary>
            /// Value
            /// </summary>
            public tValue Value { get; } = value;
        }

        /// <summary>
        /// Dictionary enumerator
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="enumerator">Enumerator</param>
        protected class DictionaryEnumerator(in IEnumerator<KeyValuePair<tKey, tValue>> enumerator) : IDictionaryEnumerator
        {
            /// <summary>
            /// Enumerator
            /// </summary>
            protected readonly IEnumerator<KeyValuePair<tKey, tValue>> Enumerator = enumerator;

            /// <inheritdoc/>
            public DictionaryEntry Entry => new(Enumerator.Current.Key, Enumerator.Current.Value);

            /// <inheritdoc/>
            public object Key => Enumerator.Current.Key;

            /// <inheritdoc/>
            public object? Value => Enumerator.Current.Value;

            /// <inheritdoc/>
            public object Current => Entry;

            /// <inheritdoc/>
            public bool MoveNext() => Enumerator.MoveNext();

            /// <inheritdoc/>
            public void Reset() => Enumerator.Reset();
        }
    }
}

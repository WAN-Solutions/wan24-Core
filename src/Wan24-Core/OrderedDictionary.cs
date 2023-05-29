using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

//TODO .NET 8: Get as frozen dictionary

namespace wan24.Core
{
    /// <summary>
    /// Ordered dictionary
    /// </summary>
    /// <typeparam name="tKey">Key type</typeparam>
    /// <typeparam name="tValue">Value type</typeparam>
    [Serializable]
    public class OrderedDictionary<tKey, tValue> : IOrderedDictionary<tKey, tValue> where tKey : notnull
    {
        /// <summary>
        /// Items
        /// </summary>
        protected readonly List<KeyValuePair<tKey, tValue>> Items;
        /// <summary>
        /// Serialization info
        /// </summary>
        protected SerializationInfo? Info = null;
        /// <summary>
        /// Streaming context
        /// </summary>
        protected StreamingContext? Context = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public OrderedDictionary() : this(0) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public OrderedDictionary(int capacity)
        {
            Items = new(capacity);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Items</param>
        public OrderedDictionary(IEnumerable<KeyValuePair<tKey, tValue>> items) => Items = new(items);

        /// <summary>
        /// Constructor (creates a read-only dictionary)
        /// </summary>
        /// <param name="dict">Ordered dictionary</param>
        public OrderedDictionary(OrderedDictionary<tKey, tValue> dict) : this(0)
        {
            Items.AddRange(dict);
            IsReadOnly = true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected OrderedDictionary(SerializationInfo info, StreamingContext context) : this(0)
        {
            Info = info;
            Context = context;
        }

        /// <inheritdoc/>
        public ICollection<tKey> Keys => Items.Select(i => i.Key).ToArray();

        /// <inheritdoc/>
        public ICollection<tValue> Values => Items.Select(i => i.Value).ToArray();

        /// <inheritdoc/>
        ICollection IDictionary.Keys => Items.Select(i => i.Key).ToArray();

        /// <inheritdoc/>
        ICollection IDictionary.Values => Items.Select(i => i.Value).ToArray();

        /// <inheritdoc/>
        IEnumerable<tKey> IReadOnlyDictionary<tKey, tValue>.Keys => Items.Select(i => i.Key).ToArray();

        /// <inheritdoc/>
        IEnumerable<tValue> IReadOnlyDictionary<tKey, tValue>.Values => Items.Select(i => i.Value).ToArray();

        /// <inheritdoc/>
        public int Count => Items.Count;

        /// <inheritdoc/>
        public virtual bool IsReadOnly { get; protected set; }

        /// <inheritdoc/>
        public virtual bool IsFixedSize => IsReadOnly;

        /// <inheritdoc/>
        public virtual bool IsSynchronized => false;

        /// <inheritdoc/>
        public virtual object SyncRoot => this;

        /// <inheritdoc/>
        public tValue this[tKey key]
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
        public tValue this[int index]
        {
            get => Items[EnsureValidIndex(index)].Value;
            set
            {
                index = EnsureValidIndex(index);
                KeyValuePair<tKey, tValue> prev = Items[index];
                Items[index] = new(Items[index].Key, value);
                RaiseOnUpdated(index, prev.Key, prev.Value);
            }
        }

        /// <inheritdoc/>
        public object? this[object key]
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
        public void Add(object key, object? value)
        {
            EnsureWritable();
            tKey k = (tKey)EnsureValidKey(key);
            EnsureFreshKey(k);
            Items.Add(new(k, (tValue)EnsureValidValue(value)!));
            RaiseOnAdded(Count, k, (tValue)EnsureValidValue(value)!);
        }

        /// <inheritdoc/>
        public void Insert(int index, tKey key, tValue value)
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
        public void Insert(int index, object key, object? value)
        {
            EnsureWritable();
            Insert(EnsureValidIndex(index), (tKey)EnsureValidKey(key), (tValue)EnsureValidValue(value)!);
        }

        /// <inheritdoc/>
        public void ReplaceAt(int index, tKey key, tValue value)
        {
            EnsureWritable();
            KeyValuePair<tKey, tValue> prev = Items[index];
            Items[EnsureValidIndex(index)] = new(key, value);
            RaiseOnRemoved(index, prev.Key, prev.Value);
            RaiseOnAdded(index, key, value);
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<tKey, tValue> item)
        {
            int index = IndexOfKey(item.Key);
            return index != -1 && IsEqual(Items[index].Value, item.Value);
        }

        /// <inheritdoc/>
        public bool Contains(object key) => key != null && typeof(tKey).IsAssignableFrom(key.GetType()) && ContainsKey((tKey)key);

        /// <inheritdoc/>
        public bool ContainsKey(tKey key) => IndexOfKey(key) != -1;

        /// <inheritdoc/>
        public bool ContainsValue(tValue value) => IndexOfValue(value) != -1;

        /// <inheritdoc/>
        public KeyValuePair<tKey, tValue> Get(int index)
        {
            if (index < 0 || index >= Items.Count) throw new ArgumentOutOfRangeException(nameof(index));
            return Items[index];
        }

        /// <inheritdoc/>
        public int IndexOfKey(tKey key)
        {
            for (int i = 0; i < Items.Count; i++)
                if (IsEqual(Items[i].Key, key))
                    return i;
            return -1;
        }

        /// <inheritdoc/>
        public int IndexOfValue(tValue value)
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
        public void Remove(object key)
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
            KeyValuePair<tKey, tValue>[] prev = Items.ToArray();
            Items.Clear();
            for (int i = 0, len = prev.Length; i < len; i++) RaiseOnRemoved(i, prev[i].Key, prev[i].Value);
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<tKey, tValue>[] array, int arrayIndex)
        {
            if (arrayIndex < 0 || arrayIndex > array.Length) throw new IndexOutOfRangeException();
            if (array.Length - arrayIndex < Items.Count) throw new OverflowException();
            for (int i = 0; i < Items.Count; array[i + arrayIndex] = Items[i], i++) ;
        }

        /// <inheritdoc/>
        public void CopyTo(Array array, int index)
        {
            if (index < 0 || index > array.Length) throw new IndexOutOfRangeException();
            if (array.Length - index < Items.Count) throw new OverflowException();
            for (int i = 0; i < Items.Count; array.SetValue(Items[i], i + index), i++) ;
        }

        /// <inheritdoc/>
        public bool TryGetValue(tKey key, [MaybeNullWhen(false)] out tValue value)
        {
            value = default;
            int index = IndexOfKey(key);
            if (index != -1) value = Items[index].Value;
            return index != -1;
        }

        /// <inheritdoc/>
        public OrderedDictionary<tKey, tValue> AsReadOnly() => new(this);

        /// <inheritdoc/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ArgumentNullException.ThrowIfNull(info);
            info.AddValue(nameof(Items.Capacity), Items.Capacity);
            info.AddValue(nameof(IsReadOnly), IsReadOnly);
            info.AddValue(nameof(Keys), (tKey[])Keys);
            info.AddValue(nameof(Values), (tValue)Values);
        }

        /// <inheritdoc/>
        public void OnDeserialization(object? sender)
        {
            if (Info == null) throw new SerializationException("No serialization info");
            Items.Capacity = (int)(Info.GetValue(nameof(Items.Capacity), typeof(int)) ?? throw new SerializationException($"Failed to deserialize {nameof(Items.Capacity)} ({typeof(int)})"));
            tKey[] keys = (tKey[])(Info.GetValue(nameof(Keys), typeof(tKey[])) ?? throw new SerializationException($"Failed to deserialize {nameof(Keys)} ({typeof(tKey[])})"));
            tValue[] values = (tValue[])(Info.GetValue(nameof(Values), typeof(tValue[])) ?? throw new SerializationException($"Failed to deserialize {nameof(Values)} ({typeof(tValue[])})"));
            if (keys.Length != values.Length) throw new SerializationException("Keys/values count mismatch");
            for (int i = 0; i < keys.Length; Add(keys[i], values[i]), i++) ;
            IsReadOnly = (bool)(Info.GetValue(nameof(IsReadOnly), typeof(bool)) ?? throw new SerializationException($"Failed to deserialize {nameof(IsReadOnly)} ({typeof(bool)})"));
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<tKey, tValue>> GetEnumerator() => Items.GetEnumerator();

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
        protected bool IsEqual(object? a, object? b) => (a == null && b == null) || (a != null && a.Equals(b)) || (b != null && b.Equals(a));

        /// <summary>
        /// Ensure a valid key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Key</returns>
        /// <exception cref="ArgumentException">If the key is invalid</exception>
        protected object EnsureValidKey(object key)
            => key == null || !typeof(tKey).IsAssignableFrom(key.GetType()) ? throw new ArgumentException("Invalid key", nameof(key)) : key;

        /// <summary>
        /// Ensure a valid value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        /// <exception cref="ArgumentException">If the value is invalid</exception>
        protected object? EnsureValidValue(object? value)
            => value != null && !typeof(tValue).IsAssignableFrom(value.GetType()) ? throw new ArgumentException("Invalid value", nameof(value)) : value;

        /// <summary>
        /// Ensure a valid index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Index</returns>
        /// <exception cref="IndexOutOfRangeException">If the index is invalid</exception>
        protected int EnsureValidIndex(int index) => index < 0 || index >= Items.Count ? throw new IndexOutOfRangeException() : index;

        /// <summary>
        /// Ensure writable
        /// </summary>
        /// <exception cref="NotSupportedException">If read-only</exception>
        protected void EnsureWritable()
        {
            if (IsReadOnly) throw new NotSupportedException();
        }

        /// <summary>
        /// Ensure a fresh (non-existing) key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Key</returns>
        /// <exception cref="ArgumentException">If the key exists</exception>
        protected tKey EnsureFreshKey(tKey key) => IndexOfKey(key) == -1 ? key : throw new ArgumentException("Key exists", nameof(key));

        /// <summary>
        /// Ensure an existing key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Index</returns>
        /// <exception cref="KeyNotFoundException">If the key doesn't exist</exception>
        protected int EnsureExistingKey(tKey key)
        {
            int index = IndexOfKey(key);
            if (index == -1) throw new KeyNotFoundException();
            return index;
        }

        /// <summary>
        /// Delegate for an <see cref="OrderedDictionary{tKey, tValue}"/> event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="index">Key/value pair index</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public delegate void OrderedDictionary_Delegate(OrderedDictionary<tKey, tValue> sender, int index, tKey key, tValue value);

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
        protected virtual void RaiseOnAdded(int index, tKey key, tValue value) => OnAdded?.Invoke(this, index, key, value);

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
        protected virtual void RaiseOnUpdated(int index, tKey key, tValue value) => OnUpdated?.Invoke(this, index, key, value);

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
        protected virtual void RaiseOnRemoved(int index, tKey key, tValue value) => OnRemoved?.Invoke(this, index, key, value);

        /// <summary>
        /// Dictionary enumerator
        /// </summary>
        protected class DictionaryEnumerator : IDictionaryEnumerator
        {
            /// <summary>
            /// Enumerator
            /// </summary>
            protected readonly IEnumerator<KeyValuePair<tKey, tValue>> Enumerator;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="enumerator">Enumerator</param>
            public DictionaryEnumerator(IEnumerator<KeyValuePair<tKey, tValue>> enumerator) => Enumerator = enumerator;

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

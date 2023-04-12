﻿using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

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
            get
            {
                int index = IndexOfKey(key);
                if (index == -1) throw new KeyNotFoundException();
                return Items[index].Value;
            }
            set
            {
                if (IsReadOnly) throw new NotSupportedException();
                int index = IndexOfKey(key);
                if (index == -1)
                {
                    Items.Add(new(key, value));
                }
                else
                {
                    Items[index] = new(key, value);
                }
            }
        }

        /// <inheritdoc/>
        public tValue this[int index]
        {
            get => Items.Count <= index ? throw new IndexOutOfRangeException() : Items[index].Value;
            set
            {
                if (index < 0 || index >= Items.Count) throw new IndexOutOfRangeException();
                Items[index] = new(Items[index].Key, value);
            }
        }

        /// <inheritdoc/>
        public object? this[object key]
        {
            get
            {
                if (key == null || !typeof(tKey).IsAssignableFrom(key.GetType())) throw new KeyNotFoundException();
                return this[(tKey)key];
            }
            set
            {
                if (IsReadOnly) throw new NotSupportedException();
                if (key == null || !typeof(tKey).IsAssignableFrom(key.GetType())) throw new ArgumentException("Invalid key", nameof(key));
                if (value != null && !typeof(tValue).IsAssignableFrom(value.GetType())) throw new ArgumentException("Invalid value", nameof(value));
                this[(tKey)key] = (tValue)value!;
            }
        }

        /// <inheritdoc/>
        object? IOrderedDictionary.this[int index]
        {
            get => this[index];
            set
            {
                if (index < 0 || index >= Items.Count) throw new IndexOutOfRangeException();
                if (value != null && !typeof(tValue).IsAssignableFrom(value.GetType())) throw new ArgumentException("Invalid value", nameof(value));
                this[index] = (tValue)value!;
            }
        }

        /// <inheritdoc/>
        public virtual void Add(tKey key, tValue value)
        {
            if (IsReadOnly) throw new InvalidOperationException();
            if (IndexOfKey(key) != -1) throw new InvalidOperationException("Key exists");
            Items.Add(new(key, value));
        }

        /// <inheritdoc/>
        public virtual void Add(KeyValuePair<tKey, tValue> item)
        {
            if (IsReadOnly) throw new InvalidOperationException();
            if (IndexOfKey(item.Key) != -1) throw new InvalidOperationException("Key exists");
            Items.Add(item);
        }

        /// <inheritdoc/>
        public void Add(object key, object? value)
        {
            if (IsReadOnly) throw new InvalidOperationException();
            if (key == null || !typeof(tKey).IsAssignableFrom(key.GetType())) throw new ArgumentException("Invalid key", nameof(key));
            if (value != null && !typeof(tValue).IsAssignableFrom(value.GetType())) throw new ArgumentException("Invalid value", nameof(value));
            tKey k = (tKey)key;
            if (IndexOfKey(k) != -1) throw new InvalidOperationException("Key exists");
            Items.Add(new(k, (tValue)value!));
        }

        /// <inheritdoc/>
        public void Insert(int index, tKey key, tValue value)
        {
            if (IsReadOnly) throw new InvalidOperationException();
            if (index < 0 || index > Items.Count) throw new IndexOutOfRangeException();
            if (index == Items.Count)
            {
                Add(key, value);
                return;
            }
            if (IndexOfKey(key) != -1) throw new InvalidOperationException("Key exists");
            Items.Insert(index, new(key, value));
        }

        /// <inheritdoc/>
        public void Insert(int index, object key, object? value)
        {
            if (IsReadOnly) throw new InvalidOperationException();
            if (index < 0 || index > Items.Count) throw new IndexOutOfRangeException();
            if (key == null || !typeof(tKey).IsAssignableFrom(key.GetType())) throw new ArgumentException("Invalid key", nameof(key));
            if (value != null && !typeof(tValue).IsAssignableFrom(value.GetType())) throw new ArgumentException("Invalid value", nameof(value));
            Insert(index, (tKey)key, (tValue)value!);
        }

        /// <inheritdoc/>
        public void ReplaceAt(int index, tKey key, tValue value)
        {
            if (IsReadOnly) throw new InvalidOperationException();
            if (index < 0 || index >= Items.Count) throw new IndexOutOfRangeException();
            Items[index] = new(key, value);
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
            if (IsReadOnly) throw new InvalidOperationException();
            int index = IndexOfKey(key);
            if (index == -1) return false;
            RemoveAt(index);
            return true;
        }

        /// <inheritdoc/>
        public virtual bool Remove(KeyValuePair<tKey, tValue> item)
        {
            if (IsReadOnly) throw new InvalidOperationException();
            int index = IndexOfKey(item.Key);
            if (index == -1 || IsEqual(Items[index].Value, item.Value)) return false;
            RemoveAt(index);
            return true;
        }

        /// <inheritdoc/>
        public void Remove(object key)
        {
            if (IsReadOnly) throw new InvalidOperationException();
            if (key == null || !typeof(tKey).IsAssignableFrom(key.GetType()) || !Remove((tKey)key)) throw new KeyNotFoundException();
        }

        /// <inheritdoc/>
        public virtual void RemoveAt(int index)
        {
            if (IsReadOnly) throw new InvalidOperationException();
            if (index < 0 || index >= Items.Count) throw new IndexOutOfRangeException();
            Items.RemoveAt(index);
        }

        /// <inheritdoc/>
        public virtual void Clear()
        {
            if (IsReadOnly) throw new InvalidOperationException();
            Items.Clear();
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

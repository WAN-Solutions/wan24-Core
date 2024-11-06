using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Concurrent dictionary using a <see cref="Dictionary{TKey, TValue}"/> and <see langword="lock"/>
    /// </summary>
    /// <typeparam name="tKey">Key type</typeparam>
    /// <typeparam name="tValue">Value type</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public partial class ConcurrentLockDictionary<tKey, tValue> : IDictionary<tKey, tValue>, IReadOnlyDictionary<tKey, tValue>, IDictionary where tKey : notnull
    {
        /// <summary>
        /// Underlying dictionary
        /// </summary>
        protected readonly Dictionary<tKey, tValue> Dict;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConcurrentLockDictionary() => Dict = [];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comparer">Key comparer</param>
        public ConcurrentLockDictionary(in EqualityComparer<tKey>? comparer) => Dict = new(comparer);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public ConcurrentLockDictionary(in int capacity) => Dict = new(capacity);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="comparer">Key comparer</param>
        public ConcurrentLockDictionary(in int capacity, in EqualityComparer<tKey>? comparer) => Dict = new(capacity, comparer);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Initial items</param>
        public ConcurrentLockDictionary(in IEnumerable<KeyValuePair<tKey, tValue>> items) => Dict = new(items);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Initial items</param>
        /// <param name="comparer">Key comparer</param>
        public ConcurrentLockDictionary(in IEnumerable<KeyValuePair<tKey, tValue>> items, in EqualityComparer<tKey>? comparer) => Dict = new(items, comparer);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dict">Dictionary to copy</param>
        public ConcurrentLockDictionary(in IDictionary<tKey, tValue> dict) => Dict = new(dict);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dict">Dictionary to copy</param>
        /// <param name="comparer">Key comparer</param>
        public ConcurrentLockDictionary(in IDictionary<tKey, tValue> dict, in EqualityComparer<tKey>? comparer) => Dict = new(dict, comparer);

        /// <inheritdoc/>
        public virtual tValue this[tKey key]
        {
            get
            {
                lock (Dict) return Dict[key];
            }
            set
            {
                lock (Dict) Dict[key] = value;
            }
        }

        /// <inheritdoc/>
        public virtual ICollection<tKey> Keys
        {
            get
            {
                lock (Dict) return [.. Dict.Keys];
            }
        }

        /// <inheritdoc/>
        public virtual ICollection<tValue> Values
        {
            get
            {
                lock (Dict) return [.. Values];
            }
        }

        /// <inheritdoc/>
        public virtual int Count
        {
            get
            {
                lock (Dict) return Dict.Count;
            }
        }

        /// <inheritdoc/>
        public virtual bool IsReadOnly => false;

        /// <inheritdoc/>
        public virtual void Add(tKey key, tValue value)
        {
            lock (Dict) Dict.Add(key, value);
        }

        /// <inheritdoc/>
        public virtual void Add(KeyValuePair<tKey, tValue> item)
        {
            lock (Dict) ((ICollection<KeyValuePair<tKey, tValue>>)Dict).Add(item);
        }

        /// <inheritdoc/>
        public virtual void Clear()
        {
            lock (Dict) Dict.Clear();
        }

        /// <inheritdoc/>
        public virtual bool Contains(KeyValuePair<tKey, tValue> item)
        {
            lock (Dict) return ((ICollection<KeyValuePair<tKey, tValue>>)Dict).Contains(item);
        }

        /// <inheritdoc/>
        public virtual bool ContainsKey(tKey key)
        {
            lock (Dict) return Dict.ContainsKey(key);
        }

        /// <inheritdoc/>
        public virtual void CopyTo(KeyValuePair<tKey, tValue>[] array, int arrayIndex)
        {
            lock (Dict) ((ICollection<KeyValuePair<tKey, tValue>>)Dict).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public virtual IEnumerator<KeyValuePair<tKey, tValue>> GetEnumerator()
        {
            lock (Dict) return Dict.ToList().GetEnumerator();
        }

        /// <inheritdoc/>
        public virtual bool Remove(tKey key)
        {
            lock (Dict) return Dict.Remove(key);
        }

        /// <inheritdoc/>
        public virtual bool Remove(KeyValuePair<tKey, tValue> item)
        {
            lock (Dict) return ((ICollection<KeyValuePair<tKey, tValue>>)Dict).Remove(item);
        }

        /// <inheritdoc/>
        public virtual bool TryGetValue(tKey key, [MaybeNullWhen(false)] out tValue value)
        {
            lock (Dict) return Dict.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

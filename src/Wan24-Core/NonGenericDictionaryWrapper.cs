using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Non-generic <see cref="IDictionary"/> wrapper (casts keys and values)
    /// </summary>
    /// <typeparam name="tKey">Key type</typeparam>
    /// <typeparam name="tValue">Value type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="dict">Dictionary</param>
    public sealed class NonGenericDictionaryWrapper<tKey, tValue>(in IDictionary dict) : IDictionary<tKey, tValue>
        where tKey : notnull
    {
        /// <summary>
        /// Dictionary
        /// </summary>
        private readonly IDictionary Dict = dict;

        /// <inheritdoc/>
        public tValue this[tKey key]
        {
            get => (tValue)Dict[key]!;
            set => Dict[key] = value;
        }

        /// <inheritdoc/>
        public ICollection<tKey> Keys => Dict.Keys.Cast<tKey>().ToArray();

        /// <inheritdoc/>
        public ICollection<tValue> Values => Dict.Values.Cast<tValue>().ToArray();

        /// <inheritdoc/>
        public int Count => Dict.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => Dict.IsReadOnly;

        /// <inheritdoc/>
        public void Add(tKey key, tValue value) => Dict.Add(key, value);

        /// <inheritdoc/>
        public void Add(KeyValuePair<tKey, tValue> item) => Dict.Add(item.Key, item.Value);

        /// <inheritdoc/>
        public void Clear() => Dict.Clear();

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<tKey, tValue> item) => Dict.Contains(item.Key) && (Dict[item.Key]?.Equals(item.Value) ?? item.Value is null);

        /// <inheritdoc/>
        public bool ContainsKey(tKey key) => Dict.Contains(key);

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<tKey, tValue>[] array, int arrayIndex)
        {
            tKey[] keys = Dict.Keys.Cast<tKey>().ToArray();
            for (int i = 0, len = keys.Length, len2 = array.Length; i < len && i + arrayIndex < len2; array[i + arrayIndex] = new(keys[i], (tValue)Dict[keys[i]]!), i++) ;
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<tKey, tValue>> GetEnumerator() => new Enumerator(Dict.GetEnumerator());

        /// <inheritdoc/>
        public bool Remove(tKey key)
        {
            int count = Dict.Count;
            Dict.Remove(key);
            return Dict.Count < count;
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<tKey, tValue> item) => Contains(item) && Remove(item.Key);

        /// <inheritdoc/>
        public bool TryGetValue(tKey key, [MaybeNullWhen(false)] out tValue value)
        {
            if (Dict.Contains(key))
            {
                value = (tValue)Dict[key]!;
                return true;
            }
            value = default;
            return false;
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Enumerator
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="enumerator">Dictionary enumerator</param>
        private sealed class Enumerator(in IDictionaryEnumerator enumerator) : SimpleDisposableBase(), IEnumerator<KeyValuePair<tKey, tValue>>
        {
            /// <summary>
            /// Enumerator
            /// </summary>
            private readonly IDictionaryEnumerator DictEnumerator = enumerator;
            /// <summary>
            /// Current
            /// </summary>
            private KeyValuePair<tKey, tValue> _Current = default;

            /// <inheritdoc/>
            public KeyValuePair<tKey, tValue> Current => _Current;

            /// <inheritdoc/>
            object IEnumerator.Current => _Current;

            /// <inheritdoc/>
            public bool MoveNext()
            {
                if (DictEnumerator.MoveNext())
                {
                    _Current = new((tKey)DictEnumerator.Entry.Key, (tValue)DictEnumerator.Entry.Value!);
                    return true;
                }
                _Current = default;
                return false;
            }

            /// <inheritdoc/>
            public void Reset() => DictEnumerator.Reset();

            /// <inheritdoc/>
            protected override void Dispose(bool disposing) => DictEnumerator.TryDispose();
        }
    }
}

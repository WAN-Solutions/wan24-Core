using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    // IDictionary implementation
    public partial class InMemoryCache<T> : IDictionary<string, T>
    {
        /// <inheritdoc/>
        InMemoryCacheEntry<T> IDictionary<string, InMemoryCacheEntry<T>>.this[string key]
        {
            get => Get(key) ?? throw new KeyNotFoundException();
            set => Add(key, value.Item, value.CreateOptions());
        }

        /// <inheritdoc/>
        T IDictionary<string, T>.this[string key]
        {
            get => Get(key) is InMemoryCacheEntry<T> entry
                ? entry.Item
                : throw new KeyNotFoundException();
            set => Add(key, value);
        }

        /// <inheritdoc/>
        ICollection<string> IDictionary<string, InMemoryCacheEntry<T>>.Keys => IfUndisposed(() => Cache.Keys);

        /// <inheritdoc/>
        ICollection<string> IDictionary<string, T>.Keys => IfUndisposed(() => Cache.Keys);

        /// <inheritdoc/>
        ICollection<InMemoryCacheEntry<T>> IDictionary<string, InMemoryCacheEntry<T>>.Values => IfUndisposed(() => Cache.Values);

        /// <inheritdoc/>
        ICollection<T> IDictionary<string, T>.Values => IfUndisposed(() => Cache.Values.Select(e => e.Item).ToList());

        /// <inheritdoc/>
        int ICollection<KeyValuePair<string, InMemoryCacheEntry<T>>>.Count => IfUndisposed(_Count);

        /// <inheritdoc/>
        int ICollection<KeyValuePair<string, T>>.Count => IfUndisposed(_Count);

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<string, InMemoryCacheEntry<T>>>.IsReadOnly => false;

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<string, T>>.IsReadOnly => false;

        /// <inheritdoc/>
        void IDictionary<string, InMemoryCacheEntry<T>>.Add(string key, InMemoryCacheEntry<T> value) => Add(key, value.Item, value.CreateOptions());

        /// <inheritdoc/>
        void ICollection<KeyValuePair<string, InMemoryCacheEntry<T>>>.Add(KeyValuePair<string, InMemoryCacheEntry<T>> item)
            => Add(item.Key, item.Value.Item, item.Value.CreateOptions());

        /// <inheritdoc/>
        void IDictionary<string, T>.Add(string key, T value) => Add(key, value);

        /// <inheritdoc/>
        void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item) => Add(item.Key, item.Value);

        /// <inheritdoc/>
        void ICollection<KeyValuePair<string, InMemoryCacheEntry<T>>>.Clear() => Clear(disposeItems: true);

        /// <inheritdoc/>
        void ICollection<KeyValuePair<string, T>>.Clear() => Clear(disposeItems: true);

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<string, InMemoryCacheEntry<T>>>.Contains(KeyValuePair<string, InMemoryCacheEntry<T>> item)
            => Get(item.Key) is InMemoryCacheEntry<T> entry && entry.Item!.Equals(item.Value.Item);

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<string, T>>.Contains(KeyValuePair<string, T> item)
            => Get(item.Key) is InMemoryCacheEntry<T> entry && entry.Item!.Equals(item.Value);

        /// <inheritdoc/>
        bool IDictionary<string, InMemoryCacheEntry<T>>.ContainsKey(string key) => IfUndisposed(() => Cache.ContainsKey(key));

        /// <inheritdoc/>
        bool IDictionary<string, T>.ContainsKey(string key) => IfUndisposed(() => Cache.ContainsKey(key));

        /// <inheritdoc/>
        void ICollection<KeyValuePair<string, InMemoryCacheEntry<T>>>.CopyTo(KeyValuePair<string, InMemoryCacheEntry<T>>[] array, int arrayIndex)
            => IfUndisposed(() => Cache.CopyTo(array, arrayIndex));

        /// <inheritdoc/>
        void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            EnsureUndisposed();
            KeyValuePair<string, InMemoryCacheEntry<T>>[] kvp = [.. ((IEnumerable<KeyValuePair<string, InMemoryCacheEntry<T>>>)this).Take(array.Length - arrayIndex)];
            for (int i = arrayIndex, ii = 0, len = Math.Min(kvp.Length, array.Length); i < len; i++, ii++)
                array[i] = new(kvp[ii].Key, kvp[ii].Value.Item);
        }

        /// <inheritdoc/>
        IEnumerator<KeyValuePair<string, InMemoryCacheEntry<T>>> IEnumerable<KeyValuePair<string, InMemoryCacheEntry<T>>>.GetEnumerator()
            => GetEnumerator();

        /// <inheritdoc/>
        IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator()
            => ((IEnumerable<KeyValuePair<string, InMemoryCacheEntry<T>>>)this).Select(kvp => new KeyValuePair<string, T>(kvp.Key, kvp.Value.Item)).GetEnumerator();

        /// <inheritdoc/>
        bool IDictionary<string, InMemoryCacheEntry<T>>.Remove(string key)
        {
            if (TryRemove(key) is not InMemoryCacheEntry<T> removed)
                return false;
            if (IsItemDisposable)
                DisposeItem(removed.Item);
            return true;
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<string, InMemoryCacheEntry<T>>>.Remove(KeyValuePair<string, InMemoryCacheEntry<T>> item)
        {
            if (Get(item.Key) is not InMemoryCacheEntry<T> entry || !entry.Item!.Equals(item.Value.Item) || !Remove(entry))
                return false;
            if (IsItemDisposable)
                DisposeItem(entry.Item);
            return true;
        }

        /// <inheritdoc/>
        bool IDictionary<string, T>.Remove(string key)
        {
            if (TryRemove(key) is not InMemoryCacheEntry<T> removed)
                return false;
            if(IsItemDisposable)
                DisposeItem(removed.Item);
            return true;
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
        {
            if (Get(item.Key) is not InMemoryCacheEntry<T> entry || !entry.Item!.Equals(item.Value) || !Remove(entry))
                return false;
            if(IsItemDisposable)
                DisposeItem(entry.Item);
            return true;
        }

        /// <inheritdoc/>
        bool IDictionary<string, InMemoryCacheEntry<T>>.TryGetValue(string key, [NotNullWhen(returnValue: true)] out InMemoryCacheEntry<T>? value)
        {
            value = Get(key);
            return value is not null;
        }

        /// <inheritdoc/>
        bool IDictionary<string, T>.TryGetValue(string key, [NotNullWhen(returnValue: true)] out T? value)
        {
            if (Get(key) is not InMemoryCacheEntry<T> entry)
            {
                value = default;
                return false;
            }
            value = entry.Item!;
            return true;
        }
    }
}

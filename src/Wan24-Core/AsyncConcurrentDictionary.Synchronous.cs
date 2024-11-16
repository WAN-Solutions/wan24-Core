using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    // Synchronous
    public partial class AsyncConcurrentDictionary<tKey, tValue> : IDictionary<tKey, tValue>
    {
        /// <inheritdoc/>
        public virtual tValue this[tKey key]
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                return Dict[key];
            }
            set
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                Dict[key] = value;
            }
        }

        /// <inheritdoc/>
        public virtual ICollection<tKey> Keys
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                return [.. Dict.Keys];
            }
        }

        /// <inheritdoc/>
        public virtual ICollection<tValue> Values
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                return [.. Values];
            }
        }

        /// <inheritdoc/>
        public virtual int Count
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                return Dict.Count;
            }
        }

        /// <inheritdoc/>
        public virtual bool IsReadOnly => false;

        /// <inheritdoc/>
        public virtual void Add(tKey key, tValue value)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            Dict.Add(key, value);
        }

        /// <inheritdoc/>
        public virtual void Add(KeyValuePair<tKey, tValue> item)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            ((ICollection<KeyValuePair<tKey, tValue>>)Dict).Add(item);
        }

        /// <inheritdoc/>
        public virtual void Clear()
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            Dict.Clear();
        }

        /// <inheritdoc/>
        public virtual bool Contains(KeyValuePair<tKey, tValue> item)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            return ((ICollection<KeyValuePair<tKey, tValue>>)Dict).Contains(item);
        }

        /// <inheritdoc/>
        public virtual bool ContainsKey(tKey key)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            return Dict.ContainsKey(key);
        }

        /// <inheritdoc/>
        public virtual void CopyTo(KeyValuePair<tKey, tValue>[] array, int arrayIndex)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            ((ICollection<KeyValuePair<tKey, tValue>>)Dict).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public virtual IEnumerator<KeyValuePair<tKey, tValue>> GetEnumerator()
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            return Dict.ToList().GetEnumerator();
        }

        /// <inheritdoc/>
        public virtual bool Remove(tKey key)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            return Dict.Remove(key);
        }

        /// <inheritdoc/>
        public virtual bool Remove(KeyValuePair<tKey, tValue> item)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            return ((ICollection<KeyValuePair<tKey, tValue>>)Dict).Remove(item);
        }

        /// <inheritdoc/>
        public virtual bool TryGetValue(tKey key, [MaybeNullWhen(false)] out tValue value)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            return Dict.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

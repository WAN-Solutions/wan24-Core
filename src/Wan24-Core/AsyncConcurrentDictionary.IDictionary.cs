using System.Collections;

namespace wan24.Core
{
    // IDictionary
    public partial class AsyncConcurrentDictionary<tKey, tValue> : IDictionary
    {
        /// <inheritdoc/>
        public virtual object? this[object key]
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                return ((IDictionary)Dict)[key];
            }
            set
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                ((IDictionary)Dict)[key] = value;
            }
        }

        /// <inheritdoc/>
        public virtual bool IsFixedSize => false;

        /// <inheritdoc/>
        public virtual bool IsSynchronized => false;

        /// <inheritdoc/>
        public virtual object SyncRoot => throw new NotSupportedException();

        /// <inheritdoc/>
        ICollection IDictionary.Keys
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                return new List<tKey>(Dict.Keys);
            }
        }

        /// <inheritdoc/>
        ICollection IDictionary.Values
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                return new List<tValue>(Dict.Values);
            }
        }

        /// <inheritdoc/>
        public virtual void Add(object key, object? value)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            ((IDictionary)Dict).Add(key, value);
        }

        /// <inheritdoc/>
        public virtual bool Contains(object key)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            return ((IDictionary)Dict).Contains(key);
        }

        /// <inheritdoc/>
        public virtual void CopyTo(Array array, int index)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            ((ICollection)Dict).CopyTo(array, index);
        }

        /// <inheritdoc/>
        public virtual void Remove(object key)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            ((IDictionary)Dict).Remove(key);
        }

        /// <inheritdoc/>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            return ((IDictionary)new Dictionary<tKey, tValue>(Dict)).GetEnumerator();
        }
    }
}

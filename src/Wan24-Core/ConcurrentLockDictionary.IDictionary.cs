using System.Collections;

namespace wan24.Core
{
    // IDictionary
    public partial class ConcurrentLockDictionary<tKey, tValue>
    {
        /// <inheritdoc/>
        public virtual object? this[object key]
        {
            get
            {
                lock (Dict) return ((IDictionary)Dict)[key];
            }
            set
            {
                lock (Dict) ((IDictionary)Dict)[key] = value;
            }
        }

        /// <inheritdoc/>
        public virtual bool IsFixedSize => false;

        /// <inheritdoc/>
        public virtual bool IsSynchronized => true;

        /// <inheritdoc/>
        public virtual object SyncRoot => Dict;

        /// <inheritdoc/>
        ICollection IDictionary.Keys
        {
            get
            {
                lock (Dict) return new List<tKey>(Dict.Keys);
            }
        }

        /// <inheritdoc/>
        ICollection IDictionary.Values
        {
            get
            {
                lock (Dict) return new List<tValue>(Dict.Values);
            }
        }

        /// <inheritdoc/>
        public virtual void Add(object key, object? value)
        {
            lock (Dict) ((IDictionary)Dict).Add(key, value);
        }

        /// <inheritdoc/>
        public virtual bool Contains(object key)
        {
            lock (Dict) return ((IDictionary)Dict).Contains(key);
        }

        /// <inheritdoc/>
        public virtual void CopyTo(Array array, int index)
        {
            lock (Dict) ((ICollection)Dict).CopyTo(array, index);
        }

        /// <inheritdoc/>
        public virtual void Remove(object key)
        {
            lock (Dict) ((IDictionary)Dict).Remove(key);
        }

        /// <inheritdoc/>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            lock (Dict) return ((IDictionary)new Dictionary<tKey, tValue>(Dict)).GetEnumerator();
        }
    }
}

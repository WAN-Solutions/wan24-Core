using System.Collections;
using System.Diagnostics;

namespace wan24.Core
{
    /// <summary>
    /// Concurrent hash set
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public class ConcurrentHashSet<T> : ISet<T>, IReadOnlySet<T>
    {
        /// <summary>
        /// Set
        /// </summary>
        protected readonly HashSet<T> Set;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConcurrentHashSet() => Set = [];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public ConcurrentHashSet(in int capacity) => Set = new(capacity);

        /// <inheritdoc/>
        public virtual int Count => Set.Count;

        /// <inheritdoc/>
        public virtual bool IsReadOnly => ((ICollection<T>)Set).IsReadOnly;

        /// <inheritdoc/>
        public virtual bool Add(T item)
        {
            lock (Set) return ((ISet<T>)Set).Add(item);
        }

        /// <inheritdoc/>
        public virtual void Clear()
        {
            lock (Set) Set.Clear();
        }

        /// <inheritdoc/>
        public virtual bool Contains(T item)
        {
            lock(Set) return Set.Contains(item);
        }

        /// <inheritdoc/>
        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            lock(Set) Set.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public virtual void ExceptWith(IEnumerable<T> other)
        {
            lock(Set) Set.ExceptWith(other);
        }

        /// <inheritdoc/>
        public virtual IEnumerator<T> GetEnumerator()
        {
            lock (Set) return Set.ToList().GetEnumerator();
        }

        /// <inheritdoc/>
        public virtual void IntersectWith(IEnumerable<T> other)
        {
            lock(Set) Set.IntersectWith(other);
        }

        /// <inheritdoc/>
        public virtual bool IsProperSubsetOf(IEnumerable<T> other)
        {
            lock(Set) return Set.IsProperSubsetOf(other);
        }

        /// <inheritdoc/>
        public virtual bool IsProperSupersetOf(IEnumerable<T> other)
        {
            lock(Set) return Set.IsProperSupersetOf(other);
        }

        /// <inheritdoc/>
        public virtual bool IsSubsetOf(IEnumerable<T> other)
        {
            lock(Set) return Set.IsSubsetOf(other);
        }

        /// <inheritdoc/>
        public virtual bool IsSupersetOf(IEnumerable<T> other)
        {
            lock(Set) return Set.IsSupersetOf(other);
        }

        /// <inheritdoc/>
        public virtual bool Overlaps(IEnumerable<T> other)
        {
            lock(Set) return Set.Overlaps(other);
        }

        /// <inheritdoc/>
        public virtual bool Remove(T item)
        {
            lock(Set) return Set.Remove(item);
        }

        /// <inheritdoc/>
        public virtual bool SetEquals(IEnumerable<T> other)
        {
            lock(Set) return Set.SetEquals(other);
        }

        /// <inheritdoc/>
        public virtual void SymmetricExceptWith(IEnumerable<T> other)
        {
            lock(Set) Set.SymmetricExceptWith(other);
        }

        /// <inheritdoc/>
        public virtual void UnionWith(IEnumerable<T> other)
        {
            lock(Set) Set.UnionWith(other);
        }

        /// <inheritdoc/>
        void ICollection<T>.Add(T item) => Add(item);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

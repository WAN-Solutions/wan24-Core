using System.Collections;

namespace wan24.Core
{
    /// <summary>
    /// Concurrent list
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public class ConcurrentList<T> : IList<T>, IReadOnlyList<T>
    {
        /// <summary>
        /// List
        /// </summary>
        protected readonly List<T> List;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConcurrentList() => List = [];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public ConcurrentList(in int capacity) => List = new(capacity);

        /// <inheritdoc/>
        public virtual T this[int index] 
        {
            get
            {
                lock (List) return ((IList<T>)List)[index];
            }
            set
            {
                lock (List) ((IList<T>)List)[index] = value;
            }
        }

        /// <inheritdoc/>
        public virtual int Count => List.Count;

        /// <inheritdoc/>
        public virtual bool IsReadOnly => ((ICollection<T>)List).IsReadOnly;

        /// <inheritdoc/>
        public virtual void Add(T item)
        {
            lock (List) List.Add(item);
        }

        /// <inheritdoc/>
        public virtual void Clear()
        {
            lock (List) List.Clear();
        }

        /// <inheritdoc/>
        public virtual bool Contains(T item)
        {
            lock (List) return List.Contains(item);
        }

        /// <inheritdoc/>
        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            lock (List) List.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public virtual IEnumerator<T> GetEnumerator() => new ConcurrentListEnumerator(this);

        /// <inheritdoc/>
        public virtual int IndexOf(T item)
        {
            lock (List) return List.IndexOf(item);
        }

        /// <inheritdoc/>
        public virtual void Insert(int index, T item)
        {
            lock (List) List.Insert(index, item);
        }

        /// <inheritdoc/>
        public virtual bool Remove(T item)
        {
            lock (List) return List.Remove(item);
        }

        /// <inheritdoc/>
        public virtual void RemoveAt(int index)
        {
            lock (List) List.RemoveAt(index);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Concurrent enumerator
        /// </summary>
        /// <param name="list">List</param>
        protected class ConcurrentListEnumerator(in ConcurrentList<T> list) : IEnumerator<T>
        {
            /// <summary>
            /// List
            /// </summary>
            protected readonly ConcurrentList<T> List = list;
            /// <summary>
            /// Index
            /// </summary>
            protected int Index = -1;
            /// <summary>
            /// Current item
            /// </summary>
            protected T _Current = default!;
            /// <summary>
            /// If <see cref="Current"/> has a value (if <see cref="MoveNext"/> was called)
            /// </summary>
            protected bool HasCurrent = false;

            /// <inheritdoc/>
            public virtual T Current
            {
                get
                {
                    if (!HasCurrent) throw new InvalidOperationException();
                    return _Current;
                }
            }

            /// <inheritdoc/>
            object IEnumerator.Current => Current!;

            /// <inheritdoc/>
            public virtual bool MoveNext()
            {
                if (Index > -1 && !HasCurrent) return false;
                Index++;
                lock (List.List)
                {
                    if (Index >= List.List.Count)
                    {
                        HasCurrent = false;
                        _Current = default!;
                        return false;
                    }
                    HasCurrent = true;
                    _Current = List[Index];
                    return true;
                }
            }

            /// <inheritdoc/>
            public virtual void Reset() => Index = -1;

            /// <inheritdoc/>
            public virtual void Dispose() { }
        }
    }
}

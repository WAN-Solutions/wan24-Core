using System.Collections;

namespace wan24.Core
{
    /// <summary>
    /// Non-generic <see cref="IList"/> wrapper (does cast list items)
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="list">List</param>
    public sealed class NonGenericListWrapper<T>(in IList list) : IList<T>
    {
        /// <summary>
        /// List
        /// </summary>
        private readonly IList List = list;

        /// <inheritdoc/>
        public T this[int index]
        {
            get => (T)List[index]!;
            set => List[index] = value;
        }

        /// <inheritdoc/>
        public int Count => List.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => List.IsReadOnly;

        /// <inheritdoc/>
        public void Add(T item) => List.Add(item);

        /// <inheritdoc/>
        public void Clear() => List.Clear();

        /// <inheritdoc/>
        public bool Contains(T item) => List.Contains(item);

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex) => List.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => new Enumerator(List.GetEnumerator());

        /// <inheritdoc/>
        public int IndexOf(T item) => List.IndexOf(item);

        /// <inheritdoc/>
        public void Insert(int index, T item) => List.Insert(index, item);

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            int count = List.Count;
            List.Remove(item);
            return List.Count < count;
        }

        /// <inheritdoc/>
        public void RemoveAt(int index) => List.RemoveAt(index);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => List.GetEnumerator();

        /// <summary>
        /// Enumerator
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="enumerator">Enumerator</param>
        private sealed class Enumerator(in IEnumerator enumerator) : SimpleDisposableBase(), IEnumerator<T>
        {
            /// <summary>
            /// Enumerator
            /// </summary>
            private readonly IEnumerator ListEnumerator = enumerator;

            /// <inheritdoc/>
            public T Current => (T)ListEnumerator.Current!;

            /// <inheritdoc/>
            object IEnumerator.Current => ListEnumerator.Current;

            /// <inheritdoc/>
            public bool MoveNext() => ListEnumerator.MoveNext();

            /// <inheritdoc/>
            public void Reset() => ListEnumerator.Reset();

            /// <inheritdoc/>
            protected override void Dispose(bool disposing) => ListEnumerator.TryDispose();
        }
    }
}

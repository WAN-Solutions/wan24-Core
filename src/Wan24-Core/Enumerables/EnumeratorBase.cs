using System.Collections;

namespace wan24.Core.Enumerables
{
    /// <summary>
    /// Base class for an enumerator
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public abstract class EnumeratorBase<T> : BasicDisposableBase, IEnumerator<T>, IEnumerable<T>
    {
        /// <summary>
        /// Current item
        /// </summary>
        protected T _Current = default!;
        /// <summary>
        /// Current state
        /// </summary>
        protected int State = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        protected EnumeratorBase() : base() { }

        /// <inheritdoc/>
        public virtual T Current => _Current;

        /// <inheritdoc/>
        object IEnumerator.Current => Current!;

        /// <inheritdoc/>
        public abstract bool MoveNext();

        /// <inheritdoc/>
        public virtual void Reset()
        {
            EnsureUndisposed();
            _Current = default!;
            State = 0;
        }

        /// <inheritdoc/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => this;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _Current = default!;
            State = 0;
        }
    }
}

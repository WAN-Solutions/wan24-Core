namespace wan24.Core.Enumerables
{
    /// <summary>
    /// List enumerator
    /// </summary>
    /// <typeparam name="tList">List type</typeparam>
    /// <typeparam name="tItem">Item type</typeparam>
    /// <typeparam name="tResult">Result type</typeparam>
    public class ListSelectEnumerator<tList, tItem, tResult> : EnumeratorBase<tResult> where tList : IList<tItem>
    {
        /// <summary>
        /// List
        /// </summary>
        protected readonly tList List;
        /// <summary>
        /// Initial list count value
        /// </summary>
        protected readonly int ListCount;
        /// <summary>
        /// Start state
        /// </summary>
        protected readonly int StartState;
        /// <summary>
        /// End state
        /// </summary>
        protected readonly int EndState;
        /// <summary>
        /// Selector
        /// </summary>
        protected readonly Func<tItem, tResult> Selector;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="list">List</param>
        /// <param name="selector">Selector</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        public ListSelectEnumerator(in tList list, in Func<tItem, tResult> selector, in int offset = 0, in int? count = null) : base()
        {
            List = list;
            ListCount = list.Count;
            ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(offset));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, list.Count, nameof(offset));
            StartState = State = offset - 1;
            EndState = offset + (count ?? list.Count);
            if (EndState > list.Count)
            {
                if (count.HasValue) throw new ArgumentOutOfRangeException(nameof(count));
                EndState = list.Count;
            }
            Selector = selector;
        }

        /// <inheritdoc/>
        public override bool MoveNext()
        {
            EnsureUndisposed();
            EnsureInitialListCount();
            if (++State < EndState)
            {
                _Current = Selector(List[State]);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            EnsureUndisposed();
            EnsureInitialListCount();
            base.Reset();
            State = StartState;
        }

        /// <summary>
        /// Ensure the <see cref="List"/> has the initial <see cref="ListCount"/>
        /// </summary>
        protected virtual void EnsureInitialListCount()
        {
            if (List.Count != ListCount) throw new InvalidOperationException("List size changed");
        }
    }
}

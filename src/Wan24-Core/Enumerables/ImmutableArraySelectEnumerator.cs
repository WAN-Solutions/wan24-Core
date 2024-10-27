using System.Collections.Immutable;

namespace wan24.Core.Enumerables
{
    /// <summary>
    /// Array SELECT enumerator
    /// </summary>
    /// <typeparam name="tItem">Item type</typeparam>
    /// <typeparam name="tResult">Result type</typeparam>
    public class ImmutableArraySelectEnumerator<tItem, tResult> : EnumeratorBase<tResult>
    {
        /// <summary>
        /// Array
        /// </summary>
        protected readonly ImmutableArray<tItem> Array;
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
        /// <param name="arr">Array</param>
        /// <param name="selector">Selector</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        public ImmutableArraySelectEnumerator(in ImmutableArray<tItem> arr, in Func<tItem, tResult> selector, in int offset = 0, in int? count = null) : base()
        {
            Array = arr;
            ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(offset));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, arr.Length, nameof(offset));
            StartState = State = offset - 1;
            EndState = offset + (count ?? arr.Length);
            if (EndState > arr.Length)
            {
                if (count.HasValue) throw new ArgumentOutOfRangeException(nameof(count));
                EndState = arr.Length;
            }
            Selector = selector;
        }

        /// <inheritdoc/>
        public override bool MoveNext()
        {
            EnsureUndisposed();
            if (++State < EndState)
            {
                _Current = Selector(Array[State]);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();
            State = StartState;
        }
    }
}

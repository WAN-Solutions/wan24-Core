namespace wan24.Core.Enumerables
{
    /// <summary>
    /// Array enumerator
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public class ArrayEnumerator<T> : EnumeratorBase<T>
    {
        /// <summary>
        /// Array
        /// </summary>
        protected readonly T[] Array;
        /// <summary>
        /// Start state
        /// </summary>
        protected readonly int StartState;
        /// <summary>
        /// End state
        /// </summary>
        protected readonly int EndState;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="arr">Array</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        public ArrayEnumerator(in T[] arr, in int offset = 0, in int? count = null) : base()
        {
            Array = arr;
            ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(offset));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(offset, arr.Length, nameof(offset));
            StartState = State = offset - 1;
            EndState = offset + (count ?? arr.Length);
            if (EndState > arr.Length)
            {
                if (count.HasValue) throw new ArgumentOutOfRangeException(nameof(count));
                EndState = arr.Length;
            }
        }

        /// <inheritdoc/>
        public override bool MoveNext()
        {
            EnsureUndisposed();
            if (++State < EndState)
            {
                _Current = Array[State];
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

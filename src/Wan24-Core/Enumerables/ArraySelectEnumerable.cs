using System.Collections;

namespace wan24.Core.Enumerables
{
    /// <summary>
    /// Array SELECT enumerable
    /// </summary>
    /// <typeparam name="tItem">Item type</typeparam>
    /// <typeparam name="tResult">Result type</typeparam>
    public partial class ArraySelectEnumerable<tItem, tResult> : EnumerableBase<tResult>, ICoreEnumerable<tResult>
    {
        /// <summary>
        /// Empty
        /// </summary>
        public static readonly ArraySelectEnumerable<tItem, tResult> Empty = new([], static i => throw new InvalidProgramException());

        /// <summary>
        /// Array
        /// </summary>
        public readonly tItem[] Array;
        /// <summary>
        /// Offset
        /// </summary>
        public readonly int Offset;
        /// <summary>
        /// Length
        /// </summary>
        public readonly int Length;
        /// <summary>
        /// Selector
        /// </summary>
        protected readonly Func<tItem, tResult> Selector;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="arr">Array</param>
        /// <param name="selector">Predicate</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        public ArraySelectEnumerable(in tItem[] arr, in Func<tItem, tResult> selector, in int offset = 0, in int? count = null) : base()
        {
            Array = arr;
            ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(offset));
            int len = arr.Length;
            ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, len, nameof(offset));
            Offset = offset;
            Length = count ?? (len - offset);
            if (Length > len)
            {
                if (count.HasValue) throw new ArgumentOutOfRangeException(nameof(count));
                Length = len - offset;
            }
            Selector = selector;
        }

        /// <inheritdoc/>
        public virtual IEnumerator<tResult> GetEnumerator() => new ArraySelectEnumerator<tItem, tResult>(Array, Selector, Offset, Length);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

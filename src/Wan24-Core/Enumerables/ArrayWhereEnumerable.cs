using System.Collections;

namespace wan24.Core.Enumerables
{
    /// <summary>
    /// Array WHERE enumerable
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public partial class ArrayWhereEnumerable<T> : EnumerableBase<T>, ICoreEnumerable<T>
    {
        /// <summary>
        /// Empty
        /// </summary>
        public static readonly ArrayWhereEnumerable<T> Empty = new([], static i => throw new InvalidProgramException());

        /// <summary>
        /// Array
        /// </summary>
        public readonly T[] Array;
        /// <summary>
        /// Offset
        /// </summary>
        public readonly int Offset;
        /// <summary>
        /// Length
        /// </summary>
        public readonly int Length;
        /// <summary>
        /// Predicate
        /// </summary>
        protected readonly Func<T, bool> Predicate;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        public ArrayWhereEnumerable(in T[] arr, in Func<T, bool> predicate, in int offset = 0, in int? count = null) : base()
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
            Predicate = predicate;
        }

        /// <inheritdoc/>
        public virtual IEnumerator<T> GetEnumerator() => new ArrayWhereEnumerator<T>(Array, Predicate, Offset, Length);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

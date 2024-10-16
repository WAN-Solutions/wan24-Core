using System.Collections;

namespace wan24.Core.Enumerables
{
    /// <summary>
    /// Array enumerable
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public partial class ArrayEnumerable<T> : ICoreEnumerable<T>
    {
        /// <summary>
        /// Empty
        /// </summary>
        public static readonly ArrayEnumerable<T> Empty = new([]);

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
        /// Constructor
        /// </summary>
        /// <param name="arr">Array</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        public ArrayEnumerable(in T[] arr, in int offset = 0, in int? count = null)
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
        }

        /// <inheritdoc/>
        public virtual IEnumerator<T> GetEnumerator() => new ArrayEnumerator<T>(Array, Offset, Length);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

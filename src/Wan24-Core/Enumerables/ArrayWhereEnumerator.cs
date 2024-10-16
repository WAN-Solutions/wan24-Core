namespace wan24.Core.Enumerables
{
    /// <summary>
    /// Array WHERE enumerator
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="arr">Array</param>
    /// <param name="predicate">Predicate</param>
    /// <param name="offset">Offset</param>
    /// <param name="count">Count</param>
    public class ArrayWhereEnumerator<T>(in T[] arr, in Func<T, bool> predicate, in int offset = 0, in int? count = null)
        : ArrayEnumerator<T>(arr, offset, count)
    {
        /// <summary>
        /// Predicate
        /// </summary>
        protected readonly Func<T, bool> Predicate = predicate;

        /// <inheritdoc/>
        public override bool MoveNext()
        {
            while (base.MoveNext())
                if (Predicate(_Current))
                    return true;
            return false;
        }
    }
}

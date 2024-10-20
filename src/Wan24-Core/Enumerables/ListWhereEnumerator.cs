namespace wan24.Core.Enumerables
{
    /// <summary>
    /// List WHERE enumerator
    /// </summary>
    /// <typeparam name="tList">List type</typeparam>
    /// <typeparam name="tItem">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="list">List</param>
    /// <param name="predicate">Predicate</param>
    /// <param name="offset">Offset</param>
    /// <param name="count">Count</param>
    public class ListWhereEnumerator<tList, tItem>(in tList list, in Func<tItem, bool> predicate, in int offset = 0, in int? count = null)
        : ListEnumerator<tList, tItem>(list, offset, count)
        where tList : IList<tItem>
    {
        /// <summary>
        /// Predicate
        /// </summary>
        protected readonly Func<tItem, bool> Predicate = predicate;

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

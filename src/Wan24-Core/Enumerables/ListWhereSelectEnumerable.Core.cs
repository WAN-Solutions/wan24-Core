namespace wan24.Core.Enumerables
{
    // ICoreEnumerable implementation
    public partial class ListWhereSelectEnumerable<tList, tItem, tResult>
    {
        /// <inheritdoc/>
        ICoreEnumerable<tResult1> ICoreEnumerable<tResult>.Select<tResult1>(Func<tResult, tResult1> selector) => Select(selector);

        /// <inheritdoc/>
        ICoreEnumerable<tResult> ICoreEnumerable<tResult>.Skip(int count) => Skip(count);

        /// <inheritdoc/>
        ICoreEnumerable<tResult> ICoreEnumerable<tResult>.SkipWhile(Func<tResult, bool> predicate) => SkipWhile(predicate);

        /// <inheritdoc/>
        ICoreEnumerable<tResult> ICoreEnumerable<tResult>.Take(int count) => Take(count);

        /// <inheritdoc/>
        ICoreEnumerable<tResult> ICoreEnumerable<tResult>.TakeWhile(Func<tResult, bool> predicate) => TakeWhile(predicate);

        /// <inheritdoc/>
        ICoreEnumerable<tResult> ICoreEnumerable<tResult>.Where(Func<tResult, bool> predicate) => new EnumerableAdapter<IEnumerable<tResult>, tResult>(this.Where(predicate));
    }
}

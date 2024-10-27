namespace wan24.Core.Enumerables
{
    // ICoreEnumerable implementation
    public partial class ListEnumerable<tList, tItem>
    {
        /// <inheritdoc/>
        ICoreEnumerable<tResult> ICoreEnumerable<tItem>.Select<tResult>(Func<tItem, tResult> selector) => Select(selector);

        /// <inheritdoc/>
        ICoreEnumerable<tItem> ICoreEnumerable<tItem>.Skip(int count) => Skip(count);

        /// <inheritdoc/>
        ICoreEnumerable<tItem> ICoreEnumerable<tItem>.SkipWhile(Func<tItem, bool> predicate) => SkipWhile(predicate);

        /// <inheritdoc/>
        ICoreEnumerable<tItem> ICoreEnumerable<tItem>.Take(int count) => Take(count);

        /// <inheritdoc/>
        ICoreEnumerable<tItem> ICoreEnumerable<tItem>.TakeWhile(Func<tItem, bool> predicate) => TakeWhile(predicate);

        /// <inheritdoc/>
        ICoreEnumerable<tItem> ICoreEnumerable<tItem>.Where(Func<tItem, bool> predicate) => Where(predicate);
    }
}

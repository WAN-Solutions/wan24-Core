namespace wan24.Core.Enumerables
{
    // ICoreEnumerable implementation
    public partial class ArrayEnumerable<T>
    {
        /// <inheritdoc/>
        ICoreEnumerable<tResult> ICoreEnumerable<T>.Select<tResult>(Func<T, tResult> selector) => Select(selector);

        /// <inheritdoc/>
        ICoreEnumerable<T> ICoreEnumerable<T>.Skip(int count) => Skip(count);

        /// <inheritdoc/>
        ICoreEnumerable<T> ICoreEnumerable<T>.SkipWhile(Func<T, bool> predicate) => SkipWhile(predicate);

        /// <inheritdoc/>
        ICoreEnumerable<T> ICoreEnumerable<T>.Take(int count) => Take(count);

        /// <inheritdoc/>
        ICoreEnumerable<T> ICoreEnumerable<T>.TakeWhile(Func<T, bool> predicate) => TakeWhile(predicate);

        /// <inheritdoc/>
        ICoreEnumerable<T> ICoreEnumerable<T>.Where(Func<T, bool> predicate) => Where(predicate);
    }
}

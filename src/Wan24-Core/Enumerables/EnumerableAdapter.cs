using System.Collections;

namespace wan24.Core.Enumerables
{
    /// <summary>
    /// <see cref="IEnumerable{T}"/> to <see cref="ICoreEnumerable{T}"/> adapter
    /// </summary>
    /// <typeparam name="tEnumerable">Enumerable type</typeparam>
    /// <typeparam name="tItem">Item type</typeparam>
    public class EnumerableAdapter<tEnumerable, tItem>(in tEnumerable enumerable) : EnumerableBase<tItem>, ICoreEnumerable<tItem> where tEnumerable : IEnumerable<tItem>
    {
        /// <summary>
        /// Enumerable
        /// </summary>
        public virtual tEnumerable Enumerable { get; } = enumerable;

        /// <inheritdoc/>
        public virtual bool All(in Func<tItem, bool> predicate) => Enumerable.All(predicate);

        /// <inheritdoc/>
        public virtual Task<bool> AllAsync(Func<tItem, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
            => Enumerable.AllAsync(predicate, cancellationToken);

        /// <inheritdoc/>
        public virtual bool Any() => Enumerable.Any();

        /// <inheritdoc/>
        public virtual bool Any(Func<tItem, bool> predicate) => Enumerable.Any(predicate);

        /// <inheritdoc/>
        public virtual Task<bool> AnyAsync(Func<tItem, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
            => Enumerable.AnyAsync(predicate, cancellationToken);

        /// <inheritdoc/>
        public virtual bool Contains(tItem obj) => Enumerable.Contains(obj);

        /// <inheritdoc/>
        public virtual bool ContainsAll(params tItem[] objs) => Enumerable.ContainsAll(objs);

        /// <inheritdoc/>
        public virtual bool ContainsAny(params tItem[] objs) => Enumerable.ContainsAny(objs);

        /// <inheritdoc/>
        public virtual bool ContainsAtLeast(in int count)
        {
            int cnt = 0;
            foreach (tItem _ in Enumerable)
                if (++cnt >= count)
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool ContainsAtMost(in int count)
        {
            int cnt = 0;
            foreach (tItem _ in Enumerable)
                if (++cnt > count)
                    return false;
            return true;
        }

        /// <inheritdoc/>
        public virtual int Count() => Enumerable.Count();

        /// <inheritdoc/>
        public virtual int Count(Func<tItem, bool> predicate) => Enumerable.Count(predicate);

        /// <inheritdoc/>
        public virtual Task<int> CountAsync(Func<tItem, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
            => Enumerable.CountAsync(predicate, cancellationToken);

        /// <inheritdoc/>
        public virtual int DiscardAll(in bool dispose = true) => Enumerable.DiscardAll(dispose);

        /// <inheritdoc/>
        public virtual Task<int> DiscardAllAsync(bool dispose = true) => DiscardAllAsync(dispose);

        /// <inheritdoc/>
        public virtual IEnumerable<tItem> Distinct() => Enumerable.Distinct();

        /// <inheritdoc/>
        public virtual IEnumerable<tItem> DistinctBy<tKey>(Func<tItem, tKey> keySelector) => Enumerable.DistinctBy(keySelector);

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<tItem> DistinctByAsync<tKey>(Func<tItem, CancellationToken, Task<tKey>> keySelector, CancellationToken cancellationToken = default)
            => Enumerable.DistinctByAsync(keySelector, cancellationToken);

        /// <inheritdoc/>
        public virtual void ExecuteForAll(in Action<tItem> action) => Enumerable.ExecuteForAll(action);

        /// <inheritdoc/>
        public virtual IEnumerable<tReturn> ExecuteForAll<tReturn>(Func<tItem, EnumerableExtensions.ExecuteResult<tReturn>> action)
            => Enumerable.ExecuteForAll(action);

        /// <inheritdoc/>
        public virtual Task ExecuteForAllAsync(Func<tItem, CancellationToken, Task> action, CancellationToken cancellationToken = default)
            => Enumerable.ExecuteForAllAsync(action, cancellationToken);

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<tReturn> ExecuteForAllAsync<tReturn>(
            Func<tItem, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tReturn>>> action,
            CancellationToken cancellationToken = default
            )
            => Enumerable.ExecuteForAllAsync(action, cancellationToken);

        /// <inheritdoc/>
        public virtual tItem First() => Enumerable.First();

        /// <inheritdoc/>
        public virtual tItem First(Func<tItem, bool> predicate) => Enumerable.First(predicate);

        /// <inheritdoc/>
        public virtual Task<tItem> FirstAsync(Func<tItem, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
            => Enumerable.FirstAsync(predicate, cancellationToken);

        /// <inheritdoc/>
        public override tItem FirstOrDefault(tItem defaultValue) => Enumerable.FirstOrDefault(defaultValue);

        /// <inheritdoc/>
        public override tItem FirstOrDefault(Func<tItem, bool> predicate, tItem defaultValue) => Enumerable.FirstOrDefault(predicate, defaultValue);

        /// <inheritdoc/>
        public override Task<tItem> FirstOrDefaultAsync(Func<tItem, CancellationToken, Task<bool>> predicate, tItem defaultValue, CancellationToken cancellationToken = default)
            => Enumerable.FirstOrDefaultAsync(predicate, defaultValue, cancellationToken);

        /// <inheritdoc/>
        public virtual IEnumerator<tItem> GetEnumerator() => Enumerable.GetEnumerator();

        /// <inheritdoc/>
        public virtual ICoreEnumerable<tResult> Select<tResult>(Func<tItem, tResult> selector) => new EnumerableAdapter<IEnumerable<tResult>, tResult>(Enumerable.Select(selector));

        /// <inheritdoc/>
        public virtual ICoreEnumerable<tItem> Skip(int count) => new EnumerableAdapter<IEnumerable<tItem>, tItem>(Enumerable.Skip(count));

        /// <inheritdoc/>
        public virtual ICoreEnumerable<tItem> SkipWhile(Func<tItem, bool> predicate) => new EnumerableAdapter<IEnumerable<tItem>, tItem>(Enumerable.SkipWhile(predicate));

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<tItem> SkipWhileAsync(Func<tItem, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
            => Enumerable.SkipWhileAsync(predicate, cancellationToken);

        /// <inheritdoc/>
        public virtual ICoreEnumerable<tItem> Take(int count) => new EnumerableAdapter<IEnumerable<tItem>, tItem>(Enumerable.Take(count));

        /// <inheritdoc/>
        public virtual ICoreEnumerable<tItem> TakeWhile(Func<tItem, bool> predicate) => new EnumerableAdapter<IEnumerable<tItem>, tItem>(Enumerable.TakeWhile(predicate));

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<tItem> TakeWhileAsync(Func<tItem, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
            => Enumerable.TakeWhileAsync(predicate, cancellationToken);

        /// <inheritdoc/>
        public virtual ICoreEnumerable<tItem> Where(Func<tItem, bool> predicate) => new EnumerableAdapter<IEnumerable<tItem>, tItem>(Enumerable.Where(predicate));

        /// <inheritdoc/>
        public virtual tItem[] ToArray() => Enumerable.ToArray();

        /// <inheritdoc/>
        public virtual int ToBuffer(in Span<tItem> buffer)
        {
            int index = -1;
            foreach(tItem item in Enumerable)
            {
                if (++index > buffer.Length) throw new OutOfMemoryException();
                buffer[index] = item;
            }
            return index + 1;
        }

        /// <inheritdoc/>
        public virtual List<tItem> ToList() => Enumerable.ToList();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => Enumerable.GetEnumerator();
    }
}

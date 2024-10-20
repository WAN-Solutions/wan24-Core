using System.Runtime.CompilerServices;

namespace wan24.Core.Enumerables
{
    // Extensions
    public partial class ArrayEnumerable<T>
    {
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ArrayWhereEnumerable<T> Where(Func<T, bool> predicate) => new(Array, predicate, Offset, Length);

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="selector">Selector</param>
        /// <returns>Enumerable</returns>
        public virtual ArraySelectEnumerable<T, tResult> Select<tResult>(Func<T, tResult> selector) => new(Array, selector, Offset, Length);

        /// <inheritdoc/>
        public virtual int Count() => Length;

        /// <inheritdoc/>
        public virtual int Count(Func<T, bool> predicate)
        {
            Span<T> data = Array;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (predicate(data[i]))
                    res++;
            return res;
        }

        /// <inheritdoc/>
        public virtual async Task<int> CountAsync(Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            T[] data = Array;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                if (await predicate(data[i], cancellationToken).DynamicContext())
                    res++;
            return res;
        }

        /// <inheritdoc/>
        public virtual bool Contains(T obj)
        {
            if (Length == 0) return false;
            Span<T> data = Array;
            bool isItemNull = obj is null;
            T item;
            for (int i = Offset, len = i + Length, hc = obj?.GetHashCode() ?? 0; i < len; i++)
                if ((isItemNull && data[i] is null) || (!isItemNull && (item = data[i]) is not null && hc == item.GetHashCode() && item.Equals(obj)))
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool ContainsAll(params T[] objs)
        {
            if (Length < 1) return false;
            int objsLen = objs.Length;// Number of objects to look for
            if (objsLen == 0 || objsLen > Length) return false;
            Span<T> objsSpan = objs.AsSpan();// Span of objects to look for
            int i = 0;// Index counter
            using RentedArrayRefStruct<int> hashCodes = new(objsLen, clean: false);// Hash codes of objects to look for
            Span<int> hashCodesSpan = hashCodes.Span;// Hash codes span
            for (; i < objsLen; hashCodes[i] = objsSpan[i]?.GetHashCode() ?? 0, i++) ;
            using RentedArrayRefStruct<bool> seen = new(objsLen);// Found object indicators
            Span<bool> seenSpan = seen.Span;// Found object indicators span
            bool isItemNull;// If the current item is NULL
            T item,// Current item
                obj;// Current object
            Span<T> data = Array;
            i = Offset;
            for (int j, len = i + Length, seenCnt = 0, hc; i < len; i++)
                for (item = data[i], j = 0, hc = item?.GetHashCode() ?? 0; j < objsLen; j++)
                    if (
                        !seenSpan[j] && hashCodesSpan[j] == hc &&
                        (
                            ((isItemNull = item is null) && objsSpan[j] is null) ||
                            (!isItemNull && (obj = objsSpan[j]) is not null && obj.Equals(item))
                        )
                        )
                    {
                        if (++seenCnt >= objsLen) return true;
                        seenSpan[j] = true;
                    }
            return false;
        }

        /// <inheritdoc/>
        public virtual bool ContainsAny(params T[] objs)
        {
            if (Length < 1) return false;
            int objsLen = objs.Length;// Number of objects to look for
            if (objsLen == 0 || objsLen > Length) return false;
            Span<T> objsSpan = objs.AsSpan();// Span of objects to look for
            int i = 0;// Index counter
            using RentedArrayRefStruct<int> hashCodes = new(objsLen, clean: false);// Hash codes of objects to look for
            Span<int> hashCodesSpan = hashCodes.Span;// Hash codes span
            for (; i < objsLen; hashCodes[i] = objsSpan[i]?.GetHashCode() ?? 0, i++) ;
            bool isItemNull;// If the current item is NULL
            T item,// Current item
                obj;// Current object
            Span<T> data = Array;
            i = Offset;
            for (int j, len = i + Length, hc; i < len; i++)
                for (item = data[i], j = 0, hc = item?.GetHashCode() ?? 0; j < objsLen; j++)
                    if (
                        hashCodesSpan[j] == hc &&
                        (
                            ((isItemNull = item is null) && objsSpan[j] is null) ||
                            (!isItemNull && (obj = objsSpan[j]) is not null && obj.Equals(item))
                        )
                        )
                        return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool ContainsAtLeast(in int count) => Length >= count;

        /// <inheritdoc/>
        public virtual bool ContainsAtMost(in int count) => Length <= count;

        /// <inheritdoc/>
        public virtual bool All(in Func<T, bool> predicate) => Count(predicate) == Length;

        /// <inheritdoc/>
        public virtual async Task<bool> AllAsync(Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
            => await CountAsync(predicate, cancellationToken).DynamicContext() == Length;

        /// <inheritdoc/>
        public virtual bool Any() => Length > 0;

        /// <inheritdoc/>
        public virtual bool Any(Func<T, bool> predicate)
        {
            Span<T> data = Array;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (predicate(data[i]))
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> AnyAsync(Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            T[] data = Array;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                if (await predicate(data[i], cancellationToken).DynamicContext())
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual void ExecuteForAll(in Action<T> action)
        {
            Span<T> data = Array;
            for (int i = Offset, len = i + Length; i < len; action(data[i]), i++) ;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<tReturn> ExecuteForAll<tReturn>(Func<T, EnumerableExtensions.ExecuteResult<tReturn>> action)
        {
            T[] data = Array;
            EnumerableExtensions.ExecuteResult<tReturn> returnValue;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                returnValue = action(data[i]);
                if (!returnValue.Next) break;
                if (returnValue) yield return returnValue.Result;
            }
        }

        /// <inheritdoc/>
        public virtual async Task ExecuteForAllAsync(Func<T, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            T[] data = Array;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); await action(data[i], cancellationToken).DynamicContext(), i++) ;
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<tReturn> ExecuteForAllAsync<tReturn>(
            Func<T, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tReturn>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            T[] data = Array;
            EnumerableExtensions.ExecuteResult<tReturn> returnValue;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
            {
                returnValue = await action(data[i], cancellationToken).DynamicContext();
                if (!returnValue.Next) break;
                if (returnValue) yield return returnValue.Result;
            }
        }

        /// <inheritdoc/>
        public virtual int DiscardAll(in bool dispose = true)
        {
            if (!dispose) return Length;
            Span<T> data = Array;
            for (int i = Offset, len = i + Length; i < len; data[i]?.TryDispose(), i++) ;
            return Length;
        }

        /// <inheritdoc/>
        public virtual async Task<int> DiscardAllAsync(bool dispose = true)
        {
            if (!dispose) return Length;
            T[] data = Array;
            T item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (item is not null) await item.TryDisposeAsync().DynamicContext();
            }
            return Length;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<T> Distinct()
        {
            using RentedArrayStructSimple<int> seenHashCodes = new(Length, clean: false);// Seen items hash codes
            int[] seenHashCodesArray = seenHashCodes.Array;// Seen items hash codes array
            using RentedArrayStructSimple<T> seen = new(Length, clean: false);// Seen items
            T[] seenArray = seen.Array;// Seen items array
            bool useItem,// If to use the current item
                isItemNull;// If the current item is NULL
            T item,// Current item
                seenItem;// Seen item
            T[] data = Array;
            for (int i = Offset, len = i + Length, j, seenCnt = 0, hc; i < len; i++)
            {
                for (item = data[i], hc = item?.GetHashCode() ?? 0, useItem = true, j = 0; j < seenCnt; j++)
                    if (seenHashCodes[j] == hc && (((isItemNull = item is null) && seen[j] is null) || (!isItemNull && (seenItem = seen[j]) is not null && seenItem.Equals(item))))
                    {
                        useItem = false;
                        break;
                    }
                if (!useItem) continue;
                seenHashCodesArray[seenCnt] = hc;
                seenArray[seenCnt] = item;
                seenCnt++;
                yield return item;
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<T> DistinctBy<tKey>(Func<T, tKey> keySelector)
        {
            using RentedArrayStructSimple<int> seenHashCodes = new(Length, clean: false);// Seen keys hash codes
            int[] seenHashCodesArray = seenHashCodes.Array;// Seen keys hash codes array
            using RentedArrayStructSimple<tKey> seen = new(Length, clean: false);// Seen keys
            tKey[] seenArray = seen.Array;// Seen keys array
            bool useItem,// If to use the current item
                isKeyNull;// If the current key is NULL
            T item;// Current item
            tKey key,// Current key
                seenKey;// Seen key
            T[] data = Array;
            for (int i = Offset, len = i + Length, j, seenCnt = 0, hc; i < len; i++)
            {
                for (item = data[i], key = keySelector(item), hc = key?.GetHashCode() ?? 0, useItem = true, j = 0; j < seenCnt; j++)
                    if (seenHashCodes[j] == hc && (((isKeyNull = key is null) && seen[j] is null) || (!isKeyNull && (seenKey = seen[j]) is not null && seenKey.Equals(key))))
                    {
                        useItem = false;
                        break;
                    }
                if (!useItem) continue;
                seenHashCodesArray[seenCnt] = hc;
                seenArray[seenCnt] = key;
                seenCnt++;
                yield return item;
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<T> DistinctByAsync<tKey>(
            Func<T, CancellationToken, Task<tKey>> keySelector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            using RentedArrayStructSimple<int> seenHashCodes = new(Length, clean: false);// Seen keys hash codes
            int[] seenHashCodesArray = seenHashCodes.Array;// Seen keys hash codes array
            using RentedArrayStructSimple<tKey> seen = new(Length, clean: false);// Seen keys
            tKey[] seenArray = seen.Array;// Seen keys array
            bool useItem,// If to use the current item
                isKeyNull;// If the current key is NULL
            T item;// Current item
            tKey key,// Current key
                seenKey;// Seen key
            T[] data = Array;
            for (int i = Offset, len = i + Length, j, seenCnt = 0, hc; i < len; i++)
            {
                for (item = data[i], key = await keySelector(item, cancellationToken).DynamicContext(), hc = key?.GetHashCode() ?? 0, useItem = true, j = 0; j < seenCnt; j++)
                    if (seenHashCodes[j] == hc && (((isKeyNull = key is null) && seen[j] is null) || (!isKeyNull && (seenKey = seen[j]) is not null && seenKey.Equals(key))))
                    {
                        useItem = false;
                        break;
                    }
                if (!useItem) continue;
                seenHashCodesArray[seenCnt] = hc;
                seenArray[seenCnt] = key;
                seenCnt++;
                yield return item;
            }
        }

        /// <inheritdoc/>
        public virtual T First() => Length > 0 ? Array[Offset] : throw new InvalidOperationException("Sequence contains no elements");

        /// <inheritdoc/>
        public virtual T First(Func<T, bool> predicate)
        {
            Span<T> data = Array;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (predicate(data[i]))
                    return data[i];
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public virtual async Task<T> FirstAsync(Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            T[] data = Array;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                if (await predicate(data[i], cancellationToken).DynamicContext())
                    return data[i];
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public virtual T FirstOrDefault(T defaultValue) => Length > 0 ? Array[Offset] : defaultValue;

        /// <inheritdoc/>
        public virtual T FirstOrDefault(Func<T, bool> predicate, T defaultValue)
        {
            Span<T> data = Array;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (predicate(data[i]))
                    return data[i];
            return defaultValue;
        }

        /// <inheritdoc/>
        public virtual async Task<T> FirstOrDefaultAsync(Func<T, CancellationToken, Task<bool>> predicate, T defaultValue, CancellationToken cancellationToken = default)
        {
            T[] data = Array;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                if (await predicate(data[i], cancellationToken).DynamicContext())
                    return data[i];
            return defaultValue;
        }

        /// <summary>
        /// Skip
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Enumerable</returns>
        public virtual ArrayEnumerable<T> Skip(int count)
            => count >= Length
                ? Empty
                : count < 1
                    ? this
                    : new(Array, Offset + count, Length - count);

        /// <summary>
        /// Skip while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ArrayEnumerable<T> SkipWhile(Func<T, bool> predicate)
        {
            Span<T> data = Array;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (!predicate(data[i]))
                    return new(Array, i, len - i);
            return Empty;
        }

        /// <summary>
        /// Skip while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public virtual async IAsyncEnumerable<T> SkipWhileAsync(Func<T, CancellationToken, Task<bool>> predicate, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            T[] data = Array;
            bool skip = true;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.IsCancellationRequested; i++)
            {
                if (skip)
                {
                    if (await predicate(data[i], cancellationToken).DynamicContext()) continue;
                    skip = false;
                }
                yield return data[i];
            }
        }

        /// <summary>
        /// Take
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Enumerable</returns>
        public virtual ArrayEnumerable<T> Take(int count)
            => count < 1
                ? Empty
                : count < Length
                    ? new(Array, Offset, count)
                    : this;

        /// <summary>
        /// Take while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ArrayEnumerable<T> TakeWhile(Func<T, bool> predicate)
        {
            Span<T> data = Array;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (!predicate(data[i]))
                    return i == Offset
                        ? Empty
                        : new(Array, Offset, i - Offset);
            return this;
        }

        /// <summary>
        /// Take while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public virtual async IAsyncEnumerable<T> TakeWhileAsync(Func<T, CancellationToken, Task<bool>> predicate, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            T[] data = Array;
            T item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.IsCancellationRequested; i++)
            {
                item = data[i];
                if (!await predicate(item, cancellationToken).DynamicContext()) yield break;
                yield return item;
            }
        }

        /// <inheritdoc/>
        public T[] ToArray()
        {
            if (Length < 1) return [];
            T[] res = new T[Length];
            Array.AsSpan(Offset, Length).CopyTo(res);
            return res;
        }

        /// <inheritdoc/>
        public int ToBuffer(in Span<T> buffer)
        {
            if (Length < 1) return 0;
            if (buffer.Length < Length) throw new OutOfMemoryException("Buffer to small");
            Array.AsSpan(Offset, Length).CopyTo(buffer);
            return Length;
        }

        /// <inheritdoc/>
        public List<T> ToList()
        {
            if (Length < 1) return [];
            List<T> res = new(Length);
            Span<T> data = Array;
            for (int i = Offset, len = i + Length; i < len; res.Add(data[i]), i++) ;
            return res;
        }
    }
}

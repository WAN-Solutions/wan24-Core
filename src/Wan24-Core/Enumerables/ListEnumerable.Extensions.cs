using System.Runtime.CompilerServices;

namespace wan24.Core.Enumerables
{
    // Extensions
    public partial class ListEnumerable<tList, tItem>
    {
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ListWhereEnumerable<tList, tItem> Where(Func<tItem, bool> predicate)
        {
            EnsureInitialListCount();
            return new(List, predicate, Offset, Length);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="selector">Selector</param>
        /// <returns>Enumerable</returns>
        public virtual ListSelectEnumerable<tList, tItem, tResult> Select<tResult>(Func<tItem, tResult> selector)
        {
            EnsureInitialListCount();
            return new(List, selector, Offset, Length);
        }

        /// <inheritdoc/>
        public virtual int Count()
        {
            EnsureInitialListCount();
            return Length;
        }

        /// <inheritdoc/>
        public virtual int Count(Func<tItem, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (predicate(data[i]))
                    res++;
            return res;
        }

        /// <inheritdoc/>
        public virtual async Task<int> CountAsync(Func<tItem, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                if (await predicate(data[i], cancellationToken).DynamicContext())
                    res++;
            return res;
        }

        /// <inheritdoc/>
        public virtual bool Contains(tItem obj)
        {
            EnsureInitialListCount();
            if (Length == 0) return false;
            tList data = List;
            bool isItemNull = obj is null;
            tItem item;
            for (int i = Offset, len = i + Length, hc = obj?.GetHashCode() ?? 0; i < len; i++)
                if ((isItemNull && data[i] is null) || (!isItemNull && (item = data[i]) is not null && hc == item.GetHashCode() && item.Equals(obj)))
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool ContainsAll(params tItem[] objs)
        {
            EnsureInitialListCount();
            if (Length < 1) return false;
            int objsLen = objs.Length;// Number of objects to look for
            if (objsLen == 0 || objsLen > Length) return false;
            Span<tItem> objsSpan = objs.AsSpan();// Span of objects to look for
            int i = 0;// Index counter
            using RentedArrayRefStruct<int> hashCodes = new(objsLen, clean: false);// Hash codes of objects to look for
            Span<int> hashCodesSpan = hashCodes.Span;// Hash codes span
            for (; i < objsLen; hashCodes[i] = objsSpan[i]?.GetHashCode() ?? 0, i++) ;
            using RentedArrayRefStruct<bool> seen = new(objsLen);// Found object indicators
            Span<bool> seenSpan = seen.Span;// Found object indicators span
            bool isItemNull;// If the current item is NULL
            tItem item,// Current item
                obj;// Current object
            tList data = List;
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
        public virtual bool ContainsAny(params tItem[] objs)
        {
            EnsureInitialListCount();
            if (Length < 1) return false;
            int objsLen = objs.Length;// Number of objects to look for
            if (objsLen == 0 || objsLen > Length) return false;
            Span<tItem> objsSpan = objs.AsSpan();// Span of objects to look for
            int i = 0;// Index counter
            using RentedArrayRefStruct<int> hashCodes = new(objsLen, clean: false);// Hash codes of objects to look for
            Span<int> hashCodesSpan = hashCodes.Span;// Hash codes span
            for (; i < objsLen; hashCodes[i] = objsSpan[i]?.GetHashCode() ?? 0, i++) ;
            bool isItemNull;// If the current item is NULL
            tItem item,// Current item
                obj;// Current object
            tList data = List;
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
        public virtual bool ContainsAtLeast(in int count)
        {
            EnsureInitialListCount();
            return Length >= count;
        }

        /// <inheritdoc/>
        public virtual bool ContainsAtMost(in int count)
        {
            EnsureInitialListCount();
            return Length <= count;
        }

        /// <inheritdoc/>
        public virtual bool All(in Func<tItem, bool> predicate)
        {
            EnsureInitialListCount();
            return Count(predicate) == Length;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> AllAsync(Func<tItem, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            return await CountAsync(predicate, cancellationToken).DynamicContext() == Length;
        }

        /// <inheritdoc/>
        public virtual bool Any()
        {
            EnsureInitialListCount();
            return Length > 0;
        }

        /// <inheritdoc/>
        public virtual bool Any(Func<tItem, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (predicate(data[i]))
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> AnyAsync(Func<tItem, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                if (await predicate(data[i], cancellationToken).DynamicContext())
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual void ExecuteForAll(in Action<tItem> action)
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; action(data[i]), i++) ;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<tReturn> ExecuteForAll<tReturn>(Func<tItem, EnumerableExtensions.ExecuteResult<tReturn>> action)
        {
            EnsureInitialListCount();
            tList data = List;
            EnumerableExtensions.ExecuteResult<tReturn> returnValue;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                returnValue = action(data[i]);
                if (!returnValue.Next) break;
                if (returnValue) yield return returnValue.Result;
            }
        }

        /// <inheritdoc/>
        public virtual async Task ExecuteForAllAsync(Func<tItem, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); await action(data[i], cancellationToken).DynamicContext(), i++) ;
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<tReturn> ExecuteForAllAsync<tReturn>(
            Func<tItem, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tReturn>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            EnsureInitialListCount();
            tList data = List;
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
            EnsureInitialListCount();
            if (!dispose) return Length;
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; data[i]?.TryDispose(), i++) ;
            return Length;
        }

        /// <inheritdoc/>
        public virtual async Task<int> DiscardAllAsync(bool dispose = true)
        {
            EnsureInitialListCount();
            if (!dispose) return Length;
            tList data = List;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (item is not null) await item.TryDisposeAsync().DynamicContext();
            }
            return Length;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<tItem> Distinct()
        {
            EnsureInitialListCount();
            using RentedArrayStructSimple<int> seenHashCodes = new(Length, clean: false);// Seen items hash codes
            int[] seenHashCodesArray = seenHashCodes.Array;// Seen items hash codes array
            using RentedArrayStructSimple<tItem> seen = new(Length, clean: false);// Seen items
            tItem[] seenArray = seen.Array;// Seen items array
            bool useItem,// If to use the current item
                isItemNull;// If the current item is NULL
            tItem item,// Current item
                seenItem;// Seen item
            tList data = List;
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
        public virtual IEnumerable<tItem> DistinctBy<tKey>(Func<tItem, tKey> keySelector)
        {
            EnsureInitialListCount();
            using RentedArrayStructSimple<int> seenHashCodes = new(Length, clean: false);// Seen keys hash codes
            int[] seenHashCodesArray = seenHashCodes.Array;// Seen keys hash codes array
            using RentedArrayStructSimple<tKey> seen = new(Length, clean: false);// Seen keys
            tKey[] seenArray = seen.Array;// Seen keys array
            bool useItem,// If to use the current item
                isKeyNull;// If the current key is NULL
            tItem item;// Current item
            tKey key,// Current key
                seenKey;// Seen key
            tList data = List;
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
        public virtual async IAsyncEnumerable<tItem> DistinctByAsync<tKey>(
            Func<tItem, CancellationToken, Task<tKey>> keySelector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            EnsureInitialListCount();
            using RentedArrayStructSimple<int> seenHashCodes = new(Length, clean: false);// Seen keys hash codes
            int[] seenHashCodesArray = seenHashCodes.Array;// Seen keys hash codes array
            using RentedArrayStructSimple<tKey> seen = new(Length, clean: false);// Seen keys
            tKey[] seenArray = seen.Array;// Seen keys array
            bool useItem,// If to use the current item
                isKeyNull;// If the current key is NULL
            tItem item;// Current item
            tKey key,// Current key
                seenKey;// Seen key
            tList data = List;
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
        public virtual tItem First()
        {
            EnsureInitialListCount();
            return Length > 0 ? List[Offset] : throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public virtual tItem First(Func<tItem, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (predicate(data[i]))
                    return data[i];
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public virtual async Task<tItem> FirstAsync(Func<tItem, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                if (await predicate(data[i], cancellationToken).DynamicContext())
                    return data[i];
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public override tItem FirstOrDefault(tItem defaultValue)
        {
            EnsureInitialListCount();
            return Length > 0 ? List[Offset] : defaultValue;
        }

        /// <inheritdoc/>
        public override tItem FirstOrDefault(Func<tItem, bool> predicate, tItem defaultValue)
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (predicate(data[i]))
                    return data[i];
            return defaultValue;
        }

        /// <inheritdoc/>
        public override async Task<tItem> FirstOrDefaultAsync(Func<tItem, CancellationToken, Task<bool>> predicate, tItem defaultValue, CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
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
        public virtual ListEnumerable<tList, tItem> Skip(int count)
        {
            EnsureInitialListCount();
            return count >= Length
                ? CreateEmptyInstance()
                : count < 1
                    ? this
                    : new(List, Offset + count, Length - count);
        }

        /// <summary>
        /// Skip while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ListEnumerable<tList, tItem> SkipWhile(Func<tItem, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (!predicate(data[i]))
                    return new(List, i, len - i);
            return CreateEmptyInstance();
        }

        /// <summary>
        /// Skip while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public virtual async IAsyncEnumerable<tItem> SkipWhileAsync(Func<tItem, CancellationToken, Task<bool>> predicate, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
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
        public virtual ListEnumerable<tList, tItem> Take(int count)
        {
            EnsureInitialListCount();
            return count < 1
                ? CreateEmptyInstance()
                : count < Length
                    ? new(List, Offset, count)
                    : this;
        }

        /// <summary>
        /// Take while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ListEnumerable<tList, tItem> TakeWhile(Func<tItem, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (!predicate(data[i]))
                    return i == Offset
                        ? CreateEmptyInstance()
                        : new(List, Offset, i - Offset);
            return this;
        }

        /// <summary>
        /// Take while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public virtual async IAsyncEnumerable<tItem> TakeWhileAsync(Func<tItem, CancellationToken, Task<bool>> predicate, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
            tItem item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.IsCancellationRequested; i++)
            {
                item = data[i];
                if (!await predicate(item, cancellationToken).DynamicContext()) yield break;
                yield return item;
            }
        }

        /// <inheritdoc/>
        public tItem[] ToArray()
        {
            EnsureInitialListCount();
            if (Length < 1) return [];
            tItem[] res = new tItem[Length];
            List.CopyTo(res, arrayIndex: 0);
            return res;
        }

        /// <inheritdoc/>
        public int ToBuffer(in Span<tItem> buffer)
        {
            EnsureInitialListCount();
            if (Length < 1) return 0;
            if (buffer.Length < Length) throw new OutOfMemoryException("Buffer to small");
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; buffer[i] = data[i], i++) ;
            return Length;
        }

        /// <inheritdoc/>
        public List<tItem> ToList()
        {
            EnsureInitialListCount();
            if (Length < 1) return [];
            List<tItem> res = new(Length);
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; res.Add(data[i]), i++) ;
            return res;
        }
    }
}

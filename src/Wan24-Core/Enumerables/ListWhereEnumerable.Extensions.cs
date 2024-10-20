using System.Runtime.CompilerServices;

namespace wan24.Core.Enumerables
{
    // Extensions
    public partial class ListWhereEnumerable<tList, tItem>
    {
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ListWhereEnumerable<tList, tItem> Where(Func<tItem, bool> predicate)
        {
            EnsureInitialListCount();
            return new(List, item => Predicate(item) && predicate(item), Offset, Length);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="selector">Selector</param>
        /// <returns>Enumerable</returns>
        public virtual ListWhereSelectEnumerable<tList, tItem, tResult> Select<tResult>(Func<tItem, tResult> selector)
        {
            EnsureInitialListCount();
            return new(List, Predicate, selector, Offset, Length);
        }

        /// <inheritdoc/>
        public virtual int Count()
        {
            EnsureInitialListCount();
            tList data = List;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]))
                    res++;
            return res;
        }

        /// <inheritdoc/>
        public virtual int Count(Func<tItem, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            int res = 0;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item) && predicate(item))
                    res++;
            }
            return res;
        }

        /// <inheritdoc/>
        public virtual async Task<int> CountAsync(Func<tItem, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
            int res = 0;
            tItem item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
            {
                item = data[i];
                if (Predicate(item) && await predicate(item, cancellationToken).DynamicContext())
                    res++;
            }
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
            {
                item = data[i];
                if (Predicate(item) && ((isItemNull && item is null) || (!isItemNull && item is not null && hc == item.GetHashCode() && item.Equals(obj))))
                    return true;
            }
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
            {
                item = data[i];
                if (!Predicate(item)) continue;
                for (j = 0, hc = item?.GetHashCode() ?? 0; j < objsLen; j++)
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
            {
                item = data[i];
                if (!Predicate(item)) continue;
                for (j = 0, hc = item?.GetHashCode() ?? 0; j < objsLen; j++)
                    if (
                        hashCodesSpan[j] == hc &&
                        (
                            ((isItemNull = item is null) && objsSpan[j] is null) ||
                            (!isItemNull && (obj = objsSpan[j]) is not null && obj.Equals(item))
                        )
                        )
                        return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual bool ContainsAtLeast(in int count)
        {
            EnsureInitialListCount();
            tList data = List;
            int cnt = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]) && ++cnt >= count)
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool ContainsAtMost(in int count)
        {
            EnsureInitialListCount();
            tList data = List;
            int cnt = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]) && ++cnt > count)
                    return false;
            return true;
        }

        /// <inheritdoc/>
        public virtual bool All(in Func<tItem, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item) && !predicate(item))
                    return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> AllAsync(Func<tItem, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item) && !await predicate(item, cancellationToken).DynamicContext())
                    return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public virtual bool Any()
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]))
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool Any(Func<tItem, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item) && predicate(item))
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> AnyAsync(Func<tItem, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
            tItem item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
            {
                item = data[i];
                if (Predicate(item) && await predicate(item, cancellationToken).DynamicContext())
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual void ExecuteForAll(in Action<tItem> action)
        {
            EnsureInitialListCount();
            tList data = List;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item)) action(item);
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<tReturn> ExecuteForAll<tReturn>(Func<tItem, EnumerableExtensions.ExecuteResult<tReturn>> action)
        {
            EnsureInitialListCount();
            tList data = List;
            EnumerableExtensions.ExecuteResult<tReturn> returnValue;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
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
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item)) await action(data[i], cancellationToken).DynamicContext();
            }
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
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                returnValue = await action(data[i], cancellationToken).DynamicContext();
                if (!returnValue.Next) break;
                if (returnValue) yield return returnValue.Result;
            }
        }

        /// <inheritdoc/>
        public virtual int DiscardAll(in bool dispose = true)
        {
            EnsureInitialListCount();
            tList data = List;
            tItem item;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item))
                {
                    res++;
                    item?.TryDispose();
                }
            }
            return res;
        }

        /// <inheritdoc/>
        public virtual async Task<int> DiscardAllAsync(bool dispose = true)
        {
            EnsureInitialListCount();
            tList data = List;
            tItem item;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item))
                {
                    res++;
                    if (item is not null) await item.TryDisposeAsync().DynamicContext();
                }
            }
            return res;
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
                item = data[i];
                if (!Predicate(item)) continue;
                for (hc = item?.GetHashCode() ?? 0, useItem = true, j = 0; j < seenCnt; j++)
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
                item = data[i];
                if (!Predicate(item)) continue;
                for (key = keySelector(item), hc = key?.GetHashCode() ?? 0, useItem = true, j = 0; j < seenCnt; j++)
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
                item = data[i];
                if (!Predicate(item)) continue;
                for (key = await keySelector(item, cancellationToken).DynamicContext(), hc = key?.GetHashCode() ?? 0, useItem = true, j = 0; j < seenCnt; j++)
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
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]))
                    return data[i];
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public virtual tItem First(Func<tItem, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item) && predicate(item))
                    return item;
            }
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public virtual async Task<tItem> FirstAsync(Func<tItem, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
            tItem item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
            {
                item = data[i];
                if (Predicate(item) && await predicate(item, cancellationToken).DynamicContext())
                    return item;
            }
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public override tItem FirstOrDefault(tItem defaultValue)
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]))
                    return data[i];
            return defaultValue;
        }

        /// <inheritdoc/>
        public override tItem FirstOrDefault(Func<tItem, bool> predicate, tItem defaultValue)
        {
            EnsureInitialListCount();
            tList data = List;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item) && predicate(item))
                    return item;
            }
            return defaultValue;
        }

        /// <inheritdoc/>
        public override async Task<tItem> FirstOrDefaultAsync(Func<tItem, CancellationToken, Task<bool>> predicate, tItem defaultValue, CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
            tItem item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
            {
                item = data[i];
                if (Predicate(item) && await predicate(item, cancellationToken).DynamicContext())
                    return item;
            }
            return defaultValue;
        }

        /// <summary>
        /// Skip
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Enumerable</returns>
        public virtual ListWhereEnumerable<tList, tItem> Skip(int count)
        {
            EnsureInitialListCount();
            if (count < 1) return this;
            if (count >= Length) return CreateEmptyInstance();
            tList data = List;
            for (int i = Offset, len = i + Length, cnt = 0; i < len; i++)
                if (Predicate(data[i]) && ++cnt >= count)
                    return ++i >= len
                        ? CreateEmptyInstance()
                        : new(List, Predicate, i, len - i);
            return CreateEmptyInstance();
        }

        /// <summary>
        /// Skip while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ListWhereEnumerable<tList, tItem> SkipWhile(Func<tItem, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            tItem item;
            for (int i = Offset, len = i + Length, cnt = 0; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                cnt++;
                if (predicate(item)) continue;
                return cnt == 1
                    ? CreateEmptyInstance()
                    : new(List, Predicate, i, len - i);
            }
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
            tItem item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.IsCancellationRequested; i++)
            {
                if (skip)
                {
                    item = data[i];
                    if (!Predicate(item) || await predicate(item, cancellationToken).DynamicContext()) continue;
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
        public virtual ListWhereEnumerable<tList, tItem> Take(int count)
        {
            EnsureInitialListCount();
            if (count < 1) return CreateEmptyInstance();
            if (count >= Length) return this;
            tList data = List;
            for (int i = Offset, len = i + Length, cnt = 0; i < len; i++)
                if (Predicate(data[i]) && ++cnt >= count)
                    return new(List, Predicate, Offset, i - Offset + 1);
            return this;
        }

        /// <summary>
        /// Take while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ListWhereEnumerable<tList, tItem> TakeWhile(Func<tItem, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            tItem item;
            for (int i = Offset, len = i + Length, cnt = 0; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                cnt++;
                if (predicate(item)) continue;
                return cnt == 1
                    ? CreateEmptyInstance()
                    : i == len - 1
                        ? this
                        : new(List, Predicate, Offset, i - Offset);
            }
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
                if (!Predicate(item)) continue;
                if (!await predicate(item, cancellationToken).DynamicContext()) yield break;
                yield return item;
            }
        }

        /// <inheritdoc/>
        public tItem[] ToArray()
        {
            EnsureInitialListCount();
            if (Length < 1) return [];
            using RentedMemoryRef<tItem> buffer = new(Length, clean: false);
            int len = ToBuffer(buffer.Span);
            return len < 1
                ? []
                : buffer.Span[..len].ToArray();
        }

        /// <inheritdoc/>
        public int ToBuffer(in Span<tItem> buffer)
        {
            EnsureInitialListCount();
            if (Length < 1) return 0;
            tList data = List;
            tItem item;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                if (buffer.Length <= res) throw new OutOfMemoryException("Buffer to small");
                buffer[res] = item;
                res++;
            }
            return res;
        }

        /// <inheritdoc/>
        public List<tItem> ToList()
        {
            EnsureInitialListCount();
            if (Length < 1) return [];
            List<tItem> res = new(Length);
            tList data = List;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item))
                    res.Add(item);
            }
            return res;
        }
    }
}

using System.Runtime.CompilerServices;

namespace wan24.Core.Enumerables
{
    // Extensions
    public partial class ListSelectEnumerable<tList, tItem, tResult>
    {
        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tResult2">Result type</typeparam>
        /// <param name="selector">Selector</param>
        /// <returns>Enumerable</returns>
        public virtual ListSelectEnumerable<tList, tItem, tResult2> Select<tResult2>(Func<tResult, tResult2> selector)
        {
            EnsureInitialListCount();
            return new(List, item => selector(Selector(item)), Offset, Length);
        }

        /// <inheritdoc/>
        public virtual int Count()
        {
            EnsureInitialListCount();
            return Length;
        }

        /// <inheritdoc/>
        public virtual int Count(Func<tResult, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (predicate(Selector(data[i])))
                    res++;
            return res;
        }

        /// <inheritdoc/>
        public virtual async Task<int> CountAsync(Func<tResult, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                if (await predicate(Selector(data[i]), cancellationToken).DynamicContext())
                    res++;
            return res;
        }

        /// <inheritdoc/>
        public virtual bool Contains(tResult obj)
        {
            EnsureInitialListCount();
            if (Length == 0) return false;
            tList data = List;
            bool isItemNull = obj is null;
            tResult item;
            for (int i = Offset, len = i + Length, hc = obj?.GetHashCode() ?? 0; i < len; i++)
            {
                item = Selector(data[i]);
                if ((isItemNull && item is null) || (!isItemNull && item is not null && hc == item.GetHashCode() && item.Equals(obj)))
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual bool ContainsAll(params tResult[] objs)
        {
            EnsureInitialListCount();
            if (Length < 1) return false;
            int objsLen = objs.Length;// Number of objects to look for
            if (objsLen == 0 || objsLen > Length) return false;
            Span<tResult> objsSpan = objs.AsSpan();// Span of objects to look for
            int i = 0;// Index counter
            using RentedArrayRefStruct<int> hashCodes = new(objsLen, clean: false);// Hash codes of objects to look for
            Span<int> hashCodesSpan = hashCodes.Span;// Hash codes span
            for (; i < objsLen; hashCodes[i] = objsSpan[i]?.GetHashCode() ?? 0, i++) ;
            using RentedArrayRefStruct<bool> seen = new(objsLen);// Found object indicators
            Span<bool> seenSpan = seen.Span;// Found object indicators span
            bool isItemNull;// If the current item is NULL
            tResult item,// Current item
                obj;// Current object
            tList data = List;
            i = Offset;
            for (int j, len = i + Length, seenCnt = 0, hc; i < len; i++)
                for (item = Selector(data[i]), j = 0, hc = item?.GetHashCode() ?? 0; j < objsLen; j++)
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
        public virtual bool ContainsAny(params tResult[] objs)
        {
            EnsureInitialListCount();
            if (Length < 1) return false;
            int objsLen = objs.Length;// Number of objects to look for
            if (objsLen == 0 || objsLen > Length) return false;
            Span<tResult> objsSpan = objs.AsSpan();// Span of objects to look for
            int i = 0;// Index counter
            using RentedArrayRefStruct<int> hashCodes = new(objsLen, clean: false);// Hash codes of objects to look for
            Span<int> hashCodesSpan = hashCodes.Span;// Hash codes span
            for (; i < objsLen; hashCodes[i] = objsSpan[i]?.GetHashCode() ?? 0, i++) ;
            bool isItemNull;// If the current item is NULL
            tResult item,// Current item
                obj;// Current object
            tList data = List;
            i = Offset;
            for (int j, len = i + Length, hc; i < len; i++)
                for (item = Selector(data[i]), j = 0, hc = item?.GetHashCode() ?? 0; j < objsLen; j++)
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
        public virtual bool All(in Func<tResult, bool> predicate)
        {
            EnsureInitialListCount();
            return Count(predicate) == Length;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> AllAsync(Func<tResult, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
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
        public virtual bool Any(Func<tResult, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (predicate(Selector(data[i])))
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> AnyAsync(Func<tResult, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                if (await predicate(Selector(data[i]), cancellationToken).DynamicContext())
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual void ExecuteForAll(in Action<tResult> action)
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; action(Selector(data[i])), i++) ;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<tReturn> ExecuteForAll<tReturn>(Func<tResult, EnumerableExtensions.ExecuteResult<tReturn>> action)
        {
            EnsureInitialListCount();
            tList data = List;
            EnumerableExtensions.ExecuteResult<tReturn> returnValue;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                returnValue = action(Selector(data[i]));
                if (!returnValue.Next) break;
                if (returnValue) yield return returnValue.Result;
            }
        }

        /// <inheritdoc/>
        public virtual async Task ExecuteForAllAsync(Func<tResult, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
            for (
                int i = Offset, len = i + Length;
                i < len && !cancellationToken.GetIsCancellationRequested();
                await action(Selector(data[i]), cancellationToken).DynamicContext(), i++
                ) ;
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<tReturn> ExecuteForAllAsync<tReturn>(
            Func<tResult, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tReturn>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            EnsureInitialListCount();
            tList data = List;
            EnumerableExtensions.ExecuteResult<tReturn> returnValue;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
            {
                returnValue = await action(Selector(data[i]), cancellationToken).DynamicContext();
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
            for (int i = Offset, len = i + Length; i < len; Selector(data[i])?.TryDispose(), i++) ;
            return Length;
        }

        /// <inheritdoc/>
        public virtual async Task<int> DiscardAllAsync(bool dispose = true)
        {
            EnsureInitialListCount();
            if (!dispose) return Length;
            tList data = List;
            tResult item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = Selector(data[i]);
                if (item is not null) await item.TryDisposeAsync().DynamicContext();
            }
            return Length;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<tResult> Distinct()
        {
            EnsureInitialListCount();
            using RentedArrayStructSimple<int> seenHashCodes = new(Length, clean: false);// Seen items hash codes
            int[] seenHashCodesArray = seenHashCodes.Array;// Seen items hash codes array
            using RentedArrayStructSimple<tResult> seen = new(Length, clean: false);// Seen items
            tResult[] seenArray = seen.Array;// Seen items array
            bool useItem,// If to use the current item
                isItemNull;// If the current item is NULL
            tResult item,// Current item
                seenItem;// Seen item
            tList data = List;
            for (int i = Offset, len = i + Length, j, seenCnt = 0, hc; i < len; i++)
            {
                for (item = Selector(data[i]), hc = item?.GetHashCode() ?? 0, useItem = true, j = 0; j < seenCnt; j++)
                    if (
                        seenHashCodes[j] == hc &&
                        (
                            ((isItemNull = item is null) && seen[j] is null) ||
                            (!isItemNull && (seenItem = seen[j]) is not null && seenItem.Equals(item))
                        )
                        )
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
        public virtual IEnumerable<tResult> DistinctBy<tKey>(Func<tResult, tKey> keySelector)
        {
            EnsureInitialListCount();
            using RentedArrayStructSimple<int> seenHashCodes = new(Length, clean: false);// Seen keys hash codes
            int[] seenHashCodesArray = seenHashCodes.Array;// Seen keys hash codes array
            using RentedArrayStructSimple<tKey> seen = new(Length, clean: false);// Seen keys
            tKey[] seenArray = seen.Array;// Seen keys array
            bool useItem,// If to use the current item
                isKeyNull;// If the current key is NULL
            tResult item;// Current item
            tKey key,// Current key
                seenKey;// Seen key
            tList data = List;
            for (int i = Offset, len = i + Length, j, seenCnt = 0, hc; i < len; i++)
            {
                for (item = Selector(data[i]), key = keySelector(item), hc = key?.GetHashCode() ?? 0, useItem = true, j = 0; j < seenCnt; j++)
                    if (
                        seenHashCodes[j] == hc &&
                        (
                            ((isKeyNull = key is null) && seen[j] is null) ||
                            (!isKeyNull && (seenKey = seen[j]) is not null && seenKey.Equals(key))
                        )
                        )
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
        public virtual async IAsyncEnumerable<tResult> DistinctByAsync<tKey>(
            Func<tResult, CancellationToken, Task<tKey>> keySelector,
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
            tResult item;// Current item
            tKey key,// Current key
                seenKey;// Seen key
            tList data = List;
            for (int i = Offset, len = i + Length, j, seenCnt = 0, hc; i < len; i++)
            {
                for (
                    item = Selector(data[i]), key = await keySelector(item, cancellationToken).DynamicContext(), hc = key?.GetHashCode() ?? 0, useItem = true, j = 0;
                    j < seenCnt;
                    j++
                    )
                    if (
                        seenHashCodes[j] == hc &&
                        (
                            ((isKeyNull = key is null) && seen[j] is null) ||
                            (!isKeyNull && (seenKey = seen[j]) is not null && seenKey.Equals(key))
                        )
                        )
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
        public virtual tResult First()
        {
            EnsureInitialListCount();
            return Length > 0 ? Selector(List[Offset]) : throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public virtual tResult First(Func<tResult, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            tResult item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = Selector(data[i]);
                if (predicate(item))
                    return item;
            }
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public virtual async Task<tResult> FirstAsync(Func<tResult, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            EnsureInitialListCount();
            tList data = List;
            tResult item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
            {
                item = Selector(data[i]);
                if (await predicate(item, cancellationToken).DynamicContext())
                    return item;
            }
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public virtual tResult? FirstOrDefault(tResult? defaultValue = default)
        {
            EnsureInitialListCount();
            return Length > 0 ? Selector(List[Offset]) : defaultValue;
        }

        /// <inheritdoc/>
        public virtual tResult? FirstOrDefault(Func<tResult, bool> predicate, tResult? defaultValue = default)
        {
            EnsureInitialListCount();
            tList data = List;
            tResult item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = Selector(data[i]);
                if (predicate(item))
                    return item;
            }
            return defaultValue;
        }

        /// <inheritdoc/>
        public virtual async Task<tResult?> FirstOrDefaultAsync(
            Func<tResult, CancellationToken, Task<bool>> predicate,
            tResult? defaultValue = default,
            CancellationToken cancellationToken = default
            )
        {
            EnsureInitialListCount();
            tList data = List;
            tResult item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
            {
                item = Selector(data[i]);
                if (await predicate(item, cancellationToken).DynamicContext())
                    return item;
            }
            return defaultValue;
        }

        /// <summary>
        /// Skip
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Enumerable</returns>
        public virtual ListSelectEnumerable<tList, tItem, tResult> Skip(int count)
        {
            EnsureInitialListCount();
            return count >= Length
                ? CreateEmptyInstance()
                : count < 1
                    ? this
                    : new(List, Selector, Offset + count, Length - count);
        }

        /// <summary>
        /// Skip while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ListSelectEnumerable<tList, tItem, tResult> SkipWhile(Func<tResult, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (!predicate(Selector(data[i])))
                    return new(List, Selector, i, len - i);
            return CreateEmptyInstance();
        }

        /// <summary>
        /// Skip while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public virtual async IAsyncEnumerable<tResult> SkipWhileAsync(
            Func<tResult, CancellationToken, Task<bool>> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            EnsureInitialListCount();
            tList data = List;
            bool skip = true;
            tResult item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.IsCancellationRequested; i++)
            {
                item = Selector(data[i]);
                if (skip)
                {
                    if (await predicate(item, cancellationToken).DynamicContext()) continue;
                    skip = false;
                }
                yield return item;
            }
        }

        /// <summary>
        /// Take
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Enumerable</returns>
        public virtual ListSelectEnumerable<tList, tItem, tResult> Take(int count)
        {
            EnsureInitialListCount();
            return count < 1
                ? CreateEmptyInstance()
                : count < Length
                    ? new(List, Selector, Offset, count)
                    : this;
        }

        /// <summary>
        /// Take while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ListSelectEnumerable<tList, tItem, tResult> TakeWhile(Func<tResult, bool> predicate)
        {
            EnsureInitialListCount();
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (!predicate(Selector(data[i])))
                    return i == Offset
                        ? CreateEmptyInstance()
                        : new(List, Selector, Offset, i - Offset);
            return this;
        }

        /// <summary>
        /// Take while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public virtual async IAsyncEnumerable<tResult> TakeWhileAsync(
            Func<tResult, CancellationToken, Task<bool>> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            EnsureInitialListCount();
            tList data = List;
            tResult item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.IsCancellationRequested; i++)
            {
                item = Selector(data[i]);
                if (!await predicate(item, cancellationToken).DynamicContext()) yield break;
                yield return item;
            }
        }

        /// <inheritdoc/>
        public override tResult[] ToArray()
        {
            EnsureInitialListCount();
            if (Length < 1) return [];
            tList data = List;
            tResult[] res = new tResult[Length];
            for (int i = Offset, len = i + Length; i < len; res[i] = Selector(data[i]), i++) ;
            return res;
        }

        /// <inheritdoc/>
        public virtual int ToBuffer(in Span<tResult> buffer)
        {
            EnsureInitialListCount();
            if (Length < 1) return 0;
            if (buffer.Length < Length) throw new OutOfMemoryException("Buffer to small");
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; buffer[i] = Selector(data[i]), i++) ;
            return Length;
        }

        /// <inheritdoc/>
        public virtual List<tResult> ToList()
        {
            EnsureInitialListCount();
            if (Length < 1) return [];
            List<tResult> res = new(Length);
            tList data = List;
            for (int i = Offset, len = i + Length; i < len; res.Add(Selector(data[i])), i++) ;
            return res;
        }
    }
}

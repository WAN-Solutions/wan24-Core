using System.Runtime.CompilerServices;

namespace wan24.Core.Enumerables
{
    // Extensions
    public partial class ArrayWhereSelectEnumerable<tItem, tResult>
    {
        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tResult2">Result type</typeparam>
        /// <param name="selector">Selector</param>
        /// <returns>Enumerable</returns>
        public virtual ArrayWhereSelectEnumerable<tItem, tResult2> Select<tResult2>(Func<tResult, tResult2> selector)
            => new(Array, Predicate, item => selector(Selector(item)), Offset, Length);

        /// <inheritdoc/>
        public virtual int Count()
        {
            Span<tItem> data = Array;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]))
                    res++;
            return res;
        }

        /// <inheritdoc/>
        public virtual int Count(Func<tResult, bool> predicate)
        {
            Span<tItem> data = Array;
            int res = 0;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item) && predicate(Selector(item)))
                    res++;
            }
            return res;
        }

        /// <inheritdoc/>
        public virtual async Task<int> CountAsync(Func<tResult, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            tItem[] data = Array;
            int res = 0;
            tItem item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
            {
                item = data[i];
                if (Predicate(item) && await predicate(Selector(item), cancellationToken).DynamicContext())
                    res++;
            }
            return res;
        }

        /// <inheritdoc/>
        public virtual bool Contains(tResult obj)
        {
            if (Length == 0) return false;
            Span<tItem> data = Array;
            bool isItemNull = obj is null;
            tItem item;
            tResult result;
            for (int i = Offset, len = i + Length, hc = obj?.GetHashCode() ?? 0; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                result = Selector(item);
                if ((isItemNull && result is null) || (!isItemNull && result is not null && hc == result.GetHashCode() && result.Equals(obj)))
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual bool ContainsAll(params tResult[] objs)
        {
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
            tItem item;// Current item
            tResult result,// Current result
                obj;// Current object
            Span<tItem> data = Array;
            i = Offset;
            for (int j, len = i + Length, seenCnt = 0, hc; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                result = Selector(item);
                for (j = 0, hc = result?.GetHashCode() ?? 0; j < objsLen; j++)
                    if (
                        !seenSpan[j] && hashCodesSpan[j] == hc &&
                        (
                            ((isItemNull = result is null) && objsSpan[j] is null) ||
                            (!isItemNull && (obj = objsSpan[j]) is not null && obj.Equals(result))
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
        public virtual bool ContainsAny(params tResult[] objs)
        {
            if (Length < 1) return false;
            int objsLen = objs.Length;// Number of objects to look for
            if (objsLen == 0 || objsLen > Length) return false;
            Span<tResult> objsSpan = objs.AsSpan();// Span of objects to look for
            int i = 0;// Index counter
            using RentedArrayRefStruct<int> hashCodes = new(objsLen, clean: false);// Hash codes of objects to look for
            Span<int> hashCodesSpan = hashCodes.Span;// Hash codes span
            for (; i < objsLen; hashCodes[i] = objsSpan[i]?.GetHashCode() ?? 0, i++) ;
            bool isItemNull;// If the current item is NULL
            tItem item;// Current item
            tResult result,// Current result
                obj;// Current object
            Span<tItem> data = Array;
            i = Offset;
            for (int j, len = i + Length, hc; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                result = Selector(item);
                for (j = 0, hc = result?.GetHashCode() ?? 0; j < objsLen; j++)
                    if (
                        hashCodesSpan[j] == hc &&
                        (
                            ((isItemNull = result is null) && objsSpan[j] is null) ||
                            (!isItemNull && (obj = objsSpan[j]) is not null && obj.Equals(result))
                        )
                        )
                        return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual bool ContainsAtLeast(in int count)
        {
            Span<tItem> data = Array;
            int cnt = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]) && ++cnt >= count)
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool ContainsAtMost(in int count)
        {
            Span<tItem> data = Array;
            int cnt = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]) && ++cnt > count)
                    return false;
            return true;
        }

        /// <inheritdoc/>
        public virtual bool All(in Func<tResult, bool> predicate)
        {
            Span<tItem> data = Array;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item) && !predicate(Selector(item)))
                    return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> AllAsync(Func<tResult, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            tItem[] data = Array;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item) && !await predicate(Selector(item), cancellationToken).DynamicContext())
                    return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public virtual bool Any()
        {
            Span<tItem> data = Array;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]))
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool Any(Func<tResult, bool> predicate)
        {
            Span<tItem> data = Array;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item) && predicate(Selector(item)))
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> AnyAsync(Func<tResult, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            tItem[] data = Array;
            tItem item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
            {
                item = data[i];
                if (Predicate(item) && await predicate(Selector(item), cancellationToken).DynamicContext())
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual void ExecuteForAll(in Action<tResult> action)
        {
            Span<tItem> data = Array;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item)) action(Selector(item));
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<tReturn> ExecuteForAll<tReturn>(Func<tResult, EnumerableExtensions.ExecuteResult<tReturn>> action)
        {
            tItem[] data = Array;
            EnumerableExtensions.ExecuteResult<tReturn> returnValue;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                returnValue = action(Selector(data[i]));
                if (!returnValue.Next) break;
                if (returnValue) yield return returnValue.Result;
            }
        }

        /// <inheritdoc/>
        public virtual async Task ExecuteForAllAsync(Func<tResult, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            tItem[] data = Array;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item)) await action(Selector(data[i]), cancellationToken).DynamicContext();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<tReturn> ExecuteForAllAsync<tReturn>(
            Func<tResult, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tReturn>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            tItem[] data = Array;
            EnumerableExtensions.ExecuteResult<tReturn> returnValue;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                returnValue = await action(Selector(data[i]), cancellationToken).DynamicContext();
                if (!returnValue.Next) break;
                if (returnValue) yield return returnValue.Result;
            }
        }

        /// <inheritdoc/>
        public virtual int DiscardAll(in bool dispose = true)
        {
            Span<tItem> data = Array;
            tItem item;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item))
                {
                    res++;
                    Selector(item)?.TryDispose();
                }
            }
            return res;
        }

        /// <inheritdoc/>
        public virtual async Task<int> DiscardAllAsync(bool dispose = true)
        {
            tItem[] data = Array;
            tItem item;
            tResult result;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item))
                {
                    res++;
                    result = Selector(item);
                    if (result is not null) await result.TryDisposeAsync().DynamicContext();
                }
            }
            return res;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<tResult> Distinct()
        {
            using RentedArrayStructSimple<int> seenHashCodes = new(Length, clean: false);// Seen items hash codes
            int[] seenHashCodesArray = seenHashCodes.Array;// Seen items hash codes array
            using RentedArrayStructSimple<tResult> seen = new(Length, clean: false);// Seen items
            tResult[] seenArray = seen.Array;// Seen items array
            bool useItem,// If to use the current item
                isItemNull;// If the current item is NULL
            tItem item;// Current item
            tResult result,// Current result
                seenItem;// Seen item
            tItem[] data = Array;
            for (int i = Offset, len = i + Length, j, seenCnt = 0, hc; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                result = Selector(item);
                for (hc = result?.GetHashCode() ?? 0, useItem = true, j = 0; j < seenCnt; j++)
                    if (
                        seenHashCodes[j] == hc &&
                        (
                            ((isItemNull = result is null) && seen[j] is null) ||
                            (!isItemNull && (seenItem = seen[j]) is not null && seenItem.Equals(result))
                        )
                        )
                    {
                        useItem = false;
                        break;
                    }
                if (!useItem) continue;
                seenHashCodesArray[seenCnt] = hc;
                seenArray[seenCnt] = result;
                seenCnt++;
                yield return result;
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<tResult> DistinctBy<tKey>(Func<tResult, tKey> keySelector)
        {
            using RentedArrayStructSimple<int> seenHashCodes = new(Length, clean: false);// Seen keys hash codes
            int[] seenHashCodesArray = seenHashCodes.Array;// Seen keys hash codes array
            using RentedArrayStructSimple<tKey> seen = new(Length, clean: false);// Seen keys
            tKey[] seenArray = seen.Array;// Seen keys array
            bool useItem,// If to use the current item
                isKeyNull;// If the current key is NULL
            tItem item;// Current item
            tResult result;// Current result
            tKey key,// Current key
                seenKey;// Seen key
            tItem[] data = Array;
            for (int i = Offset, len = i + Length, j, seenCnt = 0, hc; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                result = Selector(item);
                for (key = keySelector(result), hc = key?.GetHashCode() ?? 0, useItem = true, j = 0; j < seenCnt; j++)
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
                yield return result;
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<tResult> DistinctByAsync<tKey>(
            Func<tResult, CancellationToken, Task<tKey>> keySelector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            using RentedArrayStructSimple<int> seenHashCodes = new(Length, clean: false);// Seen keys hash codes
            int[] seenHashCodesArray = seenHashCodes.Array;// Seen keys hash codes array
            using RentedArrayStructSimple<tKey> seen = new(Length, clean: false);// Seen keys
            tKey[] seenArray = seen.Array;// Seen keys array
            bool useItem,// If to use the current item
                isKeyNull;// If the current key is NULL
            tItem item;// Current item
            tResult result;// Current result
            tKey key,// Current key
                seenKey;// Seen key
            tItem[] data = Array;
            for (int i = Offset, len = i + Length, j, seenCnt = 0, hc; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                result = Selector(item);
                for (key = await keySelector(result, cancellationToken).DynamicContext(), hc = key?.GetHashCode() ?? 0, useItem = true, j = 0; j < seenCnt; j++)
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
                yield return result;
            }
        }

        /// <inheritdoc/>
        public virtual tResult First()
        {
            Span<tItem> data = Array;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]))
                    return Selector(data[i]);
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public virtual tResult First(Func<tResult, bool> predicate)
        {
            Span<tItem> data = Array;
            tItem item;
            tResult result;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                result = Selector(item);
                if (predicate(result))
                    return result;
            }
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public virtual async Task<tResult> FirstAsync(Func<tResult, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            tItem[] data = Array;
            tItem item;
            tResult result;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                result = Selector(item);
                if (await predicate(result, cancellationToken).DynamicContext())
                    return result;
            }
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public virtual tResult? FirstOrDefault(tResult? defaultValue = default)
        {
            Span<tItem> data = Array;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]))
                    return Selector(data[i]);
            return defaultValue;
        }

        /// <inheritdoc/>
        public virtual tResult? FirstOrDefault(Func<tResult, bool> predicate, tResult? defaultValue = default)
        {
            Span<tItem> data = Array;
            tItem item;
            tResult result;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                result = Selector(item);
                if (predicate(result))
                    return result;
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
            tItem[] data = Array;
            tItem item;
            tResult result;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                result = Selector(item);
                if (await predicate(result, cancellationToken).DynamicContext())
                    return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Skip
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Enumerable</returns>
        public virtual ArrayWhereSelectEnumerable<tItem, tResult> Skip(int count)
        {
            if (count < 1) return this;
            if (count >= Length) return Empty;
            Span<tItem> data = Array;
            for (int i = Offset, len = i + Length, cnt = 0; i < len; i++)
                if (Predicate(data[i]) && ++cnt >= count)
                    return ++i >= len
                        ? Empty
                        : new(Array, Predicate, Selector, i, len - i);
            return Empty;
        }

        /// <summary>
        /// Skip while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ArrayWhereSelectEnumerable<tItem, tResult> SkipWhile(Func<tResult, bool> predicate)
        {
            Span<tItem> data = Array;
            tItem item;
            for (int i = Offset, len = i + Length, cnt = 0; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                cnt++;
                if (predicate(Selector(item))) continue;
                return cnt == 1
                    ? Empty
                    : new(Array, Predicate, Selector, i, len - i);
            }
            return Empty;
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
            tItem[] data = Array;
            bool skip = true;
            tItem item;
            tResult result;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.IsCancellationRequested; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                result = Selector(item);
                if (skip)
                {
                    if (await predicate(result, cancellationToken).DynamicContext()) continue;
                    skip = false;
                }
                yield return result;
            }
        }

        /// <summary>
        /// Take
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Enumerable</returns>
        public virtual ArrayWhereSelectEnumerable<tItem, tResult> Take(int count)
        {
            if (count < 1) return Empty;
            if (count >= Length) return this;
            Span<tItem> data = Array;
            for (int i = Offset, len = i + Length, cnt = 0; i < len; i++)
                if (Predicate(data[i]) && ++cnt >= count)
                    return new(Array, Predicate, Selector, Offset, i - Offset + 1);
            return this;
        }

        /// <summary>
        /// Take while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ArrayWhereSelectEnumerable<tItem, tResult> TakeWhile(Func<tResult, bool> predicate)
        {
            Span<tItem> data = Array;
            tItem item;
            for (int i = Offset, len = i + Length, cnt = 0; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                cnt++;
                if (predicate(Selector(item))) continue;
                return cnt == 1
                    ? Empty
                    : i == len - 1
                        ? this
                        : new(Array, Predicate, Selector, Offset, i - Offset);
            }
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
            tItem[] data = Array;
            tItem item;
            tResult result;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.IsCancellationRequested; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                result = Selector(item);
                if (!await predicate(result, cancellationToken).DynamicContext()) yield break;
                yield return result;
            }
        }

        /// <inheritdoc/>
        public override tResult[] ToArray()
        {
            if (Length < 1) return [];
            using RentedMemoryRef<tResult> buffer = new(Length, clean: false);
            int len = ToBuffer(buffer.Span);
            return len < 1
                ? []
                : buffer.Span[..len].ToArray();
        }

        /// <inheritdoc/>
        public virtual int ToBuffer(in Span<tResult> buffer)
        {
            if (Length < 1) return 0;
            Span<tItem> data = Array;
            tItem item;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                if (buffer.Length <= res) throw new OutOfMemoryException("Buffer to small");
                buffer[res] = Selector(item);
                res++;
            }
            return res;
        }

        /// <inheritdoc/>
        public virtual List<tResult> ToList()
        {
            if (Length < 1) return [];
            List<tResult> res = new(Length);
            Span<tItem> data = Array;
            tItem item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item))
                    res.Add(Selector(item));
            }
            return res;
        }
    }
}

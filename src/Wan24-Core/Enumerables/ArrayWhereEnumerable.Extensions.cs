using System.Runtime.CompilerServices;

namespace wan24.Core.Enumerables
{
    // Extensions
    public partial class ArrayWhereEnumerable<T>
    {
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ArrayWhereEnumerable<T> Where(Func<T, bool> predicate) => new(Array, item => Predicate(item) && predicate(item), Offset, Length);

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="selector">Selector</param>
        /// <returns>Enumerable</returns>
        public virtual ArrayWhereSelectEnumerable<T, tResult> Select<tResult>(Func<T, tResult> selector) => new(Array, Predicate, selector, Offset, Length);

        /// <inheritdoc/>
        public virtual int Count()
        {
            Span<T> data = Array;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]))
                    res++;
            return res;
        }

        /// <inheritdoc/>
        public virtual int Count(Func<T, bool> predicate)
        {
            Span<T> data = Array;
            int res = 0;
            T item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item) && predicate(item))
                    res++;
            }
            return res;
        }

        /// <inheritdoc/>
        public virtual async Task<int> CountAsync(Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            T[] data = Array;
            int res = 0;
            T item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
            {
                item = data[i];
                if (Predicate(item) && await predicate(item, cancellationToken).DynamicContext())
                    res++;
            }
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
            {
                item = data[i];
                if (Predicate(item) && ((isItemNull && item is null) || (!isItemNull && item is not null && hc == item.GetHashCode() && item.Equals(obj))))
                    return true;
            }
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
            Span<T> data = Array;
            int cnt = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]) && ++cnt >= count)
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool ContainsAtMost(in int count)
        {
            Span<T> data = Array;
            int cnt = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]) && ++cnt > count)
                    return false;
            return true;
        }

        /// <inheritdoc/>
        public virtual bool All(in Func<T, bool> predicate)
        {
            Span<T> data = Array;
            T item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item) && !predicate(item))
                    return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> AllAsync(Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            T[] data = Array;
            T item;
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
            Span<T> data = Array;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]))
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool Any(Func<T, bool> predicate)
        {
            Span<T> data = Array;
            T item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item) && predicate(item))
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> AnyAsync(Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            T[] data = Array;
            T item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
            {
                item = data[i];
                if (Predicate(item) && await predicate(item, cancellationToken).DynamicContext())
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual void ExecuteForAll(in Action<T> action)
        {
            Span<T> data = Array;
            T item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item)) action(item);
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<tReturn> ExecuteForAll<tReturn>(Func<T, EnumerableExtensions.ExecuteResult<tReturn>> action)
        {
            T[] data = Array;
            EnumerableExtensions.ExecuteResult<tReturn> returnValue;
            T item;
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
        public virtual async Task ExecuteForAllAsync(Func<T, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            T[] data = Array;
            T item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item)) await action(data[i], cancellationToken).DynamicContext();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<tReturn> ExecuteForAllAsync<tReturn>(
            Func<T, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tReturn>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            T[] data = Array;
            EnumerableExtensions.ExecuteResult<tReturn> returnValue;
            T item;
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
            Span<T> data = Array;
            T item;
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
            T[] data = Array;
            T item;
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
        public virtual T First()
        {
            Span<T> data = Array;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]))
                    return data[i];
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public virtual T First(Func<T, bool> predicate)
        {
            Span<T> data = Array;
            T item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item) && predicate(item))
                    return item;
            }
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public virtual async Task<T> FirstAsync(Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            T[] data = Array;
            T item;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
            {
                item = data[i];
                if (Predicate(item) && await predicate(item, cancellationToken).DynamicContext())
                    return item;
            }
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <inheritdoc/>
        public override T? FirstOrDefault(T? defaultValue)
        {
            Span<T> data = Array;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (Predicate(data[i]))
                    return data[i];
            return defaultValue;
        }

        /// <inheritdoc/>
        public override T? FirstOrDefault(Func<T, bool> predicate, T? defaultValue)
        {
            Span<T> data = Array;
            T item;
            for (int i = Offset, len = i + Length; i < len; i++)
            {
                item = data[i];
                if (Predicate(item) && predicate(item))
                    return item;
            }
            return defaultValue;
        }

        /// <inheritdoc/>
        public override async Task<T?> FirstOrDefaultAsync(Func<T, CancellationToken, Task<bool>> predicate, T? defaultValue, CancellationToken cancellationToken = default)
        {
            T[] data = Array;
            T item;
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
        public virtual ArrayWhereEnumerable<T> Skip(int count)
        {
            if (count < 1) return this;
            if (count >= Length) return Empty;
            Span<T> data = Array;
            for (int i = Offset, len = i + Length, cnt = 0; i < len; i++)
                if (Predicate(data[i]) && ++cnt >= count)
                    return ++i >= len
                        ? Empty
                        : new(Array, Predicate, i, len - i);
            return Empty;
        }

        /// <summary>
        /// Skip while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ArrayWhereEnumerable<T> SkipWhile(Func<T, bool> predicate)
        {
            Span<T> data = Array;
            T item;
            for (int i = Offset, len = i + Length, cnt = 0; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                cnt++;
                if (predicate(item)) continue;
                return cnt == 1
                    ? Empty
                    : new(Array, Predicate, i, len - i);
            }
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
            T item;
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
        public virtual ArrayWhereEnumerable<T> Take(int count)
        {
            if (count < 1) return Empty;
            if (count >= Length) return this;
            Span<T> data = Array;
            for (int i = Offset, len = i + Length, cnt = 0; i < len; i++)
                if (Predicate(data[i]) && ++cnt >= count)
                    return new(Array, Predicate, Offset, i - Offset + 1);
            return this;
        }

        /// <summary>
        /// Take while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ArrayWhereEnumerable<T> TakeWhile(Func<T, bool> predicate)
        {
            Span<T> data = Array;
            T item;
            for (int i = Offset, len = i + Length, cnt = 0; i < len; i++)
            {
                item = data[i];
                if (!Predicate(item)) continue;
                cnt++;
                if (predicate(item)) continue;
                return cnt == 1
                    ? Empty
                    : i == len - 1
                        ? this
                        : new(Array, Predicate, Offset, i - Offset);
            }
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
                if (!Predicate(item)) continue;
                if (!await predicate(item, cancellationToken).DynamicContext()) yield break;
                yield return item;
            }
        }

        /// <inheritdoc/>
        public T[] ToArray()
        {
            if (Length < 1) return [];
            using RentedMemoryRef<T> buffer = new(Length, clean: false);
            int len = ToBuffer(buffer.Span);
            return len < 1
                ? []
                : buffer.Span[..len].ToArray();
        }

        /// <inheritdoc/>
        public int ToBuffer(in Span<T> buffer)
        {
            if (Length < 1) return 0;
            Span<T> data = Array;
            T item;
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
        public List<T> ToList()
        {
            if (Length < 1) return [];
            List<T> res = new(Length);
            Span<T> data = Array;
            T item;
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

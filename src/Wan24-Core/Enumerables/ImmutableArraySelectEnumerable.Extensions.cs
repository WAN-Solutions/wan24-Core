using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace wan24.Core.Enumerables
{
    // Extensions
    public partial class ImmutableArraySelectEnumerable<tItem, tResult>
    {
        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tResult2">Result type</typeparam>
        /// <param name="selector">Selector</param>
        /// <returns>Enumerable</returns>
        public virtual ImmutableArraySelectEnumerable<tItem, tResult2> Select<tResult2>(Func<tResult, tResult2> selector)
            => new(Array, item => selector(Selector(item)), Offset, Length);

        /// <inheritdoc/>
        public virtual int Count() => Length;

        /// <inheritdoc/>
        public virtual int Count(Func<tResult, bool> predicate)
        {
            ImmutableArray<tItem> data = Array;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (predicate(Selector(data[i])))
                    res++;
            return res;
        }

        /// <inheritdoc/>
        public virtual async Task<int> CountAsync(Func<tResult, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            ImmutableArray<tItem> data = Array;
            int res = 0;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                if (await predicate(Selector(data[i]), cancellationToken).DynamicContext())
                    res++;
            return res;
        }

        /// <inheritdoc/>
        public virtual bool Contains(tResult obj)
        {
            if (Length == 0) return false;
            ImmutableArray<tItem> data = Array;
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
            ImmutableArray<tItem> data = Array;
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
            ImmutableArray<tItem> data = Array;
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
        public virtual bool ContainsAtLeast(in int count) => Length >= count;

        /// <inheritdoc/>
        public virtual bool ContainsAtMost(in int count) => Length <= count;

        /// <inheritdoc/>
        public virtual bool All(in Func<tResult, bool> predicate) => Count(predicate) == Length;

        /// <inheritdoc/>
        public virtual async Task<bool> AllAsync(Func<tResult, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
            => await CountAsync(predicate, cancellationToken).DynamicContext() == Length;

        /// <inheritdoc/>
        public virtual bool Any() => Length > 0;

        /// <inheritdoc/>
        public virtual bool Any(Func<tResult, bool> predicate)
        {
            ImmutableArray<tItem> data = Array;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (predicate(Selector(data[i])))
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> AnyAsync(Func<tResult, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            ImmutableArray<tItem> data = Array;
            for (int i = Offset, len = i + Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                if (await predicate(Selector(data[i]), cancellationToken).DynamicContext())
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual void ExecuteForAll(in Action<tResult> action)
        {
            ImmutableArray<tItem> data = Array;
            for (int i = Offset, len = i + Length; i < len; action(Selector(data[i])), i++) ;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<tReturn> ExecuteForAll<tReturn>(Func<tResult, EnumerableExtensions.ExecuteResult<tReturn>> action)
        {
            ImmutableArray<tItem> data = Array;
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
            ImmutableArray<tItem> data = Array;
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
            ImmutableArray<tItem> data = Array;
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
            if (!dispose) return Length;
            ImmutableArray<tItem> data = Array;
            for (int i = Offset, len = i + Length; i < len; Selector(data[i])?.TryDispose(), i++) ;
            return Length;
        }

        /// <inheritdoc/>
        public virtual async Task<int> DiscardAllAsync(bool dispose = true)
        {
            if (!dispose) return Length;
            ImmutableArray<tItem> data = Array;
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
            using RentedArrayStructSimple<int> seenHashCodes = new(Length, clean: false);// Seen items hash codes
            int[] seenHashCodesArray = seenHashCodes.Array;// Seen items hash codes array
            using RentedArrayStructSimple<tResult> seen = new(Length, clean: false);// Seen items
            tResult[] seenArray = seen.Array;// Seen items array
            bool useItem,// If to use the current item
                isItemNull;// If the current item is NULL
            tResult item,// Current item
                seenItem;// Seen item
            ImmutableArray<tItem> data = Array;
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
            using RentedArrayStructSimple<int> seenHashCodes = new(Length, clean: false);// Seen keys hash codes
            int[] seenHashCodesArray = seenHashCodes.Array;// Seen keys hash codes array
            using RentedArrayStructSimple<tKey> seen = new(Length, clean: false);// Seen keys
            tKey[] seenArray = seen.Array;// Seen keys array
            bool useItem,// If to use the current item
                isKeyNull;// If the current key is NULL
            tResult item;// Current item
            tKey key,// Current key
                seenKey;// Seen key
            ImmutableArray<tItem> data = Array;
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
            using RentedArrayStructSimple<int> seenHashCodes = new(Length, clean: false);// Seen keys hash codes
            int[] seenHashCodesArray = seenHashCodes.Array;// Seen keys hash codes array
            using RentedArrayStructSimple<tKey> seen = new(Length, clean: false);// Seen keys
            tKey[] seenArray = seen.Array;// Seen keys array
            bool useItem,// If to use the current item
                isKeyNull;// If the current key is NULL
            tResult item;// Current item
            tKey key,// Current key
                seenKey;// Seen key
            ImmutableArray<tItem> data = Array;
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
        public virtual tResult First() => Length > 0 ? Selector(Array[Offset]) : throw new InvalidOperationException("Sequence contains no elements");

        /// <inheritdoc/>
        public virtual tResult First(Func<tResult, bool> predicate)
        {
            ImmutableArray<tItem> data = Array;
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
            ImmutableArray<tItem> data = Array;
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
        public virtual tResult? FirstOrDefault(tResult? defaultValue = default) => Length > 0 ? Selector(Array[Offset]) : defaultValue;

        /// <inheritdoc/>
        public virtual tResult? FirstOrDefault(Func<tResult, bool> predicate, tResult? defaultValue = default)
        {
            ImmutableArray<tItem> data = Array;
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
            ImmutableArray<tItem> data = Array;
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
        public virtual ImmutableArraySelectEnumerable<tItem, tResult> Skip(int count)
            => count >= Length
                ? Empty
                : count < 1
                    ? this
                    : new(Array, Selector, Offset + count, Length - count);

        /// <summary>
        /// Skip while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ImmutableArraySelectEnumerable<tItem, tResult> SkipWhile(Func<tResult, bool> predicate)
        {
            ImmutableArray<tItem> data = Array;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (!predicate(Selector(data[i])))
                    return new(Array, Selector, i, len - i);
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
            ImmutableArray<tItem> data = Array;
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
        public virtual ImmutableArraySelectEnumerable<tItem, tResult> Take(int count)
            => count < 1
                ? Empty
                : count < Length
                    ? new(Array, Selector, Offset, count)
                    : this;

        /// <summary>
        /// Take while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        public virtual ImmutableArraySelectEnumerable<tItem, tResult> TakeWhile(Func<tResult, bool> predicate)
        {
            ImmutableArray<tItem> data = Array;
            for (int i = Offset, len = i + Length; i < len; i++)
                if (!predicate(Selector(data[i])))
                    return i == Offset
                        ? Empty
                        : new(Array, Selector, Offset, i - Offset);
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
            ImmutableArray<tItem> data = Array;
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
            if (Length < 1) return [];
            ImmutableArray<tItem> data = Array;
            tResult[] res = new tResult[Length];
            for (int i = Offset, len = i + Length; i < len; res[i] = Selector(data[i]), i++) ;
            return res;
        }

        /// <inheritdoc/>
        public virtual int ToBuffer(in Span<tResult> buffer)
        {
            if (Length < 1) return 0;
            if (buffer.Length < Length) throw new OutOfMemoryException("Buffer to small");
            ImmutableArray<tItem> data = Array;
            for (int i = Offset, len = i + Length; i < len; buffer[i] = Selector(data[i]), i++) ;
            return Length;
        }

        /// <inheritdoc/>
        public virtual List<tResult> ToList()
        {
            if (Length < 1) return [];
            List<tResult> res = new(Length);
            ImmutableArray<tItem> data = Array;
            for (int i = Offset, len = i + Length; i < len; res.Add(Selector(data[i])), i++) ;
            return res;
        }
    }
}

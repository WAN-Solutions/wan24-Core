using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Execute
    public static partial class EnumerableExtensions
    {
        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <returns>Result</returns>
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this tItem[] enumerable, Func<tItem, (tResult Result, bool Next)> action)
        {
            bool next;
            tResult result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                (result, next) = action(enumerable[i]);
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this tItem[] enumerable,
            Func<tItem, CancellationToken, Task<(tResult Result, bool Next)>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            bool next;
            tResult result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                (result, next) = await action(enumerable[i], cancellationToken).DynamicContext();
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <returns>Result</returns>
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this Memory<tItem> enumerable, Func<tItem, (tResult Result, bool Next)> action)
        {
            bool next;
            tResult result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                (result, next) = action(enumerable.Span[i]);
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this Memory<tItem> enumerable,
            Func<tItem, CancellationToken, Task<(tResult Result, bool Next)>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            bool next;
            tResult result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                (result, next) = await action(enumerable.Span[i], cancellationToken).DynamicContext();
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <returns>Result</returns>
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this ReadOnlyMemory<tItem> enumerable, Func<tItem, (tResult Result, bool Next)> action)
        {
            bool next;
            tResult result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                (result, next) = action(enumerable.Span[i]);
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this ReadOnlyMemory<tItem> enumerable,
            Func<tItem, CancellationToken, Task<(tResult Result, bool Next)>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            bool next;
            tResult result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                (result, next) = await action(enumerable.Span[i], cancellationToken).DynamicContext();
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <returns>Result</returns>
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this IEnumerable<tItem> enumerable, Func<tItem, (tResult Result, bool Next)> action)
        {
            bool next;
            tResult result;
            foreach(tItem item in enumerable)
            {
                (result, next) = action(item);
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this IEnumerable<tItem> enumerable, 
            Func<tItem, CancellationToken, Task<(tResult Result, bool Next)>> action, 
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            bool next;
            tResult result;
            foreach (tItem item in enumerable)
            {
                (result, next) = await action(item, cancellationToken).DynamicContext();
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this IAsyncEnumerable<tItem> enumerable,
            Func<tItem, CancellationToken, Task<(tResult Result, bool Next)>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            bool next;
            tResult result;
            await foreach (tItem item in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                (result, next) = await action(item, cancellationToken).DynamicContext();
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <returns>Result</returns>
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this List<tItem> enumerable, Func<tItem, (tResult Result, bool Next)> action)
        {
            bool next;
            tResult result;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                (result, next) = action(enumerable[i]);
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this List<tItem> enumerable,
            Func<tItem, CancellationToken, Task<(tResult Result, bool Next)>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            bool next;
            tResult result;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                (result, next) = await action(enumerable[i], cancellationToken).DynamicContext();
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <returns>Result</returns>
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this IList<tItem> enumerable, Func<tItem, (tResult Result, bool Next)> action)
        {
            bool next;
            tResult result;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                (result, next) = action(enumerable[i]);
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this IList<tItem> enumerable,
            Func<tItem, CancellationToken, Task<(tResult Result, bool Next)>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            bool next;
            tResult result;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                (result, next) = await action(enumerable[i], cancellationToken).DynamicContext();
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <returns>Result</returns>
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this FrozenSet<tItem> enumerable, Func<tItem, (tResult Result, bool Next)> action)
        {
            bool next;
            tResult result;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                (result, next) = action(enumerable.Items[i]);
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this FrozenSet<tItem> enumerable,
            Func<tItem, CancellationToken, Task<(tResult Result, bool Next)>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            bool next;
            tResult result;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                (result, next) = await action(enumerable.Items[i], cancellationToken).DynamicContext();
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <returns>Result</returns>
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this ImmutableArray<tItem> enumerable, Func<tItem, (tResult Result, bool Next)> action)
        {
            bool next;
            tResult result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                (result, next) = action(enumerable[i]);
                if (!next) yield break;
                yield return result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this ImmutableArray<tItem> enumerable,
            Func<tItem, CancellationToken, Task<(tResult Result, bool Next)>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            bool next;
            tResult result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                (result, next) = await action(enumerable[i], cancellationToken).DynamicContext();
                if (!next) yield break;
                yield return result;
            }
        }
    }
}

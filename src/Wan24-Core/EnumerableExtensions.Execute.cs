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
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this tItem[] enumerable, Func<tItem, ExecuteResult<tResult>> action)
        {
            ExecuteResult<tResult> result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                result = action(enumerable[i]);
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
            Func<tItem, CancellationToken, Task<ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            ExecuteResult<tResult> result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                result = await action(enumerable[i], cancellationToken).DynamicContext();
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this Memory<tItem> enumerable, Func<tItem, ExecuteResult<tResult>> action)
        {
            ExecuteResult<tResult> result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                result = action(enumerable.Span[i]);
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
            Func<tItem, CancellationToken, Task<ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            ExecuteResult<tResult> result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                result = await action(enumerable.Span[i], cancellationToken).DynamicContext();
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this ReadOnlyMemory<tItem> enumerable, Func<tItem, ExecuteResult<tResult>> action)
        {
            ExecuteResult<tResult> result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                result = action(enumerable.Span[i]);
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
            Func<tItem, CancellationToken, Task<ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            ExecuteResult<tResult> result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                result = await action(enumerable.Span[i], cancellationToken).DynamicContext();
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this IEnumerable<tItem> enumerable, Func<tItem, ExecuteResult<tResult>> action)
        {
            ExecuteResult<tResult> result;
            foreach (tItem item in enumerable)
            {
                result = action(item);
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
            Func<tItem, CancellationToken, Task<ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            ExecuteResult<tResult> result;
            foreach (tItem item in enumerable)
            {
                result = await action(item, cancellationToken).DynamicContext();
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
            Func<tItem, CancellationToken, Task<ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            ExecuteResult<tResult> result;
            await foreach (tItem item in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                result = await action(item, cancellationToken).DynamicContext();
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this List<tItem> enumerable, Func<tItem, ExecuteResult<tResult>> action)
        {
            ExecuteResult<tResult> result;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                result = action(enumerable[i]);
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
            Func<tItem, CancellationToken, Task<ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            ExecuteResult<tResult> result;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                result = await action(enumerable[i], cancellationToken).DynamicContext();
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this IList<tItem> enumerable, Func<tItem, ExecuteResult<tResult>> action)
        {
            ExecuteResult<tResult> result;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                result = action(enumerable[i]);
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
            Func<tItem, CancellationToken, Task<ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            ExecuteResult<tResult> result;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                result = await action(enumerable[i], cancellationToken).DynamicContext();
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this FrozenSet<tItem> enumerable, Func<tItem, ExecuteResult<tResult>> action)
        {
            ExecuteResult<tResult> result;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                result = action(enumerable.Items[i]);
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
            Func<tItem, CancellationToken, Task<ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            ExecuteResult<tResult> result;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                result = await action(enumerable.Items[i], cancellationToken).DynamicContext();
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this ImmutableArray<tItem> enumerable, Func<tItem, ExecuteResult<tResult>> action)
        {
            ExecuteResult<tResult> result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                result = action(enumerable[i]);
                if (!result.Next) yield break;
                if (result) yield return result.Result;
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
            Func<tItem, CancellationToken, Task<ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            ExecuteResult<tResult> result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                result = await action(enumerable[i], cancellationToken).DynamicContext();
                if (!result.Next) yield break;
                if (result) yield return result.Result;
            }
        }
    }
}

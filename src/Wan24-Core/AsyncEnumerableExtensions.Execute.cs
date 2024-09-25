using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Execute
    public static partial class AsyncEnumerableExtensions
    {
        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this tItem[] enumerable,
            Func<tItem, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            EnumerableExtensions.ExecuteResult<tResult> result;
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this Memory<tItem> enumerable,
            Func<tItem, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            EnumerableExtensions.ExecuteResult<tResult> result;
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this ReadOnlyMemory<tItem> enumerable,
            Func<tItem, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            EnumerableExtensions.ExecuteResult<tResult> result;
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this IEnumerable<tItem> enumerable,
            Func<tItem, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            EnumerableExtensions.ExecuteResult<tResult> result;
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this IAsyncEnumerable<tItem> enumerable,
            Func<tItem, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            EnumerableExtensions.ExecuteResult<tResult> result;
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this List<tItem> enumerable,
            Func<tItem, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            EnumerableExtensions.ExecuteResult<tResult> result;
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this IList<tItem> enumerable,
            Func<tItem, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            EnumerableExtensions.ExecuteResult<tResult> result;
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this FrozenSet<tItem> enumerable,
            Func<tItem, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            EnumerableExtensions.ExecuteResult<tResult> result;
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this ImmutableArray<tItem> enumerable,
            Func<tItem, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tResult>>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            EnumerableExtensions.ExecuteResult<tResult> result;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                result = await action(enumerable[i], cancellationToken).DynamicContext();
                if (!result.Next) yield break;
                if (result) yield return result.Result;
            }
        }
    }
}

using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime;
using System.Runtime.CompilerServices;
using wan24.Core.Enumerables;

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
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this tItem[] enumerable,
            Func<tItem, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tResult>>> action,
            int offset = 0,
            int? length = null,
            CancellationToken cancellationToken = default
            )
            => new ArrayEnumerable<tItem>(enumerable, offset, length).ExecuteForAllAsync(action, cancellationToken);

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this ImmutableArray<tItem> enumerable,
            Func<tItem, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tResult>>> action,
            int offset = 0,
            int? length = null,
            CancellationToken cancellationToken = default
            )
            => new ImmutableArrayEnumerable<tItem>(enumerable, offset, length).ExecuteForAllAsync(action, cancellationToken);

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IAsyncEnumerable<tResult> ExecuteForAllAsync<tItem, tResult>(
            this FrozenSet<tItem> enumerable,
            Func<tItem, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tResult>>> action,
            int offset = 0,
            int? length = null,
            CancellationToken cancellationToken = default
            )
            => new ImmutableArrayEnumerable<tItem>(enumerable.Items, offset, length).ExecuteForAllAsync(action, cancellationToken);

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IAsyncEnumerable<tResult> ExecuteForAllAsync<tList, tItem, tResult>(
            this tList enumerable,
            Func<tItem, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tResult>>> action,
            int offset = 0,
            int? length = null,
            CancellationToken cancellationToken = default
            )
            where tList : IList<tItem>
            => new ListEnumerable<tList, tItem>(enumerable, offset, length).ExecuteForAllAsync(action, cancellationToken);

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Task ExecuteForAllAsync<T>(
            this T[] enumerable,
            Func<T, CancellationToken, Task> action,
            int offset = 0,
            int? length = null,
            CancellationToken cancellationToken = default
            )
            => new ArrayEnumerable<T>(enumerable, offset, length).ExecuteForAllAsync(action, cancellationToken);

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Task ExecuteForAllAsync<T>(
            this ImmutableArray<T> enumerable,
            Func<T, CancellationToken, Task> action,
            int offset = 0,
            int? length = null,
            CancellationToken cancellationToken = default
            )
            => new ImmutableArrayEnumerable<T>(enumerable, offset, length).ExecuteForAllAsync(action, cancellationToken);

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Task ExecuteForAllAsync<T>(
            this FrozenSet<T> enumerable,
            Func<T, CancellationToken, Task> action,
            int offset = 0,
            int? length = null,
            CancellationToken cancellationToken = default
            )
            => new ImmutableArrayEnumerable<T>(enumerable.Items, offset, length).ExecuteForAllAsync(action, cancellationToken);

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Task ExecuteForAllAsync<tList, tItem>(
            this tList enumerable,
            Func<tItem, CancellationToken, Task> action,
            int offset = 0,
            int? length = null,
            CancellationToken cancellationToken = default
            )
            where tList : IList<tItem>
            => new ListEnumerable<tList, tItem>(enumerable, offset, length).ExecuteForAllAsync(action, cancellationToken);

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
            this IAsyncEnumerable<tItem> enumerable,
            Func<tItem, EnumerableExtensions.ExecuteResult<tResult>> action,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            EnumerableExtensions.ExecuteResult<tResult> result;
            await foreach (tItem item in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                result = action(item);
                if (!result.Next) yield break;
                if (result) yield return result.Result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task ExecuteForAllAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, CancellationToken, Task> action,
            CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                await action(item, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task ExecuteForAllAsync<T>(
            this IEnumerable<T> enumerable,
            Func<T, CancellationToken, Task> action,
            CancellationToken cancellationToken = default
            )
        {
            foreach (T item in enumerable)
                await action(item, cancellationToken).DynamicContext();
        }
    }
}

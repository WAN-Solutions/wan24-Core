using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Delegate extensions
    /// </summary>
    public static class DelegateExtensions
    {
        /// <summary>
        /// Invoke all delegates
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static void InvokeAll<T>(this IEnumerable<T> delegates, params object?[] param) where T : Delegate
        {
            foreach (T d in delegates) d.Method.InvokeAuto(obj: null, param);
        }

        /// <summary>
        /// Invoke all delegates (need to return <c>tResult</c>)
        /// </summary>
        /// <typeparam name="tDelegate">Delegate type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        public static IEnumerable<tResult?> InvokeAll<tDelegate, tResult>(this IEnumerable<tDelegate> delegates, params object?[] param) where tDelegate : Delegate
        {
            List<tResult?> res = [];
            foreach (tDelegate d in delegates)
            {
                if (!d.Method.ReturnType.IsAssignableFrom(typeof(tResult)))
                    throw new ArgumentException($"Result type {typeof(tResult)} doesn't match delegates return type {d.Method.ReturnType}", nameof(tResult));
                res.Add(d.Method.InvokeAuto<tResult>(obj: null, param));
            }
            return res;
        }

        /// <summary>
        /// Invoke all asynchronous delegates (need to return a task)
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task InvokeAllAsync<T>(this IEnumerable<T> delegates, CancellationToken cancellationToken = default, params object?[] param) where T : Delegate
        {
            foreach (T d in delegates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await d.Method.InvokeAutoAsync(obj: null, param).DynamicContext();
            }
        }

        /// <summary>
        /// Invoke all delegates (need to return a task of <c>tResult</c>)
        /// </summary>
        /// <typeparam name="tDelegate">Delegate type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="param">Parameters</param>
        public static async IAsyncEnumerable<tResult?> InvokeAllAsync<tDelegate, tResult>(
            this IEnumerable<tDelegate> delegates,
            [EnumeratorCancellation] CancellationToken cancellationToken = default,
            params object?[] param
            )
            where tDelegate : Delegate
        {
            foreach (tDelegate d in delegates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!d.Method.ReturnType.IsAssignableFrom(typeof(Task<tResult>)))
                    throw new ArgumentException($"Result type {typeof(tResult)} doesn't match delegates return type {d.Method.ReturnType}", nameof(tResult));
                yield return await d.Method.InvokeAutoAsync<tResult>(obj: null, param).DynamicContext();
            }
        }

        /// <summary>
        /// Invoke all delegates (need to return a task)
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task InvokeAllAsync<T>(this IAsyncEnumerable<T> delegates, CancellationToken cancellationToken = default, params object?[] param) where T : Delegate
        {
            await foreach (T d in delegates.DynamicContext().WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await d.Method.InvokeAutoAsync(obj: null, param).DynamicContext();
            }
        }

        /// <summary>
        /// Invoke all delegates (need to return a task of <c>tResult</c>)
        /// </summary>
        /// <typeparam name="tDelegate">Delegate type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="param">Parameters</param>
        public static async IAsyncEnumerable<tResult?> InvokeAllAsync<tDelegate, tResult>(
            this IAsyncEnumerable<tDelegate> delegates,
            [EnumeratorCancellation] CancellationToken cancellationToken = default,
            params object?[] param
            )
            where tDelegate : Delegate
        {
            await foreach (tDelegate d in delegates.DynamicContext().WithCancellation(cancellationToken))
            {
                if (!d.Method.ReturnType.IsAssignableFrom(typeof(Task<tResult>)))
                    throw new ArgumentException($"Result type {typeof(tResult)} doesn't match delegates return type {d.Method.ReturnType}", nameof(tResult));
                yield return await d.Method.InvokeAutoAsync<tResult>(obj: null, param).DynamicContext();
            }
        }
    }
}

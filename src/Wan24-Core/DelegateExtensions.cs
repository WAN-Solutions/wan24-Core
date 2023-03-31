using System.Reflection;
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
        public static void InvokeAll<T>(this IEnumerable<T> delegates, params object?[] param) where T : Delegate
        {
            MethodInfo? mi = null;
            foreach (T d in delegates)
            {
                mi ??= d.Method;
                mi.InvokeAuto(obj: null, param);
            }
        }

        /// <summary>
        /// Invoke all delegates
        /// </summary>
        /// <typeparam name="tDelegate">Delegate type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        public static IEnumerable<tResult?> InvokeAll<tDelegate, tResult>(this IEnumerable<tDelegate> delegates, params object?[] param) where tDelegate : Delegate
        {
            List<tResult?> res = new();
            MethodInfo? mi = null;
            foreach (tDelegate d in delegates)
            {
                if (mi == null)
                {
                    mi = d.Method;
                    if (!mi.ReturnType.IsAssignableFrom(typeof(tResult)))
                        throw new ArgumentException($"Result type {typeof(tResult)} doesn't match delegates return type {mi.ReturnType}", nameof(tResult));
                }
                res.Add((tResult?)mi.InvokeAuto(obj: null, param));
            }
            return res;
        }

        /// <summary>
        /// Invoke all delegates
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="param">Parameters</param>
        public static async Task InvokeAllAsync<T>(this IEnumerable<T> delegates, CancellationToken cancellationToken = default, params object?[] param) where T : Delegate
        {
            MethodInfo? mi = null;
            foreach (T d in delegates)
            {
                if (cancellationToken.IsCancellationRequested) return;
                mi ??= d.Method;
                await mi.InvokeAutoAsync(obj: null, param).DynamicContext();
            }
        }

        /// <summary>
        /// Invoke all delegates
        /// </summary>
        /// <typeparam name="tDelegate">Delegate type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="param">Parameters</param>
        public static async IAsyncEnumerable<tResult?> InvokeAllAsync<tDelegate, tResult>(
            this IEnumerable<tDelegate> delegates,
            [EnumeratorCancellation] CancellationToken cancellationToken = default,
            params object?[] param) where tDelegate : Delegate
        {
            MethodInfo? mi = null;
            foreach (tDelegate d in delegates)
            {
                if (mi == null)
                {
                    mi = d.Method;
                    if (!mi.ReturnType.IsAssignableFrom(typeof(tResult)))
                        throw new ArgumentException($"Result type {typeof(tResult)} doesn't match delegates return type {mi.ReturnType}", nameof(tResult));
                }
                yield return (tResult?)(await mi.InvokeAutoAsync(obj: null, param).DynamicContext());
            }
        }

        /// <summary>
        /// Invoke all delegates
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="param">Parameters</param>
        public static async Task InvokeAllAsync<T>(this IAsyncEnumerable<T> delegates, CancellationToken cancellationToken = default, params object?[] param) where T : Delegate
        {
            MethodInfo? mi = null;
            await foreach (T d in delegates.DynamicContext().WithCancellation(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested) return;
                mi ??= d.Method;
                await mi.InvokeAutoAsync(obj: null, param).DynamicContext();
            }
        }

        /// <summary>
        /// Invoke all delegates
        /// </summary>
        /// <typeparam name="tDelegate">Delegate type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="param">Parameters</param>
        public static async IAsyncEnumerable<tResult?> InvokeAllAsync<tDelegate, tResult>(
            this IAsyncEnumerable<tDelegate> delegates,
            [EnumeratorCancellation] CancellationToken cancellationToken = default,
            params object?[] param) where tDelegate : Delegate
        {
            MethodInfo? mi = null;
            await foreach (tDelegate d in delegates.DynamicContext().WithCancellation(cancellationToken))
            {
                if (mi == null)
                {
                    mi = d.Method;
                    if (!mi.ReturnType.IsAssignableFrom(typeof(tResult)))
                        throw new ArgumentException($"Result type {typeof(tResult)} doesn't match delegates return type {mi.ReturnType}", nameof(tResult));
                }
                yield return (tResult?)(await mi.InvokeAutoAsync(obj: null, param).DynamicContext());
            }
        }
    }
}

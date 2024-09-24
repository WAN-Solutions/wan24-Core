using System.Collections.Frozen;
using System.Collections.Immutable;
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
        /// Invoke all delegates
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static void InvokeAll<T>(this ReadOnlySpan<T> delegates, params object?[] param) where T : Delegate
        {
            for (int i = 0, len = delegates.Length; i < len; delegates[i].Method.InvokeAuto(obj: null, param), i++) ;
        }

        /// <summary>
        /// Invoke all delegates
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void InvokeAll<T>(this Span<T> delegates, params object?[] param) where T : Delegate => InvokeAll((ReadOnlySpan<T>)delegates, param);

        /// <summary>
        /// Invoke all delegates
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void InvokeAll<T>(this Memory<T> delegates, params object?[] param) where T : Delegate => InvokeAll((ReadOnlySpan<T>)delegates.Span, param);

        /// <summary>
        /// Invoke all delegates
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void InvokeAll<T>(this ReadOnlyMemory<T> delegates, params object?[] param) where T : Delegate => InvokeAll(delegates.Span, param);

        /// <summary>
        /// Invoke all delegates
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void InvokeAll<T>(this T[] delegates, params object?[] param) where T : Delegate => InvokeAll((ReadOnlySpan<T>)delegates, param);

        /// <summary>
        /// Invoke all delegates
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static void InvokeAll<T>(this IList<T> delegates, params object?[] param) where T : Delegate
        {
            for (int i = 0, len = delegates.Count; i < len; delegates[i].Method.InvokeAuto(obj: null, param), i++) ;
        }

        /// <summary>
        /// Invoke all delegates
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static void InvokeAll<T>(this List<T> delegates, params object?[] param) where T : Delegate
        {
            for (int i = 0, len = delegates.Count; i < len; delegates[i].Method.InvokeAuto(obj: null, param), i++) ;
        }

        /// <summary>
        /// Invoke all delegates
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static void InvokeAll<T>(this ImmutableArray<T> delegates, params object?[] param) where T : Delegate
        {
            for (int i = 0, len = delegates.Length; i < len; delegates[i].Method.InvokeAuto(obj: null, param), i++) ;
        }

        /// <summary>
        /// Invoke all delegates
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static void InvokeAll<T>(this FrozenSet<T> delegates, params object?[] param) where T : Delegate
        {
            for (int i = 0, len = delegates.Count; i < len; delegates.Items[i].Method.InvokeAuto(obj: null, param), i++) ;
        }

        /// <summary>
        /// Invoke all delegates (need to return <c>tResult</c>)
        /// </summary>
        /// <typeparam name="tDelegate">Delegate type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        public static tResult?[] InvokeAll<tDelegate, tResult>(this IEnumerable<tDelegate> delegates, params object?[] param) where tDelegate : Delegate
        {
            List<tResult?> res = [];
            foreach (tDelegate d in delegates)
            {
                if (!d.Method.ReturnType.IsAssignableFrom(typeof(tResult)))
                    throw new ArgumentException($"Result type {typeof(tResult)} doesn't match delegates return type {d.Method.ReturnType}", nameof(tResult));
                res.Add(d.Method.InvokeAuto<tResult>(obj: null, param));
            }
            return [.. res];
        }

        /// <summary>
        /// Invoke all delegates (need to return <c>tResult</c>)
        /// </summary>
        /// <typeparam name="tDelegate">Delegate type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        public static tResult?[] InvokeAll<tDelegate, tResult>(this ReadOnlySpan<tDelegate> delegates, params object?[] param) where tDelegate : Delegate
        {
            int len = delegates.Length;
            tResult?[] res = new tResult?[len];
            for(int i = 0; i < len; i++)
            {
                if (!delegates[i].Method.ReturnType.IsAssignableFrom(typeof(tResult)))
                    throw new ArgumentException($"Result type {typeof(tResult)} doesn't match delegates return type {delegates[i].Method.ReturnType}", nameof(tResult));
                res[i] = delegates[i].Method.InvokeAuto<tResult>(obj: null, param);
            }
            return res;
        }

        /// <summary>
        /// Invoke all delegates (need to return <c>tResult</c>)
        /// </summary>
        /// <typeparam name="tDelegate">Delegate type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        public static tResult?[] InvokeAll<tDelegate, tResult>(this ImmutableArray<tDelegate> delegates, params object?[] param) where tDelegate : Delegate
        {
            int len = delegates.Length;
            tResult?[] res = new tResult?[len];
            for (int i = 0; i < len; i++)
            {
                if (!delegates[i].Method.ReturnType.IsAssignableFrom(typeof(tResult)))
                    throw new ArgumentException($"Result type {typeof(tResult)} doesn't match delegates return type {delegates[i].Method.ReturnType}", nameof(tResult));
                res[i] = delegates[i].Method.InvokeAuto<tResult>(obj: null, param);
            }
            return res;
        }

        /// <summary>
        /// Invoke all delegates (need to return <c>tResult</c>)
        /// </summary>
        /// <typeparam name="tDelegate">Delegate type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        public static tResult?[] InvokeAll<tDelegate, tResult>(this FrozenSet<tDelegate> delegates, params object?[] param) where tDelegate : Delegate
        {
            int len = delegates.Count;
            tResult?[] res = new tResult?[len];
            for (int i = 0; i < len; i++)
            {
                if (!delegates.Items[i].Method.ReturnType.IsAssignableFrom(typeof(tResult)))
                    throw new ArgumentException($"Result type {typeof(tResult)} doesn't match delegates return type {delegates.Items[i].Method.ReturnType}", nameof(tResult));
                res[i] = delegates.Items[i].Method.InvokeAuto<tResult>(obj: null, param);
            }
            return res;
        }

        /// <summary>
        /// Invoke all delegates (need to return <c>tResult</c>)
        /// </summary>
        /// <typeparam name="tDelegate">Delegate type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static tResult?[] InvokeAll<tDelegate, tResult>(this tDelegate[] delegates, params object?[] param) where tDelegate : Delegate
            => InvokeAll<tDelegate, tResult>((ReadOnlySpan<tDelegate>)delegates, param);

        /// <summary>
        /// Invoke all delegates (need to return <c>tResult</c>)
        /// </summary>
        /// <typeparam name="tDelegate">Delegate type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static tResult?[] InvokeAll<tDelegate, tResult>(this Span<tDelegate> delegates, params object?[] param) where tDelegate : Delegate
            => InvokeAll<tDelegate, tResult>((ReadOnlySpan<tDelegate>)delegates, param);

        /// <summary>
        /// Invoke all delegates (need to return <c>tResult</c>)
        /// </summary>
        /// <typeparam name="tDelegate">Delegate type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static tResult?[] InvokeAll<tDelegate, tResult>(this Memory<tDelegate> delegates, params object?[] param) where tDelegate : Delegate
            => InvokeAll<tDelegate, tResult>((ReadOnlySpan<tDelegate>)delegates.Span, param);

        /// <summary>
        /// Invoke all delegates (need to return <c>tResult</c>)
        /// </summary>
        /// <typeparam name="tDelegate">Delegate type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="delegates">Delegates</param>
        /// <param name="param">Parameters</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static tResult?[] InvokeAll<tDelegate, tResult>(this ReadOnlyMemory<tDelegate> delegates, params object?[] param) where tDelegate : Delegate
            => InvokeAll<tDelegate, tResult>(delegates.Span, param);

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

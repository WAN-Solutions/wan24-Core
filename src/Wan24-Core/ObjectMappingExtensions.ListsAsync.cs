using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Lists asynchronous
    public static partial class ObjectMappingExtensions
    {
        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="sources">Source objects</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<tTarget> MapToAsync<tSource, tTarget>(
            this IEnumerable<tSource> sources,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            foreach (tSource source in sources) yield return await MapToAsync<tSource, tTarget>(source, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="sources">Source objects</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<tTarget> MapToAsync<tSource, tTarget>(
            this ReadOnlyMemory<tSource> sources,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = sources.Length; i < len; i++)
                yield return await MapToAsync<tSource, tTarget>(sources.Span[i], cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="sources">Source objects</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<tTarget> MapToAsync<tSource, tTarget>(
            this Memory<tSource> sources,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = sources.Length; i < len; i++)
                yield return await MapToAsync<tSource, tTarget>(sources.Span[i], cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="sources">Source objects</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<tTarget> MapToAsync<tSource, tTarget>(
            this tSource[] sources,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = sources.Length; i < len; i++)
                yield return await MapToAsync<tSource, tTarget>(sources[i], cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="sources">Source objects</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<tTarget> MapToAsync<tSource, tTarget>(
            this ImmutableArray<tSource> sources,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = sources.Length; i < len; i++)
                yield return await MapToAsync<tSource, tTarget>(sources[i], cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="sources">Source objects</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<tTarget> MapToAsync<tSource, tTarget>(
            this FrozenSet<tSource> sources,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = sources.Count; i < len; i++)
                yield return await MapToAsync<tSource, tTarget>(sources.Items[i], cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<object> MapObjectsToAsync(
            this IEnumerable<object> sources,
            Type targetType,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            foreach (object source in sources) yield return await MapObjectToAsync(source, targetType, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<object> MapObjectsToAsync(
            this ReadOnlyMemory<object> sources,
            Type targetType,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = sources.Length; i < len; i++)
                yield return await MapObjectToAsync(sources.Span[i], targetType, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<object> MapObjectsToAsync(
            this Memory<object> sources,
            Type targetType,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = sources.Length; i < len; i++)
                yield return await MapObjectToAsync(sources.Span[i], targetType, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<object> MapObjectsToAsync(
            this object[] sources,
            Type targetType,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = sources.Length; i < len; i++)
                yield return await MapObjectToAsync(sources[i], targetType, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<object> MapObjectsToAsync(
            this IList<object> sources,
            Type targetType,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = sources.Count; i < len; i++)
                yield return await MapObjectToAsync(sources[i], targetType, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<object> MapObjectsToAsync(
            this List<object> sources,
            Type targetType,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = sources.Count; i < len; i++)
                yield return await MapObjectToAsync(sources[i], targetType, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<object> MapObjectsToAsync(
            this ImmutableArray<object> sources,
            Type targetType,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = sources.Length; i < len; i++)
                yield return await MapObjectToAsync(sources[i], targetType, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<object> MapObjectsToAsync(
            this FrozenSet<object> sources,
            Type targetType,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = sources.Count; i < len; i++)
                yield return await MapObjectToAsync(sources.Items[i], targetType, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="sources">Source objects</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<tTarget> MapToAsync<tSource, tTarget>(
            this IAsyncEnumerable<tSource> sources,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            await foreach (tSource source in sources.WithCancellation(cancellationToken))
                yield return await MapToAsync<tSource, tTarget>(source, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<object> MapObjectsToAsync(
            this IAsyncEnumerable<object> sources,
            Type targetType,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            await foreach (object source in sources.WithCancellation(cancellationToken))
                yield return await MapObjectToAsync(source, targetType, cancellationToken).DynamicContext();
        }
    }
}

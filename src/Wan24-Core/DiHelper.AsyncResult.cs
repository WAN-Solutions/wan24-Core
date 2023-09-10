using System.Diagnostics.CodeAnalysis;
using System.Runtime;

namespace wan24.Core
{
    // Asynchronous result
    public partial class DiHelper
    {
        /// <summary>
        /// Asynchronous result
        /// </summary>
        public sealed class AsyncResult
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="obj">Object (must not be <see langword="null"/>, if <c>use</c> is <see langword="true"/>)</param>
            /// <param name="use">Use the object?</param>
            /// <exception cref="ArgumentNullException"><c>use</c> is <see langword="true"/>, but the </exception>
            public AsyncResult(object? obj, bool use)
            {
                if (use && obj is null) throw new ArgumentNullException(nameof(obj));
                Object = obj;
                Use = use;
            }

            /// <summary>
            /// Object
            /// </summary>
            public object? Object { get; }

            /// <summary>
            /// Use the object?
            /// </summary>
            [MemberNotNullWhen(returnValue: true, nameof(Object))]
            public bool Use { get; }

            /// <summary>
            /// Get a generic result from this result
            /// </summary>
            /// <typeparam name="T">Object type</typeparam>
            /// <returns>Generic result</returns>
            [TargetedPatchingOptOut("Tiny method")]
            public AsyncResult<T> GetGeneric<T>() => new((T?)Object, Use);
        }

        /// <summary>
        /// Asynchronous result
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        public sealed class AsyncResult<T>
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="obj">Object (must not be <see langword="null"/>, if <c>use</c> is <see langword="true"/>)</param>
            /// <param name="use">Use the object?</param>
            /// <exception cref="ArgumentNullException"><c>use</c> is <see langword="true"/>, but the </exception>
            public AsyncResult(T? obj, bool use)
            {
                if (use && obj is null) throw new ArgumentNullException(nameof(obj));
                Object = obj;
                Use = use;
            }

            /// <summary>
            /// Object
            /// </summary>
            public T? Object { get; }

            /// <summary>
            /// Use the object?
            /// </summary>
            [MemberNotNullWhen(returnValue: true, nameof(Object))]
            public bool Use { get; }
        }
    }
}

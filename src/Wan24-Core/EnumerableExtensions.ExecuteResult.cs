using System.Runtime.InteropServices;

namespace wan24.Core
{
    // Execute result
    public static partial class EnumerableExtensions
    {
        /// <summary>
        /// Execute result
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="Result">Result</param>
        /// <param name="UseResult">If to yield the result</param>
        /// <param name="Next">If to continue with the next item</param>
        [StructLayout(LayoutKind.Sequential)]
        public readonly record struct ExecuteResult<T>(in T Result, in bool UseResult = true, in bool Next = true)
        {
            /// <summary>
            /// If to yield the result
            /// </summary>
            public readonly bool UseResult = UseResult;
            /// <summary>
            /// If to continue with the next item
            /// </summary>
            public readonly bool Next = Next;
            /// <summary>
            /// Result
            /// </summary>
            public readonly T Result = Result;

            /// <summary>
            /// Cast as tuple
            /// </summary>
            /// <param name="result">Result</param>
            public static implicit operator (T result, bool useResult, bool next)(in ExecuteResult<T> result) => (result.Result, result.UseResult, result.Next);

            /// <summary>
            /// Cast from tuple
            /// </summary>
            /// <param name="tuple">Tuple</param>
            public static implicit operator ExecuteResult<T>(in (T Result, bool UseResult, bool Next) tuple) => new(tuple.Result, tuple.UseResult, tuple.Next);

            /// <summary>
            /// Cast from tuple
            /// </summary>
            /// <param name="tuple">Tuple</param>
            public static implicit operator ExecuteResult<T>(in (T Result, bool Next) tuple) => new(tuple.Result, Next: tuple.Next);

            /// <summary>
            /// Cast from <typeparamref name="T"/>
            /// </summary>
            /// <param name="result">Result</param>
            public static implicit operator ExecuteResult<T>(in T result) => new(result);

            /// <summary>
            /// Cast as <see cref="Result"/>
            /// </summary>
            /// <param name="result">Result</param>
            public static implicit operator T(in ExecuteResult<T> result) => result.Result;

            /// <summary>
            /// Cast as <see cref="UseResult"/>
            /// </summary>
            /// <param name="result">Result</param>
            public static implicit operator bool(in ExecuteResult<T> result) => result.UseResult;
        }
    }
}

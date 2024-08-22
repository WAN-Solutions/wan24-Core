using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Result of an asynchronous try-action
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct TryAsyncResult<T> : ITryAsyncResult
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="result">Result (must be non-<see langword="null"/>, if <c>succeed</c> is <see langword="true"/>)</param>
        /// <param name="succeed">If succeed</param>
        public TryAsyncResult(T? result = default, bool? succeed = null)
        {
            if (succeed.HasValue)
            {
                if (succeed.Value != result is not null)
                    throw result is null 
                        ? new ArgumentNullException(nameof(result)) 
                        : new ArgumentException("NULL expected", nameof(result));
            }
            else
            {
                succeed = result is not null;
            }
            Succeed = succeed.Value;
            Result = result;
        }

        /// <inheritdoc/>
        [MemberNotNullWhen(returnValue: true, nameof(Result))]
        public bool Succeed { get; }

        /// <summary>
        /// Action result (not <see langword="null"/>, if <see cref="Succeed"/> is <see langword="true"/>)
        /// </summary>
        public T? Result { get; }

        /// <inheritdoc/>
        object? ITryAsyncResult.Result => Result;

        /// <summary>
        /// Cast as succeed-flag
        /// </summary>
        /// <param name="instance">Instance</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static implicit operator bool(in TryAsyncResult<T> instance) => instance.Succeed;

        /// <summary>
        /// Cast as non-<see langword="null"/> result
        /// </summary>
        /// <param name="instance">Instance</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static implicit operator T(in TryAsyncResult<T> instance) => instance.Result ?? throw new InvalidOperationException();

        /// <summary>
        /// Cast failed result
        /// </summary>
        /// <param name="result"><see langword="false"/></param>
        [TargetedPatchingOptOut("Tiny method")]
        public static implicit operator TryAsyncResult<T>(in bool result)
        {
            if (result) throw new InvalidCastException($"{typeof(TryAsyncResult<T>)} can only be casted from FALSE");
            return new(succeed: false);
        }

        /// <summary>
        /// Cast from succeed result
        /// </summary>
        /// <param name="result">Result</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static implicit operator TryAsyncResult<T>(in T result) => new(result, succeed: true);
    }
}

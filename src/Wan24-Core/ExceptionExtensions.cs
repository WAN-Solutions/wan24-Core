using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="Exception"/> extensions
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Append exception(s)
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="ex">Exception(s) to append</param>
        /// <returns>Aggregate exception</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static AggregateException Append(this Exception exception, params Exception[] ex)
            => exception is AggregateException aex
                ? new([.. aex.InnerExceptions, .. ex])
                : new([exception, .. ex]);
    }
}

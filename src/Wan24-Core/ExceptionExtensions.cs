using Microsoft.Extensions.Logging;
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
        {
            Exception[] innerExceptions;
            int index;
            if (exception is AggregateException aex)
            {
                index = aex.InnerExceptions.Count;
                innerExceptions = new Exception[index + ex.Length];
                aex.InnerExceptions.CopyTo(innerExceptions, index: 0);
            }
            else
            {
                innerExceptions = new Exception[ex.Length + 1];
                innerExceptions[0] = exception;
                index = 1;
            }
            ex.AsSpan().CopyTo(innerExceptions.AsSpan(index));
            return new(innerExceptions);
        }

        /// <summary>
        /// Write an exception to a logger
        /// </summary>
        /// <typeparam name="T">Exception type</typeparam>
        /// <param name="exception">Exception</param>
        /// <param name="logger">Logger</param>
        /// <param name="level">Level</param>
        /// <returns>Exception</returns>
        public static T Log<T>(this T exception, ILogger? logger = null, in LogLevel level = LogLevel.Error) where T : Exception
        {
            (logger ?? Logging.Logger)?.Log(level, "Exception {type} \"{message}\" at {stack}", exception.GetType(), exception.Message, exception.StackTrace);
            return exception;
        }
    }
}

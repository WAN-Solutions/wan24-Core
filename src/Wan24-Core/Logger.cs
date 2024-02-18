using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Logger (adopts to <see cref="Logging"/> - NEVER use this as <see cref="Logging.Logger"/>!)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="level">Log level</param>
    /// <param name="next">Next logger which should receive the message</param>
    public class Logger(in LogLevel? level = null, in ILogger? next = null) : LoggerBase(level, next)
    {
        /// <inheritdoc/>
        protected override void LogInt<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            switch (logLevel)
            {
                case LogLevel.Trace: Logging.WriteTrace(formatter(state, exception)); break;
                case LogLevel.Debug: Logging.WriteDebug(formatter(state, exception)); break;
                case LogLevel.Information: Logging.WriteInfo(formatter(state, exception)); break;
                case LogLevel.Warning: Logging.WriteWarning(formatter(state, exception)); break;
                case LogLevel.Error: Logging.WriteError(formatter(state, exception)); break;
                case LogLevel.Critical: Logging.WriteCritical(formatter(state, exception)); break;
                default: throw new NotSupportedException($"Unsupported log level {logLevel}");
            }
        }

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="level">Level</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteLog(string str, in LogLevel level = LogLevel.Information, params object?[] args) => Logging.WriteLog(str, level, args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteTrace(string str, params object?[] args) => WriteLog(str, LogLevel.Trace, args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteDebug(string str, params object?[] args) => WriteLog(str, LogLevel.Debug, args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteInfo(string str, params object?[] args) => WriteLog(str, args: args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteWarning(string str, params object?[] args) => WriteLog(str, LogLevel.Warning, args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteError(string str, params object?[] args) => WriteLog(str, LogLevel.Error, args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteCritical(string str, params object?[] args) => WriteLog(str, LogLevel.Critical, args);
    }
}

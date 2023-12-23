using Microsoft.Extensions.Logging;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Logging
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// Default log level
        /// </summary>
        public const LogLevel DEFAULT_LOGLEVEL =
#if !RELEASE
            LogLevel.Debug;
#else
            LogLevel.Information;
#endif

        /// <summary>
        /// Logger
        /// </summary>
        private static ILogger? _Logger = null;

        /// <summary>
        /// Logger
        /// </summary>
        public static ILogger? Logger
        {
            get => _Logger;
            set
            {
                if (value is Logger) throw new InvalidOperationException();
                _Logger = value;
            }
        }

        /// <summary>
        /// Determine if tracing
        /// </summary>
        public static bool Trace
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => IsTracing();
        }

        /// <summary>
        /// Determine if debugging
        /// </summary>
        public static bool Debug
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => IsDebugging();
        }

        /// <summary>
        /// Determine if informative
        /// </summary>
        public static bool Info
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => IsInformative();
        }

        /// <summary>
        /// Determine if warning
        /// </summary>
        public static bool Warning
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => IsWarning();
        }

        /// <summary>
        /// Determine if error
        /// </summary>
        public static bool Error
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => IsError();
        }

        /// <summary>
        /// Determine if critical
        /// </summary>
        public static bool Critical
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => IsCritical();
        }

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="level">Level</param>
        /// <param name="args">Arguments</param>
        public static void WriteLog(this string str, in LogLevel level = LogLevel.Information, params object?[] args)
        {
            if (Logger is null)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(str);
#endif
            }
            else
            {
#pragma warning disable CA2254 // Logger call shouldn't be different from the template
                Logger.Log(level, str, args);
#pragma warning restore CA2254
                if (level <= LogLevel.Debug) System.Diagnostics.Debug.WriteLine(str);
            }
        }

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteTrace(this string str, params object?[] args) => WriteLog(str, LogLevel.Trace, args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteDebug(this string str, params object?[] args) => WriteLog(str, LogLevel.Debug, args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteInfo(this string str, params object?[] args) => WriteLog(str, args: args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteWarning(this string str, params object?[] args) => WriteLog(str, LogLevel.Warning, args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteError(this string str, params object?[] args) => WriteLog(str, LogLevel.Error, args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteCritical(this string str, params object?[] args) => WriteLog(str, LogLevel.Critical, args);

        /// <summary>
        /// Determine if tracing
        /// </summary>
        /// <returns>If tracing</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsTracing() => Logger?.GetLogLevel().IsTracing() ?? false;

        /// <summary>
        /// Determine if debugging
        /// </summary>
        /// <returns>If debugging</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsDebugging() => Logger?.GetLogLevel().IsDebugging() ?? false;

        /// <summary>
        /// Determine if informative
        /// </summary>
        /// <returns>If informative</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsInformative() => Logger?.GetLogLevel().IsInformative() ?? false;

        /// <summary>
        /// Determine if warning
        /// </summary>
        /// <returns>If warning</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsWarning() => Logger?.GetLogLevel().IsWarning() ?? false;

        /// <summary>
        /// Determine if error
        /// </summary>
        /// <returns>If error</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsError() => Logger?.GetLogLevel().IsError() ?? false;

        /// <summary>
        /// Determine if critical
        /// </summary>
        /// <returns>If critical</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsCritical() => Logger?.GetLogLevel().IsCritical() ?? false;
    }
}

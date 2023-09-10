using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Logging
    /// </summary>
    public static class Logging
    {
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
                Debug.WriteLine(str);
#endif
            }
            else
            {
#pragma warning disable CA2254 // Logger call shouldn't be different from the template
                Logger.Log(level, str, args);
#pragma warning restore CA2254
                if (level <= LogLevel.Debug) Debug.WriteLine(str);
            }
        }

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void WriteTrace(this string str, params object?[] args) => WriteLog(str, LogLevel.Trace, args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void WriteDebug(this string str, params object?[] args) => WriteLog(str, LogLevel.Debug, args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void WriteInfo(this string str, params object?[] args) => WriteLog(str, args: args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void WriteWarning(this string str, params object?[] args) => WriteLog(str, LogLevel.Warning, args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void WriteError(this string str, params object?[] args) => WriteLog(str, LogLevel.Error, args);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="args">Arguments</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void WriteCritical(this string str, params object?[] args) => WriteLog(str, LogLevel.Critical, args);
    }
}

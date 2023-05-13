using Microsoft.Extensions.Logging;
using System.Diagnostics;

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
        public static ILogger? Logger { get; set; }

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        /// <param name="level">Level</param>
#pragma warning disable CA2254 // Logger call shouldn't be different from the template
        public static void WriteLog(this string str, LogLevel level = LogLevel.Information)
        {
            if (Logger == null)
            {
#if DEBUG
                Debug.WriteLine(str);
#endif
            }
            else
            {
                Logger.Log(level, str);
                if (level <= LogLevel.Debug) Debug.WriteLine(str);
            }
        }
#pragma warning restore CA2254

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        public static void WriteTrace(this string str) => WriteLog(str, LogLevel.Trace);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        public static void WriteDebug(this string str) => WriteLog(str, LogLevel.Debug);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        public static void WriteInfo(this string str) => WriteLog(str);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        public static void WriteWarning(this string str) => WriteLog(str, LogLevel.Warning);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        public static void WriteError(this string str) => WriteLog(str, LogLevel.Error);

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="str">Message</param>
        public static void WriteCritical(this string str) => WriteLog(str, LogLevel.Critical);
    }
}

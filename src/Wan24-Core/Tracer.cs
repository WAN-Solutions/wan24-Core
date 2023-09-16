using Microsoft.Extensions.Logging;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace wan24.Core
{
    /// <summary>
    /// Tracer (could be used to trace informations and flush them to the <see cref="Logging"/> in case of an error)
    /// </summary>
    public sealed class Tracer
    {
        /// <summary>
        /// Entries
        /// </summary>
        private readonly Channel<Entry> Entries;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="level">Level which is required to write to the <see cref="Logging"/> directly</param>
        /// <param name="prefix">Prefix (will be prepended to each message)</param>
        /// <param name="entryCountLimit">Maximum number of entries to trace (overflowing old entries will be removed when adding new entries)</param>
        /// <param name="skipLogger">Skip writing to the logger based on the given level?</param>
        /// <param name="logger">Logger to use</param>
        public Tracer(
            in LogLevel level = LogLevel.Warning, 
            in string? prefix = null, 
            in int entryCountLimit = byte.MaxValue, 
            in bool skipLogger = false, 
            in ILogger? logger = null
            )
        {
            Entries = Channel.CreateBounded<Entry>(new BoundedChannelOptions(entryCountLimit)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            });
            Level = level;
            Prefix = prefix;
            EntryCountLimit = entryCountLimit;
            SkipLogger = skipLogger;
            Logger = logger;
        }

        /// <summary>
        /// Level which is required to write to the <see cref="Logging"/> directly
        /// </summary>
        public LogLevel Level { get; }

        /// <summary>
        /// Prefix (will be prepended to each message)
        /// </summary>
        public string? Prefix { get; }

        /// <summary>
        /// Maximum number of entries to trace (overflowing old entries will be removed when adding new entries)
        /// </summary>
        public int EntryCountLimit { get; }

        /// <summary>
        /// Skip writing to the logger based on the given level?
        /// </summary>
        public bool SkipLogger { get; }

        /// <summary>
        /// Logger to use
        /// </summary>
        public ILogger? Logger { get; }

        /// <summary>
        /// Current number of entries
        /// </summary>
        public int Count => Entries.Reader.Count;

        /// <summary>
        /// Write a message
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="level">Level</param>
        /// <param name="args">Arguments</param>
        /// <returns>This</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public Tracer Write(in string info, in LogLevel level, params object?[] args)
        {
            Entry entry = new(level, $"{Prefix}{info}", args);
            Entries.Writer.TryWrite(entry);
            if (!SkipLogger && entry.Level >= Level)
                if (Logger is null)
                {
                    Logging.WriteLog(entry.Info, level, args);
                }
                else
                {
#pragma warning disable CA2254 // Logger call shouldn't be different from the template
                    Logger.Log(level, info, args);
#pragma warning restore CA2254 // Logger call shouldn't be different from the template
                }
            return this;
        }

        /// <summary>
        /// Write a trace message
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="args">Arguments</param>
        /// <returns>This</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public Tracer WriteTrace(in string info, params object?[] args) => Write(info, LogLevel.Trace, args);

        /// <summary>
        /// Write a debug message
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="args">Arguments</param>
        /// <returns>This</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public Tracer WriteDebug(in string info, params object?[] args) => Write(info, LogLevel.Debug, args);

        /// <summary>
        /// Write a information message
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="args">Arguments</param>
        /// <returns>This</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public Tracer WriteInfo(in string info, params object?[] args) => Write(info, LogLevel.Information, args);

        /// <summary>
        /// Write a warning message
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="args">Arguments</param>
        /// <returns>This</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public Tracer WriteWarning(in string info, params object?[] args) => Write(info, LogLevel.Warning, args);

        /// <summary>
        /// Write an error message
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="args">Arguments</param>
        /// <returns>This</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public Tracer WriteError(in string info, params object?[] args) => Write(info, LogLevel.Error, args);

        /// <summary>
        /// Write a critical error message
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="args">Arguments</param>
        /// <returns>This</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public Tracer WriteCritical(in string info, params object?[] args) => Write(info, LogLevel.Critical, args);

        /// <summary>
        /// Clear written entries
        /// </summary>
        /// <returns>This</returns>
        public Tracer Clear()
        {
            while (Entries.Reader.TryRead(out _)) ;
            return this;
        }

        /// <summary>
        /// Flush traced entries to the <see cref="Logging"/> (and clear; already <see cref="Logging"/> written entries won't be written again)
        /// </summary>
        /// <param name="level">Level required to be written to the <see cref="Logging"/></param>
        /// <returns>This</returns>
        public Tracer Flush(in LogLevel level = LogLevel.None)
        {
            LogLevel minLevel = Level;
            ILogger? logger = Logger;
            while (Entries.Reader.TryRead(out Entry entry))
                if (entry.Level >= level && entry.Level < minLevel)
                    if (logger is null)
                    {
                        Logging.WriteLog($"Traced at {entry.Time}: {entry.Info}", entry.Level, entry.Arguments);
                    }
                    else
                    {
#pragma warning disable CA2254 // Logger call shouldn't be different from the template
                        logger.Log(entry.Level, $"Traced at {entry.Time}: {entry.Info}", entry.Arguments);
#pragma warning restore CA2254 // Logger call shouldn't be different from the template
                    }
            return this;
        }

        /// <summary>
        /// Entry
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public readonly record struct Entry
        {
            /// <summary>
            /// Level
            /// </summary>
            public readonly LogLevel Level;
            /// <summary>
            /// Time
            /// </summary>
            public readonly DateTime Time = DateTime.Now;
            /// <summary>
            /// Info
            /// </summary>
            public readonly string Info;
            /// <summary>
            /// Arguments
            /// </summary>
            public readonly object?[] Arguments;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="level">Level</param>
            /// <param name="info">Info</param>
            /// <param name="args">Arguments</param>
            internal Entry(in LogLevel level, in string info, in object?[] args)
            {
                Level = level;
                Info = info;
                Arguments = args;
            }
        }
    }
}

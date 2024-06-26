﻿using static wan24.Core.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Error handling
    /// </summary>
    public static class ErrorHandling
    {
        /// <summary>
        /// Unspecified error source ID
        /// </summary>
        public const int UNSPECIFIED_ERROR_SOURCE = 0;
        /// <summary>
        /// Unhandled exception error source
        /// </summary>
        public const int UNHANDLED_EXCEPTION = 1;
        /// <summary>
        /// Delayed service error source
        /// </summary>
        public const int DELAYED_SERVICE_ERROR = 2;
        /// <summary>
        /// Disposable adapter error source
        /// </summary>
        public const int DISPOSABLE_ADAPTER_ERROR = 3;
        /// <summary>
        /// Service error source
        /// </summary>
        public const int SERVICE_ERROR = 4;
        // wan24-Crypto is #5
        /// <summary>
        /// Bootstrapper error source
        /// </summary>
        public const int BOOTSTRAPPER_ERROR = 6;
        /// <summary>
        /// Shutdown error source
        /// </summary>
        public const int SHUTDOWN_ERROR = 7;

        /// <summary>
        /// Exception counter
        /// </summary>
        private static volatile int _ExceptionCount = 0;
        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        public static readonly object SyncObject = new();
        /// <summary>
        /// Collected errors (synchronized using <see cref="SyncObject"/>; enable by setting <see cref="ErrorCollectingHandler(ErrorInfo)"/> as <see cref="ErrorHandler"/> or calling 
        /// <see cref="ErrorCollectingHandler(ErrorInfo)"/> from within your custom error handler)
        /// </summary>
        public static readonly Queue<ErrorInfo> Errors = new();

        /// <summary>
        /// Constructor
        /// </summary>
        static ErrorHandling() => AppDomain.CurrentDomain.FirstChanceException += (s, e) => _ExceptionCount++;

        /// <summary>
        /// Error handler (first chance before <see cref="OnError"/> event handlers)
        /// </summary>
        public static ErrorHandler_Delegate? ErrorHandler { get; set; } = DefaultErrorHandler;

        /// <summary>
        /// Break the attached debugger on error?
        /// </summary>
        public static bool DebugOnError { get; set; } = true;

        /// <summary>
        /// <see cref="Errors"/> count limit (<c>0</c> for unlimited)
        /// </summary>
        public static int ErrorCountLimit { get; set; }

        /// <summary>
        /// Exception counter (volatile value)
        /// </summary>
        public static int ExceptionCount
        {
            get => _ExceptionCount;
            set => _ExceptionCount = value;
        }

        /// <summary>
        /// Handle an error (writes to the logging, too!)
        /// </summary>
        /// <param name="ex">Exception</param>
        public static void Handle(in ErrorInfo ex)
        {
            if (DebugOnError)
                System.Diagnostics.Debugger.Break();
            bool loggerDone = false;
            try
            {
                if (Logging.Logger is not null) System.Diagnostics.Debug.WriteLine(ex);
                if (Error) Logging.WriteError(ex);
                loggerDone = true;
                ErrorHandler?.Invoke(ex);
                RaiseOnError(ex);
            }
            catch(Exception ex2)
            {
                System.Diagnostics.Debugger.Break();
                if (!loggerDone && Logging.Logger is null) System.Diagnostics.Debug.WriteLine(ex);
                string message = $"Uncatched exception during error handling{(ex.Info is null ? string.Empty : $" ({ex.Info})")}";
                System.Diagnostics.Debug.Fail(message, ex2.ToString());
                Console.Error.WriteLine($"{message}: {new AggregateException(ex.Exception, ex2)}");
            }
        }

        /// <summary>
        /// An error collecting handler which stores exceptions in <see cref="Errors"/> (may be used as default error handler)
        /// </summary>
        /// <param name="ex">Exception</param>
        public static void ErrorCollectingHandler(ErrorInfo ex)
        {
            lock (SyncObject)
            {
                for (int limit = ErrorCountLimit; limit > 0 && Errors.Count >= ErrorCountLimit; Errors.Dequeue()) ;
                Errors.Enqueue(ex);
            }
        }

        /// <summary>
        /// Default error handler
        /// </summary>
        /// <param name="info">Error information</param>
        public static void DefaultErrorHandler(ErrorInfo info)
        {
            if(info.Exception is StackInfoException ex)
            {
                Logging.WriteWarning($"Warning from source #{info.Source}: {info.Exception}");
                if (Debug)
                    Logging.WriteDebug($"Stack of {ex.StackInfo.Object.GetType()}: {ex.StackInfo.Stack}");
            }
            else
            {
                Logging.WriteError($"Error from source #{info.Source}: {info.Exception}");
            }
        }

        /// <summary>
        /// Delegate for an error handler
        /// </summary>
        /// <param name="ex">Exception</param>
        public delegate void ErrorHandler_Delegate(ErrorInfo ex);

        /// <summary>
        /// Raised ion error
        /// </summary>
        public static event ErrorHandler_Delegate? OnError;
        /// <summary>
        /// Raise the <see cref="OnError"/> event
        /// </summary>
        /// <param name="ex">Exception</param>
        private static void RaiseOnError(ErrorInfo ex) => OnError?.Invoke(ex);
    }
}

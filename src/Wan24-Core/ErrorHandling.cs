using System.Diagnostics;

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
        /// Error handler (first chance before <see cref="OnError"/> event handlers)
        /// </summary>
        public static ErrorHandler_Delegate? ErrorHandler { get; set; }

        /// <summary>
        /// Break the attached debugger on error?
        /// </summary>
        public static bool DebugOnError { get; set; } = true;

        /// <summary>
        /// <see cref="Errors"/> count limit (<c>0</c> for unlimited)
        /// </summary>
        public static int ErrorCountLimit { get; set; }

        /// <summary>
        /// Handle an error (writes to the logging, too!)
        /// </summary>
        /// <param name="ex">Exception</param>
        public static void Handle(in ErrorInfo ex)
        {
            if (DebugOnError)
                Debugger.Break();
            bool loggerDone = false;
            try
            {
                if (Logging.Logger is not null) Debug.WriteLine(ex);
                Logging.WriteError(ex);
                loggerDone = true;
                ErrorHandler?.Invoke(ex);
                RaiseOnError(ex);
            }
            catch(Exception ex2)
            {
                Debugger.Break();
                if (!loggerDone && Logging.Logger is null) Debug.WriteLine(ex);
                string message = $"Uncatched exception during error handling{(ex.Info is null ? string.Empty : $" ({ex.Info})")}";
                Debug.Fail(message, ex2.ToString());
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

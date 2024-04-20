using System.Collections.Concurrent;
using static wan24.Core.Logger;
using static wan24.Core.Logging;

namespace wan24.Core
{
    /// <summary>
    /// App shutdown
    /// </summary>
    public static class Shutdown
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        private static readonly SemaphoreSync Sync = new();

        /// <summary>
        /// Asynchronous shutdown handlers (executed in parallel before raising <see cref="OnShutdown"/>)
        /// </summary>
        public static AsyncEvent<object, EventArgs> OnShutdownAsync { get; } = new();

        /// <summary>
        /// Shutdown
        /// </summary>
        public static async Task Async()
        {
            if (DidShutdown) return;
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync().DynamicContext();
            if (DidShutdown) return;
            IsShuttingDown = true;
            try
            {
                if (Debug) WriteDebug("App shutting down");
                ConcurrentDictionary<string, Task> pendingFinalizers = await RaiseOnShutdownAsync().DynamicContext();
                if (!pendingFinalizers.IsEmpty)
                {
                    List<Exception> exceptions = [];
                    int round = 0;
                    while (!pendingFinalizers.IsEmpty)
                    {
                        round++;
                        if (Debug) WriteDebug($"Waiting for {pendingFinalizers.Count} pending finalizers (round #{round})");
                        try
                        {
                            await Task.WhenAll(pendingFinalizers.Values).DynamicContext();
                        }
                        catch (AggregateException ex)
                        {
                            WriteWarning($"{ex.InnerExceptions.Count} exceptions during shutdown handling");
                            exceptions.AddRange(ex.InnerExceptions);
                        }
                        foreach (string key in pendingFinalizers.Where(kvp => kvp.Value.IsCompleted).Select(kvp => kvp.Key))
                            pendingFinalizers.TryRemove(key, out _);
                    }
                    if (Debug) WriteDebug($"All pending shutdown finalizers are done with {exceptions.Count} exceptions");
                    if (exceptions.Count > 0)
                        ErrorHandling.Handle(new("Exceptions during shutdown handling", new AggregateException(exceptions), ErrorHandling.SHUTDOWN_ERROR));
                }
            }
            catch (Exception ex)
            {
                ErrorHandling.Handle(new("Exception during shutdown", ex, ErrorHandling.SHUTDOWN_ERROR));
            }
            finally
            {
                DidShutdown = true;
                IsShuttingDown = false;
                if (Debug) WriteDebug("App shutdown done");
            }
        }

        /// <summary>
        /// Is shutting down?
        /// </summary>
        public static bool IsShuttingDown { get; private set; }

        /// <summary>
        /// Did shutdown?
        /// </summary>
        public static bool DidShutdown { get; private set; }

        /// <summary>
        /// Delegate for an <see cref="OnShutdown"/> event handler
        /// </summary>
        /// <param name="e">Arguments</param>
        public delegate void Shutdown_Delegate(ShutdownEventArgs e);
        /// <summary>
        /// Raised on shutdown
        /// </summary>
        public static event Shutdown_Delegate? OnShutdown;
        /// <summary>
        /// Raise the <see cref="OnShutdown"/> event
        /// </summary>
        /// <returns>Pending finalizer tasks</returns>
        private static async Task<ConcurrentDictionary<string, Task>> RaiseOnShutdownAsync()
        {
            ShutdownEventArgs e = new();
            await OnShutdownAsync.Abstract.RaiseEventAsync(new(), e).DynamicContext();
            OnShutdown?.Invoke(e);
            return e.FinalizerTasks;
        }

        /// <summary>
        /// <see cref="OnShutdown"/> event arguments
        /// </summary>
        public sealed class ShutdownEventArgs : EventArgs
        {
            /// <summary>
            /// Tasks of pending finalizers
            /// </summary>
            internal readonly ConcurrentDictionary<string, Task> FinalizerTasks = [];

            /// <summary>
            /// Constructor
            /// </summary>
            public ShutdownEventArgs() : base() { }

            /// <summary>
            /// Add a finalizer task to wait for
            /// </summary>
            /// <param name="task">Finalizer task</param>
            public void AddFinalizerTask(in Task task) => FinalizerTasks[Guid.NewGuid().ToString()] = task;
        }
    }
}

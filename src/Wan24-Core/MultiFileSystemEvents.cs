using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Multi file system events
    /// </summary>
    public class MultiFileSystemEvents : HostedServiceBase
    {
        /// <summary>
        /// File system watchers
        /// </summary>
        protected readonly ConcurrentDictionary<string, FileSystemEvents> Watchers = [];

        /// <summary>
        /// Constructor
        /// </summary>
        public MultiFileSystemEvents() : base() => CanPause = true;

        /// <summary>
        /// Add a file system events watcher
        /// </summary>
        /// <param name="watcher">File system events watcher (will be disposed, if not removed before disposing!)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ID</returns>
        public virtual string Add(in FileSystemEvents watcher, in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            string res = Guid.NewGuid().ToString();
            Watchers[res] = watcher;
            if (IsPaused && !watcher.IsPaused)
            {
                if (watcher.IsRunning)
                {
                    watcher.PauseAsync(cancellationToken).Wait(CancellationToken.None);
                }
                else
                {
                    watcher.StartAsync(cancellationToken).Wait(CancellationToken.None);
                    watcher.PauseAsync(cancellationToken).Wait(CancellationToken.None);
                }
            }
            else if (IsRunning)
            {
                if (watcher.IsPaused)
                {
                    watcher.ResumeAsync(cancellationToken).Wait(CancellationToken.None);
                }
                else if (!watcher.IsRunning)
                {
                    watcher.StartAsync(cancellationToken).Wait(CancellationToken.None);
                }
            }
            watcher.OnEvents += RaiseOnEvents;
            return res;
        }

        /// <summary>
        /// Add a file system events watcher
        /// </summary>
        /// <param name="watcher">File system events watcher (will be disposed, if not removed before disposing!)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ID</returns>
        public virtual async Task<string> AddAsync(FileSystemEvents watcher, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            string res = Guid.NewGuid().ToString();
            Watchers[res] = watcher;
            if (IsPaused && !watcher.IsPaused)
            {
                if (watcher.IsRunning)
                {
                    await watcher.PauseAsync(cancellationToken).DynamicContext();
                }
                else
                {
                    await watcher.StartAsync(cancellationToken).DynamicContext();
                    await watcher.PauseAsync(cancellationToken).DynamicContext();
                }
            }
            else if (IsRunning)
            {
                if (watcher.IsPaused)
                {
                    await watcher.ResumeAsync(cancellationToken).DynamicContext();
                }
                else if (!watcher.IsRunning)
                {
                    await watcher.StartAsync(cancellationToken).DynamicContext();
                }
            }
            watcher.OnEvents += RaiseOnEvents;
            return res;
        }

        /// <summary>
        /// Remove a file system events watcher
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>Removed file system events watcher (won'tbe disposed)</returns>
        public virtual FileSystemEvents Remove(in string id)
        {
            EnsureUndisposed();
            if (!Watchers.TryRemove(id, out FileSystemEvents? res)) throw new ArgumentException("ID not found", nameof(id));
            res.OnEvents -= RaiseOnEvents;
            return res;
        }

        /// <summary>
        /// Remove a file system events watcher
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="result">Removed file system events watcher (won'tbe disposed)</param>
        /// <returns>If succeed</returns>
        public virtual bool TryRemove(in string id, [NotNullWhen(returnValue: true)] out FileSystemEvents? result)
        {
            if (!EnsureUndisposed(throwException: false) || !Watchers.TryRemove(id, out result))
            {
                result = null;
                return false;
            }
            result.OnEvents -= RaiseOnEvents;
            return true;
        }

        /// <inheritdoc/>
        protected override async Task AfterStartAsync(CancellationToken cancellationToken)
        {
            await base.AfterStartAsync(cancellationToken).DynamicContext();
            List<Task> tasks = new(Watchers.Values.Select(w => !w.IsPaused ? w.StartAsync(CancellationToken.None) : Task.CompletedTask));
            await tasks.WaitAll().WaitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task BeforeStopAsync(CancellationToken cancellationToken)
        {
            await base.BeforeStopAsync(cancellationToken).DynamicContext();
            List<Task> tasks = new(Watchers.Values.Select(w => w.StopAsync(CancellationToken.None)));
            await tasks.WaitAll().WaitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task AfterPauseAsync(CancellationToken cancellationToken)
        {
            await base.AfterPauseAsync(cancellationToken).DynamicContext();
            List<Task> tasks = new(Watchers.Values.Select(w => w.IsRunning && !w.IsPaused ? w.PauseAsync(CancellationToken.None) : Task.CompletedTask));
            await tasks.WaitAll().WaitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task AfterResumeAsync(CancellationToken cancellationToken)
        {
            await base.AfterResumeAsync(cancellationToken).DynamicContext();
            List<Task> tasks = new(Watchers.Values.Select(w => w.IsPaused ? w.ResumeAsync(CancellationToken.None) : Task.CompletedTask));
            await tasks.WaitAll().WaitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync() => await CancelToken;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Watchers.Values.DisposeAll();
            foreach (FileSystemEvents watcher in Watchers.Values) watcher.OnEvents -= RaiseOnEvents;
            Watchers.Clear();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            await Watchers.Values.DisposeAllAsync().DynamicContext();
            foreach (FileSystemEvents watcher in Watchers.Values) watcher.OnEvents -= RaiseOnEvents;
            Watchers.Clear();
        }

        /// <summary>
        /// Delegate for an event hander
        /// </summary>
        /// <param name="events">Events</param>
        /// <param name="e">Arguments</param>
        public delegate void MultiFileSystemEvents_Delegate(MultiFileSystemEvents events, MultiFileSystemEventsArgs e);
        /// <summary>
        /// Raised on event
        /// </summary>
        public event MultiFileSystemEvents_Delegate? OnEvents;
        /// <summary>
        /// Raise the <see cref="OnEvents"/> event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected virtual void RaiseOnEvents(FileSystemEvents sender, FileSystemEvents.FileSystemEventsArgs e)
        {
            if (Watchers.Where(kvp => kvp.Value == sender).Select(kvp => kvp.Key).FirstOrDefault() is not string id) return;
            OnEvents?.Invoke(this, new(id, e.Arguments, e.Raised));
        }

        /// <summary>
        /// File system events <see cref="OnEvents"/> arguments
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="id">Watcher ID</param>
        /// <param name="arguments">File system event arguments</param>
        /// <param name="raised">Raised time</param>
        public class MultiFileSystemEventsArgs(in string id, in IReadOnlyList<FileSystemEventArgs> arguments, in DateTime raised)
            : FileSystemEvents.FileSystemEventsArgs(arguments, raised)
        {
            /// <summary>
            /// Watcher ID
            /// </summary>
            public string Id { get; } = id;
        }
    }
}

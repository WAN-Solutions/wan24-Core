using System.Collections.Immutable;

namespace wan24.Core
{
    /// <summary>
    /// App configuration loader
    /// </summary>
    /// <typeparam name="T">App configuration type</typeparam>
    public class AppConfigLoader<T> : HostedServiceBase where T : class, IAppConfig
    {
        /// <summary>
        /// Filesystem watcher
        /// </summary>
        protected readonly MultiFileSystemEvents FsWatch;
        /// <summary>
        /// Update event (raised when an updated configuration should be reloaded)
        /// </summary>
        protected readonly ResetEvent UpdateEvent = new();
        /// <summary>
        /// Config event (raised when <see cref="Config"/> is not <see langword="null"/>)
        /// </summary>
        protected readonly ResetEvent ConfigEvent = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="appConfigFile">App configuration file (will be watched for changes)</param>
        /// <param name="autoApply">If to auto-apply changed configuration</param>
        /// <param name="reloadThrottle">Reload throttling time in ms</param>
        /// <param name="watchedFiles">Additionally watched files (on any change the configuration will be reloaded)</param>
        public AppConfigLoader(in string appConfigFile, in bool autoApply = true, in int reloadThrottle = 300, params string[] watchedFiles) : base()
        {
            AppConfigFile = appConfigFile;
            WatchedFiles = [.. watchedFiles];
            AutoApply = autoApply;
            FsWatch = new(reloadThrottle);
            FsWatch.Add(new(Path.GetDirectoryName(AppConfigFile) ?? Path.GetFullPath("./"), Path.GetFileName(AppConfigFile), NotifyFilters.LastWrite));
            foreach (string fn in watchedFiles)
                FsWatch.Add(new(Path.GetDirectoryName(fn) ?? Path.GetFullPath("./"), Path.GetFileName(fn), NotifyFilters.LastWrite));
            FsWatch.OnEvents += HandleFsEvents;
        }

        /// <summary>
        /// App configuration file
        /// </summary>
        public string AppConfigFile { get; }

        /// <summary>
        /// Watched files
        /// </summary>
        public ImmutableArray<string> WatchedFiles { get; }

        /// <summary>
        /// If to auto-apply changed configuration
        /// </summary>
        public bool AutoApply { get; }

        /// <summary>
        /// Loaded configuration
        /// </summary>
        public T? Config { get; protected set; }

        /// <summary>
        /// Reload the app configuration
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task ReloadAppConfig(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await ReloadAppConfigInt(cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Wait for a configuration in <see cref="Config"/> to become available
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>App configuration</returns>
        public virtual async Task<T> WaitForConfig(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            T? res;
            while ((res = Config) is null) await ConfigEvent.WaitAsync(cancellationToken).DynamicContext();
            return res;
        }

        /// <summary>
        /// Handle filesystem events
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected virtual void HandleFsEvents(MultiFileSystemEvents sender, MultiFileSystemEvents.MultiFileSystemEventsArgs e)
        {
            if (!IsDisposing)
                try
                {
                    UpdateEvent.Set();
                }
                catch
                {
                }
        }

        /// <summary>
        /// Reload the app configuration
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task ReloadAppConfigInt(CancellationToken cancellationToken)
        {
            Config = await AppConfig.LoadIfExistsAsync<T>(AppConfigFile, apply: false, cancellationToken).DynamicContext();
            if (AutoApply)
            {
                AppConfigBase.Applied = null;
                if (Config is T config) await config.ApplyAsync(CancelToken).DynamicContext();
            }
            RaiseOnUpdate();
            if (Config is null)
            {
                await ConfigEvent.ResetAsync(CancellationToken.None).DynamicContext();
            }
            else
            {
                await ConfigEvent.SetAsync(CancellationToken.None).DynamicContext();
            }
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync()
        {
            await ReloadAppConfigInt(CancelToken).DynamicContext();
            while (true)
            {
                await UpdateEvent.WaitAndResetAsync(CancelToken).DynamicContext();
                await ReloadAppConfigInt(CancelToken).DynamicContext();
            }
        }

        /// <inheritdoc/>
        protected override async Task StartingAsync(CancellationToken cancellationToken)
        {
            await base.StartingAsync(cancellationToken).DynamicContext();
            await FsWatch.StartAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task StoppingAsync(CancellationToken cancellationToken)
        {
            await FsWatch.StopAsync(cancellationToken).DynamicContext();
            await base.StoppingAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            FsWatch.Dispose();
            UpdateEvent.Dispose();
            ConfigEvent.Dispose();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await FsWatch.DisposeAsync().DynamicContext();
            await UpdateEvent.DisposeAsync().DynamicContext();
            await ConfigEvent.DisposeAsync().DynamicContext();
            await base.DisposeCore().DynamicContext();
        }

        /// <summary>
        /// Delegate for an event handler
        /// </summary>
        /// <param name="loader">Loader</param>
        /// <param name="e">Arguments</param>
        public delegate void AppConfigLoaderEvent_Delegate(AppConfigLoader<T> loader, EventArgs e);
        /// <summary>
        /// Raised on configuration update (afer the <see cref="Config"/> was reloaded)
        /// </summary>
        public event AppConfigLoaderEvent_Delegate? OnUpdate;
        /// <summary>
        /// Raise the <see cref="OnUpdate"/>
        /// </summary>
        protected virtual void RaiseOnUpdate() => OnUpdate?.Invoke(this, EventArgs.Empty);
    }
}

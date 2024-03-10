using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace wan24.Core
{
    /// <summary>
    /// App configuration
    /// </summary>
    public class AppConfig : AppConfigBase
    {
        /// <summary>
        /// Default filename
        /// </summary>
        public const string DEAULT_FILENAME = "config.json";

        /// <summary>
        /// Constructor
        /// </summary>
        public AppConfig() : base() => SetApplied = true;

        /// <summary>
        /// Logging
        /// </summary>
        public LoggerConfiguration[] Logging { get; set; } = [];

        /// <summary>
        /// Default log level
        /// </summary>
        public LogLevel? LogLevel { get; set; }

        /// <summary>
        /// App start command
        /// </summary>
        public string? AppCommand { get; set; }

        /// <summary>
        /// Absolute temporary folder
        /// </summary>
        [StringLength(short.MaxValue)]
        public string? TempFolder { get; set; }

        /// <summary>
        /// Unix create file mode
        /// </summary>
        public UnixFileMode? CreateFileMode { get; set; }

        /// <summary>
        /// Unix create folder mode
        /// </summary>
        public UnixFileMode? CreateFolderMode { get; set; }

        /// <summary>
        /// CLI configuration (applied using <see cref="CliConfig"/>; the key is the public static property name including namespace and type name, the value the values (or <see langword="null"/> 
        /// in case of a boolean))
        /// </summary>
        public Dictionary<string, string[]?> Properties { get; set; } = [];

        /// <summary>
        /// Default CLI arguments
        /// </summary>
        public string[]? DefaultCliArguments { get; set; }

        /// <summary>
        /// Value for <see cref="FsHelper.SearchFolders"/>
        /// </summary>
        public string[]? SearchFolders { get; set; }

        /// <summary>
        /// Apply a CLI configuration from CLI arguments, too?
        /// </summary>
        [JsonIgnore]
        public virtual bool ApplyCliArguments => true;

        /// <summary>
        /// Call <see cref="Bootstrap.Async(System.Reflection.Assembly?, CancellationToken)"/>?
        /// </summary>
        [JsonIgnore]
        public virtual bool Bootstrap => true;

        /// <inheritdoc/>
        public override void Apply()
        {
            if (SetApplied)
            {
                if (Applied is not null) throw new InvalidOperationException();
                Applied = this;
            }
            if (LogLevel.HasValue) Settings.LogLevel = LogLevel.Value;
            foreach (LoggerConfiguration config in Logging)
                if (config is IAppConfig sub)
                {
                    sub.Apply();
                }
                else
                {
                    ApplyLogger(config);
                }
            if (TempFolder is not null) Settings.CustomTempFolder = TempFolder;
            if (CreateFileMode.HasValue) Settings.CreateFileMode = CreateFileMode.Value;
            if (CreateFolderMode.HasValue) Settings.CreateFolderMode = CreateFolderMode.Value;
            if (AppCommand is not null) ENV.AppCommand = AppCommand;
            if (DefaultCliArguments is not null) ENV._CliArguments = ENV._CliArguments.Length == 0
                    ? [.. DefaultCliArguments, .. ENV._CliArguments]
                    : [ENV._CliArguments[0], .. DefaultCliArguments, .. ENV._CliArguments.AsSpan(1)];
            List<string> args = new(Properties.Count);
            foreach (var kvp in Properties)
                if (kvp.Value is null)
                {
                    args.Add($"-{kvp.Key}");
                }
                else
                {
                    foreach (string value in kvp.Value)
                    {
                        args.Add($"--{kvp.Key}");
                        args.Add(value);
                    }
                }
            if (args.Count > 0) CliConfig.Apply(new(args));
            if (SearchFolders is not null)
                lock (FsHelper.SyncObject)
                {
                    FsHelper.SearchFolders.Clear();
                    FsHelper.SearchFolders.AddRange(SearchFolders);
                }
            ApplyProperties(afterBootstrap: false);
            if (ApplyCliArguments) CliConfig.Apply();
            if (Bootstrap) Core.Bootstrap.Async().Wait();
            ApplyProperties(afterBootstrap: true);
        }

        /// <inheritdoc/>
        public override async Task ApplyAsync(CancellationToken cancellationToken = default)
        {
            if (SetApplied)
            {
                if (Applied is not null) throw new InvalidOperationException();
                Applied = this;
            }
            if (LogLevel.HasValue) Settings.LogLevel = LogLevel.Value;
            foreach (LoggerConfiguration config in Logging)
                if (config is IAppConfig sub)
                {
                    await sub.ApplyAsync(cancellationToken).DynamicContext();
                }
                else
                {
                    ApplyLogger(config);
                }
            if (TempFolder is not null) Settings.CustomTempFolder = TempFolder;
            if (CreateFileMode.HasValue) Settings.CreateFileMode = CreateFileMode.Value;
            if (CreateFolderMode.HasValue) Settings.CreateFolderMode = CreateFolderMode.Value;
            if (AppCommand is not null) ENV.AppCommand = AppCommand;
            if (DefaultCliArguments is not null) ENV._CliArguments = ENV._CliArguments.Length == 0
                    ? [.. DefaultCliArguments, .. ENV._CliArguments]
                    : [ENV._CliArguments[0], .. DefaultCliArguments, .. ENV._CliArguments.AsSpan(1)];
            List<string> args = new(Properties.Count);
            foreach (var kvp in Properties)
                if (kvp.Value is null)
                {
                    args.Add($"-{kvp.Key}");
                }
                else
                {
                    foreach (string value in kvp.Value)
                    {
                        args.Add($"--{kvp.Key}");
                        args.Add(value);
                    }
                }
            if (args.Count > 0) CliConfig.Apply(new(args));
            if (SearchFolders is not null)
                lock (FsHelper.SyncObject)
                {
                    FsHelper.SearchFolders.Clear();
                    FsHelper.SearchFolders.AddRange(SearchFolders);
                }
            await ApplyPropertiesAsync(afterBootstrap: false, cancellationToken).DynamicContext();
            if (ApplyCliArguments) CliConfig.Apply();
            if (Bootstrap) await Core.Bootstrap.Async(cancellationToken: cancellationToken).DynamicContext();
            await ApplyPropertiesAsync(afterBootstrap: false, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Apply a logger configuration
        /// </summary>
        /// <typeparam name="T">Logger configuration type</typeparam>
        /// <param name="config">Configuration</param>
        protected virtual void ApplyLogger<T>(T config) where T : LoggerConfiguration => config.Apply();

        /// <summary>
        /// Load an app config from a JSON file
        /// </summary>
        /// <param name="fileName">Filename (may be without a path)</param>
        /// <param name="apply">Apply?</param>
        /// <returns>App config</returns>
        public static AppConfig? LoadIfExists(string fileName = DEAULT_FILENAME, in bool apply = true)
        {

            if (Path.GetFileName(fileName).Equals(fileName)) fileName = Path.Combine(ENV.AppFolder, fileName);
            return File.Exists(fileName) ? Load(fileName, apply) : null;
        }

        /// <summary>
        /// Load an app config from a JSON file
        /// </summary>
        /// <typeparam name="T">App config type</typeparam>
        /// <param name="fileName">Filename (may be without a path)</param>
        /// <param name="apply">Apply?</param>
        /// <returns>App config</returns>
        public static T? LoadIfExists<T>(string fileName = DEAULT_FILENAME, in bool apply = true) where T : class, IAppConfig
        {
            if (Path.GetFileName(fileName).Equals(fileName)) fileName = Path.Combine(ENV.AppFolder, fileName);
            return File.Exists(fileName) ? Load<T>(fileName, apply) : null;
        }

        /// <summary>
        /// Load an app config from a JSON file
        /// </summary>
        /// <param name="fileName">Filename (may be without a path)</param>
        /// <param name="apply">Apply?</param>
        /// <returns>App config</returns>
        public static AppConfig Load(string fileName = DEAULT_FILENAME, in bool apply = true) => Load<AppConfig>(fileName, apply);

        /// <summary>
        /// Load an app config from a JSON file
        /// </summary>
        /// <typeparam name="T">App config type</typeparam>
        /// <param name="fileName">Filename (may be without a path)</param>
        /// <param name="apply">Apply?</param>
        /// <returns>App config</returns>
        public static T Load<T>(string fileName = DEAULT_FILENAME, in bool apply = true) where T : class, IAppConfig
        {
            if (Path.GetFileName(fileName).Equals(fileName)) fileName = Path.Combine(ENV.AppFolder, fileName);
            using FileStream fs = FsHelper.CreateFileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using StreamReader reader = new(fs);
            T res = JsonHelper.Decode<T>(reader.ReadToEnd()) ?? throw new InvalidDataException($"Failed to decode {typeof(T)}");
            if (apply) res.Apply();
            return res;
        }

        /// <summary>
        /// Load an app config from a JSON file
        /// </summary>
        /// <param name="fileName">Filename (may be without a path)</param>
        /// <param name="apply">Apply?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>App config</returns>
        public static async Task<AppConfig?> LoadIfExistsAsync(string fileName = DEAULT_FILENAME, bool apply = true, CancellationToken cancellationToken = default)
        {
            if (Path.GetFileName(fileName).Equals(fileName)) fileName = Path.Combine(ENV.AppFolder, fileName);
            return File.Exists(fileName) ? await LoadAsync(fileName, apply, cancellationToken).DynamicContext() : null;
        }

        /// <summary>
        /// Load an app config from a JSON file
        /// </summary>
        /// <typeparam name="T">App config type</typeparam>
        /// <param name="fileName">Filename (may be without a path)</param>
        /// <param name="apply">Apply?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>App config</returns>
        public static async Task<T?> LoadIfExistsAsync<T>(string fileName = DEAULT_FILENAME, bool apply = true, CancellationToken cancellationToken = default) where T : class, IAppConfig
        {
            if (Path.GetFileName(fileName).Equals(fileName)) fileName = Path.Combine(ENV.AppFolder, fileName);
            return File.Exists(fileName) ? await LoadAsync<T>(fileName, apply, cancellationToken).DynamicContext() : null;
        }

        /// <summary>
        /// Load an app config from a JSON file
        /// </summary>
        /// <param name="fileName">Filename (may be without a path)</param>
        /// <param name="apply">Apply?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>App config</returns>
        public static Task<AppConfig> LoadAsync(string fileName = DEAULT_FILENAME, bool apply = true, CancellationToken cancellationToken = default)
            => LoadAsync<AppConfig>(fileName, apply, cancellationToken);

        /// <summary>
        /// Load an app config from a JSON file
        /// </summary>
        /// <typeparam name="T">App config type</typeparam>
        /// <param name="fileName">Filename (may be without a path)</param>
        /// <param name="apply">Apply?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>App config</returns>
        public static async Task<T> LoadAsync<T>(string fileName = DEAULT_FILENAME, bool apply = true, CancellationToken cancellationToken = default) where T : class, IAppConfig
        {
            if (Path.GetFileName(fileName).Equals(fileName)) fileName = Path.Combine(ENV.AppFolder, fileName);
            T res;
            FileStream fs = FsHelper.CreateFileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            await using (fs.DynamicContext())
                res = await JsonHelper.DecodeAsync<T>(fs, cancellationToken).DynamicContext() ?? throw new InvalidDataException($"Failed to decode {typeof(T)}");
            if (apply) await res.ApplyAsync(cancellationToken).DynamicContext();
            return res;
        }

        /// <summary>
        /// Logger configuration
        /// </summary>
        public class LoggerConfiguration
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public LoggerConfiguration() { }

            /// <summary>
            /// Logger type name (optional including namespace; not required, if <see cref="FileName"/> was set)
            /// </summary>
            [StringLength(byte.MaxValue)]
            public string? LoggerType { get; set; }

            /// <summary>
            /// Filename (using <see cref="FileLogger"/>; ignored, if <see cref="LoggerType"/> was set)
            /// </summary>
            [StringLength(short.MaxValue)]
            public string? FileName { get; set; }

            /// <summary>
            /// Log level
            /// </summary>
            public LogLevel? LogLevel { get; set; }

            /// <summary>
            /// Apply the logger configuration
            /// </summary>
            public virtual void Apply()
            {
                if (FileName is null)
                {
                    CliConfig.LoggerType = LoggerType ?? throw new InvalidDataException($"{nameof(LoggerType)} required");
                }
                else
                {
                    CliConfig.LogFile = FileName;
                }
                if (LogLevel.HasValue) CliConfig.LogLevel = LogLevel.Value;
            }
        }
    }
}

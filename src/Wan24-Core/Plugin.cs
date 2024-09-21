using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Plugin
    /// </summary>
    public class Plugin : DisposableBase
    {
        /// <summary>
        /// Loaded plugins
        /// </summary>
        protected static readonly ConcurrentDictionary<string, Plugin> Loaded = [];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembly">Assembly path</param>
        /// <param name="name">Name</param>
        protected Plugin(in string assembly, in string? name = null) : base(asyncDisposing: false)
        {
            Assembly = assembly;
            Context = new(new(Path.GetDirectoryName(assembly) ?? throw new ArgumentException("Failed to get directory name", nameof(assembly))), name);
            Context.LoadFromAssemblyPath(assembly);
            if (!Context.Plugins.Any())
            {
                Dispose();
                throw new ArgumentException("The assembly doesn't contain any plugins", nameof(assembly));
            }
            Loaded[assembly] = this;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected Plugin() : base(asyncDisposing: false)
        {
            Assembly = null!;
            Context = null!;
        }

        /// <summary>
        /// Assembly path
        /// </summary>
        public string Assembly { get; }

        /// <summary>
        /// Context
        /// </summary>
        public PluginLoadContext Context { get; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Loaded.TryRemove(Assembly, out _);
            Context?.Unload();
        }

        /// <summary>
        /// Determine if an assembly contains plugins
        /// </summary>
        /// <param name="assembly">Assembly path</param>
        /// <returns>If plugins are contained</returns>
        public static bool ContainsPlugins(in string assembly)
        {
            PluginLoadContext context = new(new(Path.GetDirectoryName(assembly) ?? throw new ArgumentException("Failed to get directory name", nameof(assembly))));
            try
            {
                context.LoadFromAssemblyPath(assembly);
                return context.Plugins.Any();
            }
            catch (Exception ex)
            {
                ErrorHandling.Handle(new($"Failed to load plugins from {assembly.MaxLength(byte.MaxValue).ToQuotedLiteral()}", ex, tag: assembly));
                return false;
            }
            finally
            {
                context.Unload();
            }
        }

        /// <summary>
        /// Get or load a plugin
        /// </summary>
        /// <param name="assembly">Assembly path to load the plugin from</param>
        /// <param name="name">Name</param>
        /// <returns>Plugin</returns>
        public static Plugin Load(string assembly, string? name = null) => Loaded.GetOrAdd(assembly, key => new(assembly, name));

        /// <summary>
        /// Get a plugin
        /// </summary>
        /// <param name="assembly">Assembly path the plugin was loaded from</param>
        /// <returns>Plugin</returns>
        public static Plugin? Get(string assembly)
            => Loaded.TryGetValue(assembly, out Plugin? res)
                ? res
                : null;
    }
}

using System.Reflection;
using System.Runtime.Loader;

namespace wan24.Core
{
    /// <summary>
    /// Plugin load context
    /// </summary>
    public class PluginLoadContext : AssemblyLoadContext
    {
        /// <summary>
        /// Assembly resolver
        /// </summary>
        protected readonly AssemblyDependencyResolver Resolver;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resolver">Assembly resolver</param>
        /// <param name="name">Name</param>
        public PluginLoadContext(in AssemblyDependencyResolver resolver, in string? name = null) : base(name, isCollectible: true)
        {
            Resolver = resolver;
            Unloading += HandleUnloading;
        }

        /// <summary>
        /// Plugins
        /// </summary>
        public IEnumerable<Type> Plugins => from ass in Assemblies
                                            from type in ass.GetTypes()
                                            where type.CanConstruct() &&
                                              typeof(IPlugin).IsAssignableFrom(type)
                                            select type;

        /// <inheritdoc/>
        protected override Assembly? Load(AssemblyName assemblyName)
            => Resolver.ResolveAssemblyToPath(assemblyName) is string fn
                ? LoadFromAssemblyPath(fn)
                : null;

        /// <inheritdoc/>
        protected override nint LoadUnmanagedDll(string unmanagedDllName)
            => Resolver.ResolveUnmanagedDllToPath(unmanagedDllName) is string fn
                ? LoadUnmanagedDllFromPath(fn)
                : IntPtr.Zero;

        /// <summary>
        /// Handle unloading
        /// </summary>
        /// <param name="context">Context</param>
        protected virtual void HandleUnloading(AssemblyLoadContext context)
        {
            Unloading -= HandleUnloading;
            TypeHelper.Instance.RemoveLoadContext(this);
        }
    }
}

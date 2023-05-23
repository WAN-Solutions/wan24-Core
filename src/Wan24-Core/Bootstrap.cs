using System.Data;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Bootstrapping
    /// </summary>
    public static class Bootstrap
    {
        /// <summary>
        /// Booted assemblies
        /// </summary>
        private static readonly HashSet<Assembly> BootedAssemblies = new();
        /// <summary>
        /// Thread synchronization
        /// </summary>
        private static readonly SemaphoreSlim Sync = new(1, 1);

        /// <summary>
        /// Asynchronous bootstrapper (after running bootstrapper methods)
        /// </summary>
        public static HashSet<BootStrapAsync_Delegate> AsyncBootstrapper { get; } = new();

        /// <summary>
        /// Scan assemblies for classes with a <see cref="BootstrapperAttribute"/>?
        /// </summary>
        public static bool FindClasses { get; set; }

        /// <summary>
        /// Scan assemblies for methods with a <see cref="BootstrapperAttribute"/>?
        /// </summary>
        public static bool FindMethods { get; set; }

        /// <summary>
        /// Did boot?
        /// </summary>
        public static bool DidBoot { get; private set; }

        /// <summary>
        /// Is booting?
        /// </summary>
        public static bool IsBooting { get; private set; }

        /// <summary>
        /// Bootstrap
        /// </summary>
        /// <param name="startAssembly">Start assembly</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="BootstrapperException">Called twice (maybe recursive)</exception>
        /// <exception cref="InvalidProgramException">A bootstrapper wasn't found</exception>
        public static async Task Async(Assembly? startAssembly = null, CancellationToken cancellationToken = default)
        {
            await Sync.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                if (DidBoot) throw new BootstrapperException("Did boot already");
                if (IsBooting) throw new BootstrapperException("Booting recursion");
                IsBooting = true;
            }
            finally
            {
                Sync.Release();
            }
            await Task.Yield();
            DiHelper.ObjectFactories[typeof(CancellationToken)] = delegate (Type t, out object? obj)
            {
                obj = cancellationToken;
                return true;
            };
            try
            {
                if (startAssembly != null) TypeHelper.Instance.ScanAssemblies(startAssembly);
                // Fixed type and method
                IEnumerable<MethodInfo> methods = from ass in TypeHelper.Instance.Assemblies
                                                  where ass.GetCustomAttribute<BootstrapperAttribute>() is BootstrapperAttribute attr &&
                                                     attr.Type != null &&
                                                     attr.Method != null
                                                  select ass.GetCustomAttribute<BootstrapperAttribute>()!.Type!.GetMethod(
                                                      ass.GetCustomAttribute<BootstrapperAttribute>()!.Method!,
                                                      BindingFlags.Static | BindingFlags.Public
                                                      )
                    ?? throw new InvalidProgramException($"Bootstrapper method {ass.GetCustomAttribute<BootstrapperAttribute>()!.Type}.{ass.GetCustomAttribute<BootstrapperAttribute>()!.Method} not found");
                // Fixed type
                if (FindClasses)
                    methods = methods.Concat(from ass in TypeHelper.Instance.Assemblies
                                             where ass.GetCustomAttribute<BootstrapperAttribute>() is BootstrapperAttribute attr &&
                                                attr.Type != null &&
                                                attr.Method == null &&
                                                attr.ScanClasses
                                             from mi in ass.GetCustomAttribute<BootstrapperAttribute>()!.Type!.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                             where !mi.IsGenericMethod && mi.GetCustomAttribute<BootstrapperAttribute>() != null
                                             select mi);
                // Find types and methods
                if (FindMethods)
                    methods = methods.Concat(from ass in TypeHelper.Instance.Assemblies
                                             where ass.GetCustomAttribute<BootstrapperAttribute>() is BootstrapperAttribute attr &&
                                               attr.Type == null &&
                                               attr.Method == null &&
                                               attr.ScanMethods
                                             from type in ass.GetTypes()
                                             where type.IsPublic &&
                                              !type.IsGenericTypeDefinition &&
                                              type.GetCustomAttribute<BootstrapperAttribute>() != null
                                             from mi in type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                             where !mi.IsGenericMethod &&
                                              mi.GetCustomAttribute<BootstrapperAttribute>() != null
                                             select mi);
                // Run the bootstrapper methods
                foreach (MethodInfo mi in methods.OrderByDescending(mi => mi.DeclaringType!.Assembly.GetCustomAttribute<BootstrapperAttribute>()!.Priority)
                    .ThenByDescending(mi => mi.DeclaringType!.GetCustomAttribute<BootstrapperAttribute>()?.Priority ?? 0)
                    .ThenByDescending(mi => mi.GetCustomAttribute<BootstrapperAttribute>()?.Priority ?? 0)
                    .ThenBy(mi => mi.Name))
                {
                    Logging.WriteDebug($"Calling bootstrapper {mi.DeclaringType}.{mi.Name}");
                    if (mi.DeclaringType != null)
                    {
                        await Sync.WaitAsync(cancellationToken).DynamicContext();
                        try
                        {
                            BootedAssemblies.Add(mi.DeclaringType.Assembly);
                        }
                        finally
                        {
                            Sync.Release();
                        }
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                    if (mi.ReturnType != typeof(void) && (typeof(Task).IsAssignableFrom(mi.ReturnType) || typeof(ValueTask).IsAssignableFrom(mi.ReturnType)))
                    {
                        await mi.InvokeAutoAsync(obj: null).DynamicContext();
                    }
                    else
                    {
                        mi.InvokeAuto(obj: null);
                    }
                }
                // Raise the events
                foreach (BootStrapAsync_Delegate bootstrapper in AsyncBootstrapper.ToArray())
                {
                    await bootstrapper(cancellationToken).DynamicContext();
                    cancellationToken.ThrowIfCancellationRequested();
                }
                OnBootstrap?.Invoke();
                AsyncBootstrapper.Clear();
            }
            finally
            {
                DiHelper.ObjectFactories.Remove(typeof(CancellationToken), out _);
                DidBoot = true;
                IsBooting = false;
            }
        }

        /// <summary>
        /// Try booting asynchronous (will bool, if not yet booted)
        /// </summary>
        /// <param name="startAssembly">Start assembly</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Did bootstrap?</returns>
        public static async Task<bool> TryAsync(Assembly? startAssembly = null, CancellationToken cancellationToken = default)
        {
            await Sync.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                if (IsBooting || DidBoot) return false;
            }
            finally
            {
                Sync.Release();
            }
            try
            {
                await Async(startAssembly, cancellationToken).DynamicContext();
                return true;
            }
            catch (BootstrapperException)
            {
                return false;
            }
        }

        /// <summary>
        /// Bootstrap an (additionally loaded) assembly
        /// </summary>
        /// <param name="assembly">Assembly</param>
        /// <param name="findClasses">Scan assembly for classes with a <see cref="BootstrapperAttribute"/>?</param>
        /// <param name="findMethods">Scan assembly for methods with a <see cref="BootstrapperAttribute"/>?</param>
        /// <param name="cancellationToken">Cancellation token (won't be available with DI)</param>
        public static async Task AssemblyAsync(Assembly assembly, bool findClasses = true, bool findMethods = true, CancellationToken cancellationToken = default)
        {
            await Sync.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                if (!BootedAssemblies.Add(assembly)) return;
            }
            finally
            {
                Sync.Release();
            }
            Logging.WriteDebug($"Single bootstrapping of assembly {assembly.GetName().FullName}");
            if (!DidBoot && !IsBooting) await Async(cancellationToken: cancellationToken).DynamicContext();
            TypeHelper.Instance.ScanAssemblies(assembly);
            // Fixed type and method
            IEnumerable<MethodInfo> methods = assembly.GetCustomAttribute<BootstrapperAttribute>() is BootstrapperAttribute attr &&
                attr.Type != null &&
                attr.Method != null
                ? new MethodInfo[]
                {
                        assembly.GetCustomAttribute<BootstrapperAttribute>()!.Type!.GetMethod(
                                                      assembly.GetCustomAttribute<BootstrapperAttribute>()!.Method!,
                                                      BindingFlags.Static | BindingFlags.Public
                                                      )
                        ?? throw new InvalidProgramException($"Bootstrapper method {assembly.GetCustomAttribute<BootstrapperAttribute>()!.Type}.{assembly.GetCustomAttribute<BootstrapperAttribute>()!.Method} not found")
                }
                : Array.Empty<MethodInfo>();
            // Fixed type
            if (findClasses)
                methods = methods.Concat(from ass in new Assembly[] { assembly }
                                         where ass.GetCustomAttribute<BootstrapperAttribute>() is BootstrapperAttribute attr &&
                                            attr.Type != null &&
                                            attr.Method == null &&
                                            attr.ScanClasses
                                         from mi in ass.GetCustomAttribute<BootstrapperAttribute>()!.Type!.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                         where !mi.IsGenericMethod && mi.GetCustomAttribute<BootstrapperAttribute>() != null
                                         select mi);
            // Find types and methods
            if (findMethods)
                methods = methods.Concat(from ass in new Assembly[] { assembly }
                                         where ass.GetCustomAttribute<BootstrapperAttribute>() is BootstrapperAttribute attr &&
                                           attr.Type == null &&
                                           attr.Method == null &&
                                           attr.ScanMethods
                                         from type in ass.GetTypes()
                                         where type.IsPublic &&
                                          !type.IsGenericTypeDefinition &&
                                          type.GetCustomAttribute<BootstrapperAttribute>() != null
                                         from mi in type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                         where !mi.IsGenericMethod &&
                                          mi.GetCustomAttribute<BootstrapperAttribute>() != null
                                         select mi);
            // Run the bootstrapper methods
            foreach (MethodInfo mi in methods.OrderByDescending(mi => mi.DeclaringType!.Assembly.GetCustomAttribute<BootstrapperAttribute>()!.Priority)
                .ThenByDescending(mi => mi.DeclaringType!.GetCustomAttribute<BootstrapperAttribute>()?.Priority ?? 0)
                .ThenByDescending(mi => mi.GetCustomAttribute<BootstrapperAttribute>()?.Priority ?? 0)
                .ThenBy(mi => mi.Name))
            {
                Logging.WriteDebug($"Calling bootstrapper {mi.DeclaringType}.{mi.Name}");
                cancellationToken.ThrowIfCancellationRequested();
                if (mi.ReturnType != typeof(void) && (typeof(Task).IsAssignableFrom(mi.ReturnType) || typeof(ValueTask).IsAssignableFrom(mi.ReturnType)))
                {
                    await mi.InvokeAutoAsync(obj: null, cancellationToken).DynamicContext();
                }
                else
                {
                    mi.InvokeAuto(obj: null);
                }
            }
        }

        /// <summary>
        /// Delegate for the <see cref="OnBootstrap"/> event
        /// </summary>
        public delegate void Bootstrap_Delegate();
        /// <summary>
        /// Raised when bootstrapping (after running bootstrapper methods and the asynchronous bootstrapper delegates)
        /// </summary>
        public static event Bootstrap_Delegate? OnBootstrap;

        /// <summary>
        /// Delegate for asynchronous bootstrapper
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public delegate Task BootStrapAsync_Delegate(CancellationToken cancellationToken);
    }
}

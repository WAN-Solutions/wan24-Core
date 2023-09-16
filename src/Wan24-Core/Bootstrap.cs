using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

//TODO Use ArrayIndexOutOfBoundException

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
        private static readonly HashSet<int> BootedAssemblies = new();
        /// <summary>
        /// Thread synchronization
        /// </summary>
        private static readonly SemaphoreSync Sync = new();

        /// <summary>
        /// Static constructor
        /// </summary>
        static Bootstrap() => AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                ErrorHandling.Handle(
                    e.ExceptionObject is Exception ex 
                        ? new($"Catched unhandled exception from the current app domain (will terminate: {e.IsTerminating})", ex, ErrorHandling.UNHANDLED_EXCEPTION) 
                        : new($"Catched unhandled exception without exception object from the current app domain (will terminate: {e.IsTerminating})", new Exception(), ErrorHandling.UNHANDLED_EXCEPTION)
                    );
            };

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
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext())
            {
                if (DidBoot) throw new BootstrapperException("Did boot already");
                if (IsBooting) throw new BootstrapperException("Booting recursion");
                IsBooting = true;
            }
            bool getCancellationToken(Type type, [NotNullWhen(returnValue: true)] out object? obj)
            {
                obj = cancellationToken;
                return true;
            }
            DiHelper.ObjectFactories[typeof(CancellationToken)] = getCancellationToken;
            try
            {
                if (startAssembly is not null) TypeHelper.Instance.ScanAssemblies(startAssembly);
                // Fixed type and method
                IEnumerable<MethodInfo> methods = from ass in TypeHelper.Instance.Assemblies
                                                  where ass.GetCustomAttributeCached<BootstrapperAttribute>() is BootstrapperAttribute attr &&
                                                     attr.Type is not null &&
                                                     attr.Method is not null
                                                  select ass.GetCustomAttributeCached<BootstrapperAttribute>()!.Type!.GetMethodCached(
                                                      ass.GetCustomAttributeCached<BootstrapperAttribute>()!.Method!,
                                                      BindingFlags.Static | BindingFlags.Public
                                                      )
                    ?? throw new InvalidProgramException($"Bootstrapper method {ass.GetCustomAttributeCached<BootstrapperAttribute>()!.Type}.{ass.GetCustomAttributeCached<BootstrapperAttribute>()!.Method} not found");
                // Fixed type
                if (FindClasses)
                    methods = methods.Concat(from ass in TypeHelper.Instance.Assemblies
                                             where ass.GetCustomAttributeCached<BootstrapperAttribute>() is BootstrapperAttribute attr &&
                                                attr.Type is not null &&
                                                attr.Method is null &&
                                                attr.ScanClasses
                                             from mi in ass.GetCustomAttributeCached<BootstrapperAttribute>()!.Type!.GetMethodsCached(BindingFlags.Public | BindingFlags.Static)
                                             where !mi.IsGenericMethod && mi.GetCustomAttributeCached<BootstrapperAttribute>() is not null
                                             select mi);
                // Find types and methods
                if (FindMethods)
                    methods = methods.Concat(from ass in TypeHelper.Instance.Assemblies
                                             where ass.GetCustomAttributeCached<BootstrapperAttribute>() is BootstrapperAttribute attr &&
                                               attr.Type is null &&
                                               attr.Method is null &&
                                               attr.ScanMethods
                                             from type in ass.GetTypes()
                                             where type.IsPublic &&
                                              !type.IsGenericTypeDefinition &&
                                              type.GetCustomAttributeCached<BootstrapperAttribute>() is not null
                                             from mi in type.GetMethodsCached(BindingFlags.Public | BindingFlags.Static)
                                             where !mi.IsGenericMethod &&
                                              mi.GetCustomAttributeCached<BootstrapperAttribute>() is not null
                                             select mi);
                // Run the bootstrapper methods
                foreach (MethodInfo mi in methods.OrderByDescending(mi => mi.DeclaringType!.Assembly.GetCustomAttributeCached<BootstrapperAttribute>()!.Priority)
                    .ThenByDescending(mi => mi.DeclaringType!.GetCustomAttributeCached<BootstrapperAttribute>()?.Priority ?? 0)
                    .ThenByDescending(mi => mi.GetCustomAttributeCached<BootstrapperAttribute>()?.Priority ?? 0)
                    .ThenBy(mi => mi.Name))
                {
                    Logging.WriteDebug($"Calling bootstrapper {mi.DeclaringType}.{mi.Name}");
                    if (mi.DeclaringType is not null)
                    {
                        using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
                        BootedAssemblies.Add(mi.DeclaringType.Assembly.GetHashCode());
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
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext())
                if (IsBooting || DidBoot) return false;
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
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext())
                if (!BootedAssemblies.Add(assembly.GetHashCode())) return;
            Logging.WriteDebug($"Single bootstrapping of assembly {assembly.GetName().FullName}");
            if (!DidBoot && !IsBooting) await Async(cancellationToken: cancellationToken).DynamicContext();
            TypeHelper.Instance.ScanAssemblies(assembly);
            // Fixed type and method
            IEnumerable<MethodInfo> methods = assembly.GetCustomAttributeCached<BootstrapperAttribute>() is BootstrapperAttribute attr &&
                attr.Type is not null &&
                attr.Method is not null
                ? new MethodInfo[]
                {
                        assembly.GetCustomAttributeCached<BootstrapperAttribute>()!.Type!.GetMethodCached(
                                                      assembly.GetCustomAttributeCached<BootstrapperAttribute>()!.Method!,
                                                      BindingFlags.Static | BindingFlags.Public
                                                      )
                        ?? throw new InvalidProgramException($"Bootstrapper method {assembly.GetCustomAttributeCached<BootstrapperAttribute>()!.Type}.{assembly.GetCustomAttributeCached<BootstrapperAttribute>()!.Method} not found")
                }
                : Array.Empty<MethodInfo>();
            // Fixed type
            if (findClasses)
                methods = methods.Concat(from ass in new Assembly[] { assembly }
                                         where ass.GetCustomAttributeCached<BootstrapperAttribute>() is BootstrapperAttribute attr &&
                                            attr.Type is not null &&
                                            attr.Method is null &&
                                            attr.ScanClasses
                                         from mi in ass.GetCustomAttributeCached<BootstrapperAttribute>()!.Type!.GetMethodsCached(BindingFlags.Public | BindingFlags.Static)
                                         where !mi.IsGenericMethod && mi.GetCustomAttributeCached<BootstrapperAttribute>() is not null
                                         select mi);
            // Find types and methods
            if (findMethods)
                methods = methods.Concat(from ass in new Assembly[] { assembly }
                                         where ass.GetCustomAttributeCached<BootstrapperAttribute>() is BootstrapperAttribute attr &&
                                           attr.Type is null &&
                                           attr.Method is null &&
                                           attr.ScanMethods
                                         from type in ass.GetTypes()
                                         where type.IsPublic &&
                                          !type.IsGenericTypeDefinition &&
                                          type.GetCustomAttributeCached<BootstrapperAttribute>() is not null
                                         from mi in type.GetMethodsCached(BindingFlags.Public | BindingFlags.Static)
                                         where !mi.IsGenericMethod &&
                                          mi.GetCustomAttributeCached<BootstrapperAttribute>() is not null
                                         select mi);
            // Run the bootstrapper methods
            foreach (MethodInfo mi in methods.OrderByDescending(mi => mi.DeclaringType!.Assembly.GetCustomAttributeCached<BootstrapperAttribute>()!.Priority)
                .ThenByDescending(mi => mi.DeclaringType!.GetCustomAttributeCached<BootstrapperAttribute>()?.Priority ?? 0)
                .ThenByDescending(mi => mi.GetCustomAttributeCached<BootstrapperAttribute>()?.Priority ?? 0)
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

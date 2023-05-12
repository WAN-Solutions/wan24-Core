using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Bootstrapping
    /// </summary>
    public static class Bootstrap
    {
        /// <summary>
        /// Asynchronous bootstrapper (after running bootstrapper methods)
        /// </summary>
        public static List<BootStrapAsync_Delegate> AsyncBootstrapper { get; } = new();

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
        public static async Task Async(Assembly? startAssembly = null, CancellationToken cancellationToken = default)
        {
            lock (AsyncBootstrapper)
            {
                if (DidBoot) throw new InvalidProgramException("Did boot already");
                if (IsBooting) throw new InvalidProgramException("Booting recursion");
                IsBooting = true;
            }
            await Task.Yield();
            DiHelper.ObjectFactories[typeof(CancellationToken)] = delegate (Type t, out object? obj)
            {
                obj = cancellationToken;
                return true;
            };
            try
            {
                TypeHelper.Instance.ScanAssemblies(startAssembly ?? Assembly.GetCallingAssembly());
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
                    .ThenByDescending(mi => mi.GetCustomAttribute<BootstrapperAttribute>()?.Priority ?? 0))
                {
                    Logging.WriteDebug($"Calling bootstrapper {mi.DeclaringType}.{mi.Name}");
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

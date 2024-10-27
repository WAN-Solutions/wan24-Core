using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using static wan24.Core.Logging;

//TODO Use new .NET 9 Lock type for SyncObject instead of an object https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-13.0/lock-object
//TODO With .NET 9 use Task.WhenEach for ExecuteForAllAsync
//TODO With .NET 9 use ReadOnlySpan<T> (or possibly HashSet<T>) for params where possible
//TODO With .NET 9 maybe implement HybridCache
//TODO With .NET 9 maybe use UUID v7
//TODO With .NET 9 ref struct can be used in iterators and asynchronous methods
//TODO .NET 9 allows ref struct generic anti-constraint allows Span-Enumerables!? https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#allows-ref-struct
//TODO .NET 9 allows ref struct in interfaces - maybe they can be implemented here and there
//TODO .NET 9 makes wan24.Core.OrderedDictionary possibly obsolete - check the fact

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
        private static readonly HashSet<Assembly> BootedAssemblies = [];
        /// <summary>
        /// Thread synchronization
        /// </summary>
        private static readonly SemaphoreSync Sync = new();

        /// <summary>
        /// Static constructor
        /// </summary>
        static Bootstrap()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                ErrorHandling.Handle(
                    e.ExceptionObject is Exception ex
                        ? new($"Catched unhandled exception from the current app domain (will terminate: {e.IsTerminating})", ex, ErrorHandling.UNHANDLED_EXCEPTION)
                        : new($"Catched unhandled exception without exception object from the current app domain (will terminate: {e.IsTerminating})", new Exception(), ErrorHandling.UNHANDLED_EXCEPTION)
                    );
            };
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Shutdown.Async().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronous bootstrapper (executed in parallel at the end of the bootstrapper before raising <see cref="OnBootstrap"/>)
        /// </summary>
        public static AsyncEvent<object, EventArgs> OnBootstrapAsync { get; } = new();

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
                IEnumerable<MethodInfoExt> methods = from ass in TypeHelper.Instance.Assemblies
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
                                             where !mi.Method.IsGenericMethod && mi.GetCustomAttributeCached<BootstrapperAttribute>() is not null
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
                                             where !mi.Method.IsGenericMethod &&
                                              mi.GetCustomAttributeCached<BootstrapperAttribute>() is not null
                                             select mi);
                // Run the bootstrapper methods
                foreach (MethodInfoExt mi in methods.OrderByDescending(mi => mi.Method.DeclaringType!.Assembly.GetCustomAttributeCached<BootstrapperAttribute>()!.Priority)
                    .ThenByDescending(mi => mi.Method.DeclaringType!.GetCustomAttributeCached<BootstrapperAttribute>()?.Priority ?? 0)
                    .ThenByDescending(mi => mi.GetCustomAttributeCached<BootstrapperAttribute>()?.Priority ?? 0)
                    .ThenBy(mi => mi.Name))
                {
                    if (Debug) Logging.WriteDebug($"Calling bootstrapper {mi.Method.DeclaringType}.{mi.Name}");
                    if (mi.Method.DeclaringType is not null)
                    {
                        using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
                        BootedAssemblies.Add(mi.Method.DeclaringType.Assembly);
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                    if (mi.ReturnType.IsTask())
                    {
                        await mi.Method.InvokeAutoAsync(obj: null).DynamicContext();
                    }
                    else
                    {
                        mi.Method.InvokeAuto(obj: null);
                    }
                }
                // Raise the events
                await OnBootstrapAsync.Abstract.RaiseEventAsync(new object(), cancellationToken: cancellationToken).DynamicContext();
                OnBootstrapAsync.Abstract.DetachAll();
                OnBootstrap?.Invoke();
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
            catch (BootstrapperException ex)
            {
                ErrorHandling.Handle(new(ex, ErrorHandling.BOOTSTRAPPER_ERROR));
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
                if (!BootedAssemblies.Add(assembly)) return;
            if (Debug) Logging.WriteDebug($"Single bootstrapping of assembly {assembly.GetName().FullName}");
            if (!DidBoot && !IsBooting) await Async(cancellationToken: cancellationToken).DynamicContext();
            TypeHelper.Instance.ScanAssemblies(assembly);
            // Fixed type and method
            IEnumerable<MethodInfoExt> methods = assembly.GetCustomAttributeCached<BootstrapperAttribute>() is BootstrapperAttribute attr &&
                attr.Type is not null &&
                attr.Method is not null
                ? new MethodInfoExt[]
                {
                    assembly.GetCustomAttributeCached<BootstrapperAttribute>()!.Type!.GetMethodCached(
                                                    assembly.GetCustomAttributeCached<BootstrapperAttribute>()!.Method!,
                                                    BindingFlags.Static | BindingFlags.Public
                                                    )
                    ?? throw new InvalidProgramException($"Bootstrapper method {assembly.GetCustomAttributeCached<BootstrapperAttribute>()!.Type}.{assembly.GetCustomAttributeCached<BootstrapperAttribute>()!.Method} not found")
                }
                : [];
            // Fixed type
            if (findClasses)
            {
                BootstrapperAttribute? bsa = assembly.GetCustomAttributeCached<BootstrapperAttribute>();
                if (bsa is not null && bsa.Type is not null && bsa.Method is null && bsa.ScanClasses)
                    methods = methods.Concat(from mi in bsa.Type.GetMethodsCached(BindingFlags.Public | BindingFlags.Static)
                                             where !mi.Method.IsGenericMethod && mi.GetCustomAttributeCached<BootstrapperAttribute>() is not null
                                             select mi);
            }
            // Find types and methods
            if (findMethods)
            {
                BootstrapperAttribute? bsa = assembly.GetCustomAttributeCached<BootstrapperAttribute>();
                if (bsa is not null && bsa.Type is null && bsa.Method is null && bsa.ScanMethods)
                    methods = methods.Concat(from type in assembly.GetTypes()
                                             where type.IsPublic &&
                                              !type.IsGenericTypeDefinition &&
                                              type.GetCustomAttributeCached<BootstrapperAttribute>() is not null
                                             from mi in type.GetMethodsCached(BindingFlags.Public | BindingFlags.Static)
                                             where !mi.Method.IsGenericMethod &&
                                              mi.GetCustomAttributeCached<BootstrapperAttribute>() is not null
                                             select mi);
            }
            // Run the bootstrapper methods
            foreach (MethodInfoExt mi in methods.OrderByDescending(mi => mi.Method.DeclaringType!.Assembly.GetCustomAttributeCached<BootstrapperAttribute>()!.Priority)
                .ThenByDescending(mi => mi.Method.DeclaringType!.GetCustomAttributeCached<BootstrapperAttribute>()?.Priority ?? 0)
                .ThenByDescending(mi => mi.GetCustomAttributeCached<BootstrapperAttribute>()?.Priority ?? 0)
                .ThenBy(mi => mi.Name))
            {
                if (Debug) Logging.WriteDebug($"Calling bootstrapper {mi.Method.DeclaringType}.{mi.Name}");
                cancellationToken.ThrowIfCancellationRequested();
                if (mi.ReturnType.IsTask())
                {
                    await mi.Method.InvokeAutoAsync(obj: null, cancellationToken).DynamicContext();
                }
                else
                {
                    mi.Method.InvokeAuto(obj: null);
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
    }
}

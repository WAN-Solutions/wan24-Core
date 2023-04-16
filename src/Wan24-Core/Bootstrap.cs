using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Bootstrapping
    /// </summary>
    public static class Bootstrap
    {
        /// <summary>
        /// Bootstrap
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task Async(CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            DiHelper.ObjectFactories[typeof(CancellationToken)] = delegate (Type t, out object? obj)
            {
                obj = cancellationToken;
                return true;
            };
            try
            {
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
                methods = methods.Concat(from ass in TypeHelper.Instance.Assemblies
                                         where ass.GetCustomAttribute<BootstrapperAttribute>() is BootstrapperAttribute attr &&
                                            attr.Type != null &&
                                            attr.Method == null
                                         from mi in ass.GetCustomAttribute<BootstrapperAttribute>()!.Type!.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                         where !mi.IsGenericMethod && mi.GetCustomAttribute<BootstrapperAttribute>() != null
                                         select mi);
                // Find types and methods
                methods = methods.Concat(from ass in TypeHelper.Instance.Assemblies
                                         where ass.GetCustomAttribute<BootstrapperAttribute>() is BootstrapperAttribute attr &&
                                           attr.Type == null &&
                                           attr.Method == null
                                         from type in ass.GetTypes()
                                         where type.IsPublic &&
                                          !type.IsGenericTypeDefinition &&
                                          type.GetCustomAttribute<BootstrapperAttribute>() != null
                                         from mi in type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                         where !mi.IsGenericMethod &&
                                          mi.GetCustomAttribute<BootstrapperAttribute>() != null
                                         select mi);
                Console.WriteLine(methods.Count());
                // Run the bootstrapper methods
                foreach (MethodInfo mi in methods.OrderByDescending(mi => mi.DeclaringType!.Assembly.GetCustomAttribute<BootstrapperAttribute>()!.Priority)
                    .ThenByDescending(mi => mi.DeclaringType!.GetCustomAttribute<BootstrapperAttribute>()?.Priority ?? 0)
                    .ThenByDescending(mi => mi.GetCustomAttribute<BootstrapperAttribute>()?.Priority ?? 0))
                {
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
            }
            finally
            {
                DiHelper.ObjectFactories.Remove(typeof(CancellationToken), out _);
            }
        }
    }
}

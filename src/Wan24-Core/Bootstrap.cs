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
                foreach (MethodInfo mi in from ass in TypeHelper.Instance.Assemblies
                                          where ass.GetCustomAttribute<BootstrapperAttribute>() != null
                                          from type in ass.GetTypes()
                                          where !type.IsGenericTypeDefinition && type.GetCustomAttribute<BootstrapperAttribute>() != null
                                          from mi in type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                          where !mi.IsGenericMethod && mi.GetCustomAttribute<BootstrapperAttribute>() != null
                                          orderby ass.GetCustomAttribute<BootstrapperAttribute>()!.Priority descending,
                                            type.GetCustomAttribute<BootstrapperAttribute>()!.Priority descending,
                                            mi.GetCustomAttribute<BootstrapperAttribute>()!.Priority descending,
                                            ass.Location,
                                            type.FullName,
                                            mi.Name
                                          select mi)
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

﻿using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using static wan24.Core.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Type helper
    /// </summary>
    public sealed class TypeHelper
    {
        /// <summary>
        /// Types (key is the type as string)
        /// </summary>
        private readonly ConcurrentDictionary<string, Type> Types = new();
        /// <summary>
        /// Assemblies (key is the assembly full name)
        /// </summary>
        private readonly ConcurrentDictionary<string, Assembly> _Assemblies = new();

        /// <summary>
        /// Static constructor
        /// </summary>
        static TypeHelper()
        {
            // Create the singleton instance with all known assemblies
            Instance = new();
            Instance._Assemblies.AddRange(
            [
                new(typeof(string).Assembly.GetName().FullName, typeof(string).Assembly),
                new(typeof(TypeHelper).Assembly.GetName().FullName, typeof(TypeHelper).Assembly)
            ]);
            if (Assembly.GetEntryAssembly() is Assembly entry) Instance._Assemblies[entry.GetName().FullName] = entry;
            if (Assembly.GetCallingAssembly() is Assembly calling) Instance._Assemblies[calling.GetName().FullName] = calling;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) Instance._Assemblies[assembly.GetName().FullName] = assembly;
            // Scan known assemblies recursive
            foreach (Assembly assembly in Instance._Assemblies.Values.ToArray()) Instance.ScanAssemblies(assembly);
            // When a new assembly was loaded, scan it and boot all added assemblies
            AppDomain.CurrentDomain.AssemblyLoad += async (s, e) =>
            {
                await Task.Yield();
                if (Debug) Logging.WriteDebug($"Scanning and booting late loaded assembly {e.LoadedAssembly.GetName().FullName}");
                List<Task> tasks = [];
                foreach (Assembly ass in Instance.ScanAssemblies(e.LoadedAssembly))
                    if (ass.GetCustomAttributeCached<BootstrapperAttribute>() is not null)
                        tasks.Add(Bootstrap.AssemblyAsync(ass, Bootstrap.FindClasses, Bootstrap.FindMethods));
                await tasks.WaitAll().DynamicContext();
            };
        }

        /// <summary>
        /// Type helper
        /// </summary>
        public TypeHelper() { }

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static TypeHelper Instance { get; }

        /// <summary>
        /// Assemblies
        /// </summary>
        public Assembly[] Assemblies => [.. _Assemblies.Values];

        /// <summary>
        /// Scan all referenced assemblies
        /// </summary>
        /// <param name="reference">Reference assembly (starting point)</param>
        /// <returns>Added assemblies</returns>
        public Assembly[] ScanAssemblies(Assembly? reference = null)
        {
            // Ensure having a reference assembly where we're starting at
            reference ??= Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            if (reference is null)
            {
                if (Warning) Logging.WriteWarning("No reference assembly for scanning");
                return [];
            }
            // Scan referenced assemblies recursive
            if (Debug) Logging.WriteDebug($"Scanning reference assembly {reference.GetName().FullName}");
            HashSet<Assembly> added = [],// Added assemblies (the return value)
                seen = [];// Seen assemblies (avoid recursion)
            Queue<Assembly> queue = new();// Assemblies to scan
            added.Add(reference);
            queue.Enqueue(reference);
            while (queue.TryDequeue(out Assembly? assembly))
            {
                if (!seen.Add(assembly)) continue;
                if (Trace) Logging.WriteTrace($"\tScanning assembly {assembly.GetName().FullName}");
                if (!ENV.IsBrowserApp)
                    foreach (AssemblyName name in assembly.GetReferencedAssemblies())
                        try
                        {
                            if (added.Add(assembly = Assembly.Load(name)))
                            {
                                if (Trace) Logging.WriteTrace($"\t\tFound new referenced assembly {assembly.GetName().FullName}");
                                queue.Enqueue(assembly);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (Warning) Logging.WriteWarning($"Found new referenced assembly {assembly.GetName().FullName}, but failed to load: {ex}");
                        }
            }
            return AddAssemblies([.. added]);
        }

        /// <summary>
        /// Add assemblies
        /// </summary>
        /// <param name="assemblies">Assemblies</param>
        /// <returns>Assemblies</returns>
        public Assembly[] AddAssemblies(params Assembly[] assemblies)
        {
            foreach (Assembly assembly in assemblies.Distinct())
                if (_Assemblies.TryAdd(assembly.GetName().FullName, assembly) && Debug)
                    Logging.WriteDebug($"Added assembly {assembly.GetName().FullName}");
            return assemblies;
        }

        /// <summary>
        /// Add types (will add their assemblies, too)
        /// </summary>
        /// <param name="types">Types</param>
        /// <returns>Types</returns>
        public Type[] AddTypes(params Type[] types)
        {
            Types.AddRange(from type in types.Distinct()
                           select new KeyValuePair<string, Type>(type.ToString(), type));
            foreach (Assembly ass in (from t in types
                                      where !_Assemblies.ContainsKey(t.Assembly.GetName().FullName)
                                      select t.Assembly).Distinct())
                ScanAssemblies(ass);
            return types;
        }

        /// <summary>
        /// Load a type from its name
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Type</returns>
        public Type? GetType(string name, bool throwOnError = false)
        {
            if (Types.TryGetValue(name, out Type? res)) return res;
            if ((res = Type.GetType(name, throwOnError)) is not null)
            {
                Types[name] = res;
                return res;
            }
            res = (from a in Assemblies
                   where (res = a.GetType(name, throwOnError)) is not null
                   select res).FirstOrDefault();
            if (res is null)
            {
                LoadTypeEventArgs e = new(name);
                OnLoadType?.Invoke(e);
                res = e.Type;
            }
            if (res is not null) Types[name] = res;
            return res;
        }

        /// <summary>
        /// Remove an assembly (and its types)
        /// </summary>
        /// <param name="assembly">Assembly to remove</param>
        /// <returns>Removed types</returns>
        public Type[] RemoveAssembly(Assembly assembly)
        {
            KeyValuePair<string, Type>[] types = [.. Types.Where(kvp => kvp.Value.Assembly == assembly)];
            _Assemblies.TryRemove(assembly.GetName().FullName, out _);
            foreach (string type in types.Select(kvp => kvp.Key)) Types.TryRemove(type, out _);
            return [.. types.Select(kvp => kvp.Value)];
        }

        /// <summary>
        /// Remove all assemblies from a load context
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Removed assemblies</returns>
        public Assembly[] RemoveLoadContext(AssemblyLoadContext? context)
        {
            Assembly[] res = [.. _Assemblies.Values.Where(a => AssemblyLoadContext.GetLoadContext(a) == context)];
            foreach(Assembly assembly in res) RemoveAssembly(assembly);
            return res;
        }

        /// <summary>
        /// Delegate for the type loader event
        /// </summary>
        /// <param name="e">Event arguments</param>
        public delegate void LoadType_Delegate(LoadTypeEventArgs e);
        /// <summary>
        /// Raised when loading a type
        /// </summary>
        public event LoadType_Delegate? OnLoadType;
    }
}

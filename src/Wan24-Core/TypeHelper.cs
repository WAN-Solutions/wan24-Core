using System.Collections.Concurrent;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Type helper
    /// </summary>
    public sealed class TypeHelper
    {
        /// <summary>
        /// Types
        /// </summary>
        private readonly ConcurrentDictionary<string, Type> Types = new();
        /// <summary>
        /// Assemblies
        /// </summary>
        private readonly List<Assembly> _Assemblies;

        /// <summary>
        /// Static constructor
        /// </summary>
        static TypeHelper() => Instance = new();

        /// <summary>
        /// Type helper
        /// </summary>
        public TypeHelper()
        {
            _Assemblies = new(new Assembly[]
            {
                typeof(string).Assembly,
                typeof(TypeHelper).Assembly
            });
            if (Assembly.GetEntryAssembly() is Assembly entry && !_Assemblies.Contains(entry)) _Assemblies.Add(entry);
            if (Assembly.GetCallingAssembly() is Assembly calling && !_Assemblies.Contains(calling)) _Assemblies.Add(calling);
            Assembly? reference = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            if (reference == null) return;
            List<Assembly> seen = new();
            void AddAssemblies(Assembly ass)
            {
                if (seen.Contains(ass)) return;
                seen.Add(ass);
                foreach (AssemblyName name in ass.GetReferencedAssemblies())
                {
                    ass = Assembly.Load(name);
                    if (!_Assemblies.Contains(ass)) _Assemblies.Add(ass);
                    AddAssemblies(ass);
                }
            }
            AddAssemblies(reference);
        }

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static TypeHelper Instance { get; }

        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        public object SyncObject { get; } = new();

        /// <summary>
        /// Assemblies
        /// </summary>
        public Assembly[] Assemblies
        {
            get
            {
                lock (SyncObject) return _Assemblies.ToArray();
            }
        }

        /// <summary>
        /// Add assemblies
        /// </summary>
        /// <param name="assemblies">Assemblies</param>
        /// <returns>Assemblies</returns>
        public Assembly[] AddAssemblies(params Assembly[] assemblies)
        {
            lock(SyncObject) foreach (Assembly assembly in assemblies) if (!_Assemblies.Contains(assembly)) _Assemblies.Add(assembly);
            return assemblies;
        }

        /// <summary>
        /// Add types (will add their assemblies, too)
        /// </summary>
        /// <param name="types">Types</param>
        /// <returns>Types</returns>
        public Type[] AddTypes(params Type[] types)
        {
            foreach (Type type in types) Types[type.ToString()] = type;
            Assembly[] assemblies = (from t in types
                                     where !_Assemblies.Contains(t.Assembly)
                                     select t.Assembly)
                                   .ToArray();
            if (assemblies.Length > 0) AddAssemblies(assemblies);
            return types;
        }

        /// <summary>
        /// Load a type from its name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Type</returns>
        public Type? GetType(string name)
        {
            if (Types.TryGetValue(name, out Type? res)) return res;
            if ((res ??= Type.GetType(name, throwOnError: false)) != null)
            {
                Types[name] = res!;
                return res;
            }
            res = (from a in Assemblies
                   where (res = a.GetType(name, throwOnError: false)) != null
                   select res)
                  .FirstOrDefault();
            if (res == null)
            {
                LoadTypeEventArgs e = new(name);
                OnLoadType?.Invoke(e);
                res = e.Type;
            }
            if (res != null) Types[name] = res;
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

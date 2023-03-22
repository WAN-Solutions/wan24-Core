using System.Collections.Concurrent;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Type helper
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// Types
        /// </summary>
        private static readonly ConcurrentDictionary<string, Type> Types = new();

        /// <summary>
        /// Assemblies
        /// </summary>
        public static readonly ConcurrentBag<Assembly> Assemblies;

        /// <summary>
        /// Type helper
        /// </summary>
        static TypeHelper() => Assemblies = new(new Assembly[]
        {
            typeof(string).Assembly,
            typeof(TypeHelper).Assembly
        });

        /// <summary>
        /// Add assemblies
        /// </summary>
        /// <param name="assemblies">Assemblies</param>
        /// <returns>Assemblies</returns>
        public static Assembly[] AddAssemblies(params Assembly[] assemblies)
        {
            foreach (Assembly assembly in assemblies) Assemblies.Add(assembly);
            return assemblies;
        }

        /// <summary>
        /// Add types (will add their assemblies, too)
        /// </summary>
        /// <param name="types">Types</param>
        /// <returns>Types</returns>
        public static Type[] AddTypes(params Type[] types)
        {
            foreach (Type type in types) Types[type.ToString()] = type;
            Assembly[] assemblies = (from t in types
                                     where !Assemblies.Contains(t.Assembly)
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
        public static Type? GetType(string name)
        {
            Type? res = null;
            if (Types.TryGetValue(name, out res)) return res;
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
        public static event LoadType_Delegate? OnLoadType;
    }
}

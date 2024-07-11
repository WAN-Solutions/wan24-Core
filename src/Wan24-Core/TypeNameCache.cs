using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Type name cache
    /// </summary>
    public static class TypeNameCache
    {
        /// <summary>
        /// Type cache (key is the type name (see <see cref="Type.ToString"/>) hash code)
        /// </summary>
        public static readonly ConcurrentDictionary<int, Type> Types = [];

        /// <summary>
        /// Add types to the <see cref="Types"/> cache
        /// </summary>
        /// <param name="types">Types</param>
        public static void Add(params Type[] types)
        {
            foreach(Type type in types) Types[type.ToString().GetHashCode()] = type;
        }
    }
}

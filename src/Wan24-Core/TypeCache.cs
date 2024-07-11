using System.Collections.Concurrent;

//TODO Use type string hash code as key from version 3+

namespace wan24.Core
{
    /// <summary>
    /// Type cache
    /// </summary>
    public static class TypeCache
    {
        /// <summary>
        /// Type cache (key is the types hash code)
        /// </summary>
        public static readonly ConcurrentDictionary<int, Type> Types = [];

        /// <summary>
        /// Add types to the <see cref="Types"/> cache
        /// </summary>
        /// <param name="types">Types</param>
        public static void Add(params Type[] types)
        {
            foreach(Type type in types) Types[type.GetHashCode()] = type;
        }
    }
}

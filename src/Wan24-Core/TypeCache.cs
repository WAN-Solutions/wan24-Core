using System.Collections.Concurrent;

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
        internal static readonly ConcurrentDictionary<int, Type> Types = [];
        /// <summary>
        /// Type cache (key is the types name (see <see cref="Type.ToString"/>) hash code)
        /// </summary>
        internal static readonly ConcurrentDictionary<int, Type> TypeNames = [];

        /// <summary>
        /// Constructor
        /// </summary>
        static TypeCache() => Add(
            typeof(bool),
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(Half),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(string),
            typeof(byte[]),
            typeof(int[]),
            typeof(long[]),
            typeof(float[]),
            typeof(double[]),
            typeof(decimal[]),
            typeof(string[])
            );

        /// <summary>
        /// Add types to the <see cref="Types"/> and the <see cref="TypeNames"/> cache
        /// </summary>
        /// <param name="types">Types</param>
        public static void Add(params Type[] types)
        {
            foreach (Type type in types)
            {
                Types[type.GetHashCode()] = type;
                TypeNames[type.ToString().GetHashCode()] = type;
            }
        }

        /// <summary>
        /// Determine if a type was cached
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public static bool Contains(in Type type) => Types.ContainsKey(type.GetHashCode());

        /// <summary>
        /// Get a cached type by its hash code (see <see cref="Type.GetHashCode"/>)
        /// </summary>
        /// <param name="hashCode">Hash code (see <see cref="Type.GetHashCode"/>)</param>
        /// <returns>Type</returns>
        public static Type? GetTypeByHashCode(in int hashCode) => Types.TryGetValue(hashCode, out Type? res) ? res : null;

        /// <summary>
        /// Get a cached type by its hash code (see <see cref="Type.ToString"/>)
        /// </summary>
        /// <param name="hashCode">Name hash code (see <see cref="Type.ToString"/>)</param>
        /// <returns>Type</returns>
        public static Type? GetTypeByNameHashCode(in int hashCode) => TypeNames.TryGetValue(hashCode, out Type? res) ? res : null;
    }
}

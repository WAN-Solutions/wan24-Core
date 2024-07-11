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
        /// Constructor
        /// </summary>
        static TypeNameCache() => Add(
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
        /// Add types to the <see cref="Types"/> cache
        /// </summary>
        /// <param name="types">Types</param>
        public static void Add(params Type[] types)
        {
            foreach(Type type in types) Types[type.ToString().GetHashCode()] = type;
        }
    }
}

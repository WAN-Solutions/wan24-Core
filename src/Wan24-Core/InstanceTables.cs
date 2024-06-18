using System.Collections.Concurrent;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Instance tables
    /// </summary>
    public static class InstanceTables
    {
        /// <summary>
        /// Registered instance tables
        /// </summary>
        public static readonly Dictionary<Type, Type> Registered = new()
        {
            {typeof(Delay), typeof(DelayTable) },
            {typeof(IInMemoryCache), typeof(InMemoryCacheTable) },
            {typeof(IObjectLockManager), typeof(ObjectLockTable) },
            {typeof(IPool), typeof(PoolTable) },
            {typeof(IProcessingInfo), typeof(ProcessTable) },
            {typeof(IServiceWorker), typeof(ServiceWorkerTable) },
            {typeof(ITimer), typeof(TimerTable) }
        };

        /// <summary>
        /// Find the instance table field of a type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Instance table field</returns>
        public static FieldInfo? FindInstanceTableField(Type type)
            => (from field in type.GetFieldsCached(BindingFlags.Public | BindingFlags.Static)
                where field.GetCustomAttributeCached<InstanceTableAttribute>() is not null
                select field)
                .FirstOrDefault();

        /// <summary>
        /// Determine if a type is a valid instance table dictionary type (<see cref="ConcurrentDictionary{IObjectKey,tValue}"/> with a <see cref="string"/> key)
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="checkBaseTypes">Check base types also?</param>
        /// <returns>If the type is a valid instance table dictionary type</returns>
        public static bool IsValidInstanceTableType(in Type type, in bool checkBaseTypes = false)
        {
            if (
                type.IsGenericType &&
                !type.IsGenericTypeDefinition &&
                type.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>) &&
                type.GetGenericArguments()[0] == typeof(string)
                )
                return true;
            return checkBaseTypes && type.GetBaseTypes().Any(
                t => t.IsGenericType &&
                    t.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>) &&
                    t.GetGenericArguments()[0] == typeof(string)
                );
        }
    }
}

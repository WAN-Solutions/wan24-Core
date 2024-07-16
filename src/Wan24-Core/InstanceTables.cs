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
            {typeof(ITimer), typeof(TimerTable) },
            {typeof(IThrottle), typeof(ThrottleTable) }
        };

        /// <summary>
        /// Find instance table provider informations for a value type
        /// </summary>
        /// <param name="valueType">Value type</param>
        /// <returns>Instance table information (key is the tables value type, value the instance table provider type)</returns>
        public static KeyValuePair<Type, Type>? FindTableProviderInfo(Type valueType)
            => Registered.FirstOrDefault(kvp => kvp.Key.IsAssignableFromExt(valueType));

        /// <summary>
        /// Find the instance table field of an instance table provider type
        /// </summary>
        /// <param name="providerType">Instance table provider type</param>
        /// <returns>Instance table field</returns>
        public static FieldInfo? FindTableProviderField(Type providerType)
            => (from field in providerType.GetFieldsCached(BindingFlags.Public | BindingFlags.Static)
                where field.GetCustomAttributeCached<InstanceTableAttribute>() is not null
                select field)
                .FirstOrDefault();

        /// <summary>
        /// Determine if a type is a valid instance table dictionary type (<see cref="ConcurrentDictionary{IObjectKey,tValue}"/> with a <see cref="string"/> key)
        /// </summary>
        /// <param name="fieldType">Type</param>
        /// <param name="checkBaseTypes">Check base types also?</param>
        /// <returns>If the type is a valid instance table dictionary type</returns>
        public static bool IsValidTableType(in Type fieldType, in bool checkBaseTypes = false)
        {
            if (
                fieldType.IsGenericType &&
                !fieldType.IsGenericTypeDefinition &&
                fieldType.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>) &&
                fieldType.GetGenericArguments()[0] == typeof(string)
                )
                return true;
            return checkBaseTypes && fieldType.GetBaseTypes().Any(
                t => t.IsGenericType &&
                    t.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>) &&
                    t.GetGenericArguments()[0] == typeof(string)
                );
        }

        /// <summary>
        /// Find an instance
        /// </summary>
        /// <param name="valueType">Expected value type</param>
        /// <param name="key">Instance key</param>
        /// <returns>Instance</returns>
        public static object? FindInstanceOf(in Type valueType, in string key)
            => FindTableProviderInfo(valueType) is not KeyValuePair<Type, Type> providerTable
                ? throw new InvalidOperationException($"{valueType} is not a suported value type (instance table provider not found)")
                : FindInstance(providerTable.Value, key);

        /// <summary>
        /// Find an instance
        /// </summary>
        /// <param name="providerType">Instance table provider type</param>
        /// <param name="key">Instance key</param>
        /// <returns>Instance</returns>
        public static object? FindInstance(in Type providerType, in string key)
            => FindTableProviderField(providerType) is not FieldInfo fi
                ? throw new ArgumentException("Not an instance table provider type (instance table field not found)", nameof(providerType))
                : FindInstance(fi, key);

        /// <summary>
        /// Find an instance
        /// </summary>
        /// <param name="tableField">Instance provider table field</param>
        /// <param name="key">Instance key</param>
        /// <returns>Instance</returns>
        public static object? FindInstance(in FieldInfo tableField, in string key)
        {
            if (!tableField.IsStatic)
                throw new ArgumentException("Not a static instance provider table field", nameof(tableField));
            Type fieldType = IsValidTableType(tableField.FieldType)
                ? tableField.FieldType
                : tableField.FieldType.GetBaseTypes().FirstOrDefault(t => IsValidTableType(t))
                    ?? throw new ArgumentException($"Invalid instance table field type {tableField.FieldType}", nameof(tableField)),
                valueType = fieldType.GetGenericArguments()[1];
            MethodInfo getValueMethod = typeof(ConcurrentDictionary<,>).MakeGenericType(typeof(string), valueType)
                .GetMethodCached(nameof(ConcurrentDictionary<string, object>.TryGetValue), BindingFlags.Public | BindingFlags.Instance)
                    ?? throw new InvalidProgramException($"Failed to get the instance table {tableField.Name} get value method");
            object?[] param = [key, null];
            if (getValueMethod.Invoke(tableField.GetValue(obj: null), param) is null)
                throw new InvalidProgramException($"{tableField.Name} get value method returned no success flag");
            return param[0];
        }
    }
}

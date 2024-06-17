using System.Collections.Concurrent;
using System.Reflection;

namespace wan24.Core
{
    // User actions
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Get exported user action informations
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="providerKey">Provider key of the object instance (leave empty to create user action information templates only)</param>
        /// <returns>User action informations</returns>
        public static IEnumerable<UserActionInfo> GetUserActionInfos(this IExportUserActions obj, string? providerKey = null)
        {
            providerKey ??= string.Empty;
            // Find the provider table instance type
            Type instanceType = InstanceTables.Registered.Keys.FirstOrDefault(k=>k.IsAssignableFromExt(obj.GetType()))
                ?? throw new ArgumentException($"Failed to find provider table object type for {obj.GetType()} in InstanceTables.Registered", nameof(obj));
            // Find the provider table type
            if (!InstanceTables.Registered.TryGetValue(instanceType, out Type? providerType))
                throw new ArgumentException($"Failed to find provider table type for {obj.GetType()} in InstanceTables.Registered", nameof(obj));
            // Find the instance table field
            FieldInfo fi = (from field in providerType.GetFieldsCached(BindingFlags.Public | BindingFlags.Static)
                            where field.GetCustomAttributeCached<InstanceTableAttribute>() is not null
                            select field)
                            .FirstOrDefault()
                                ?? throw new InvalidProgramException($"Provider table type {providerType} for {obj.GetType()} has no field with an {typeof(InstanceTableAttribute)}");
            // Validate the instance table field and it's value type is matching for the given object
            Type fieldType = typeof(ConcurrentDictionary<,>).IsAssignableFrom(fi.FieldType) && fi.FieldType.GetGenericArguments()[0] == typeof(string)
                ? fi.FieldType
                : (from t in fi.FieldType.GetBaseTypes()
                   where typeof(ConcurrentDictionary<,>).IsAssignableFrom(t) && t.GetGenericArguments()[0] == typeof(string)
                   select t)
                    .FirstOrDefault()
                        ?? throw new InvalidProgramException($"Invalid instance table field type {fi.FieldType} for {obj.GetType()}"),
                valueType = fieldType.GetGenericArguments()[1];
            if (!valueType.IsAssignableFromExt(obj.GetType()))
                throw new InvalidProgramException($"{obj.GetType()} can not be hosted by {providerType}.{fi.Name} ({valueType})");
            // Find user action methods
            foreach (MethodInfo mi in from method in obj.GetType().GetMethodsCached(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                      where method.GetCustomAttributeCached<UserActionAttribute>() is not null
                                      select method)
                yield return UserActionInfo.FromMethod(mi, providerType.ToString(), fi.Name, mi.IsStatic ? string.Empty : providerKey);
        }
    }
}

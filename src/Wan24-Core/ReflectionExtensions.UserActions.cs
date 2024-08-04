using System.Reflection;

namespace wan24.Core
{
    // User actions
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Get exported user action information
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="providerKey">Provider key of the object instance (leave empty to create user action information templates only)</param>
        /// <returns>User action information</returns>
        public static IEnumerable<UserActionInfo> GetUserActionInfos(this IExportUserActions obj, string? providerKey = null)
        {
            providerKey ??= string.Empty;
            // Find the provider table type
            KeyValuePair<Type,Type> provider = InstanceTables.FindTableProviderInfo(obj.GetType())
                ?? throw new ArgumentException($"Failed to find provider table object type for {obj.GetType()} in InstanceTables.Registered", nameof(obj));
            // Find the instance table field
            FieldInfoExt fi = InstanceTables.FindTableProviderField(provider.Value)
                ?? throw new InvalidProgramException($"Provider table type {provider.Value} for {obj.GetType()} has no field with an {typeof(InstanceTableAttribute)}");
            // Validate the instance table field and it's value type is matching for the given object
            Type fieldType = InstanceTables.IsValidTableType(fi.FieldType)
                ? fi.FieldType
                : fi.FieldType.GetBaseTypes().FirstOrDefault(t => InstanceTables.IsValidTableType(t))
                    ?? throw new InvalidProgramException($"Invalid instance table field type {fi.FieldType} for {obj.GetType()}"),
                valueType = fieldType.GetGenericArguments()[1];
            if (!valueType.IsAssignableFromExt(obj.GetType()))
                throw new InvalidProgramException($"{obj.GetType()} can not be hosted by {provider.Value}.{fi.Name} ({valueType})");
            // Find user action methods
            foreach (MethodInfoExt mi in from method in obj.GetType().GetMethodsCached(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                      where method.GetCustomAttributeCached<UserActionAttribute>() is not null
                                      select method)
                yield return UserActionInfo.FromMethod(mi, provider.Value.ToString(), fi.Name, mi.Method.IsStatic ? string.Empty : providerKey);
        }
    }
}

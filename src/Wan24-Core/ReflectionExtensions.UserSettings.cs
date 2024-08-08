using System.Reflection;

namespace wan24.Core
{
    // User settings
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Get exported user setting information
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="providerKey">Provider key of the object instance (leave empty to create user setting information templates only)</param>
        /// <returns>User setting information</returns>
        public static IEnumerable<UserSettingInfo> GetUserSettingInfos(this IExportUserSettings obj, string? providerKey = null)
        {
            providerKey ??= string.Empty;
            // Find the provider table type
            KeyValuePair<Type, Type> provider = InstanceTables.FindTableProviderInfo(obj.GetType())
                ?? throw new ArgumentException($"Failed to find provider table object type for {obj.GetType()} in InstanceTables.Registered", nameof(obj));
            // Find the instance table field
            FieldInfoExt fi = InstanceTables.FindTableProviderField(provider.Value)
                ?? throw new InvalidProgramException($"Provider table type {provider.Value} for {obj.GetType()} has no field with an {typeof(InstanceTableAttribute)}");
            // Validate the instance table field and it's value type is matching for the given object
            Type fieldType = InstanceTables.IsValidTableType(fi.FieldType)
                ? fi.FieldType
                : fi.FieldType.GetBaseTypes().FirstOrDefault(t => InstanceTables.IsValidTableType(t))
                    ?? throw new InvalidProgramException($"Invalid instance table field type {fi.FieldType} for {obj.GetType()}"),
                valueType = fieldType.GetGenericArgumentsCached()[1];
            if (!valueType.IsAssignableFromExt(obj.GetType()))
                throw new InvalidProgramException($"{obj.GetType()} can not be hosted by {provider.Value}.{fi.Name} ({valueType})");
            // Find user setting properties
            foreach (PropertyInfoExt pi in from prop in obj.GetType().GetPropertiesCached(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                           where prop.GetCustomAttributeCached<UserSettingAttribute>() is not null
                                           select prop)
                yield return UserSettingInfo.FromProperty(pi, provider.Value.ToString(), fi.Name, (pi.Property.GetMethod?.IsStatic ?? pi.Property.SetMethod?.IsStatic ?? false) ? string.Empty : providerKey);
        }
    }
}

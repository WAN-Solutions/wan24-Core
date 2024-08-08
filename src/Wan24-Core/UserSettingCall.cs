using System.Reflection;
using System.Text.Json.Serialization;

namespace wan24.Core
{
    /// <summary>
    /// User action call
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class UserSettingCall()
    {
        /// <summary>
        /// Property name
        /// </summary>
        public required string Property { get; init; }

        /// <summary>
        /// If to call the getter
        /// </summary>
        public required bool Get { get; init; }

        /// <summary>
        /// Valut to set
        /// </summary>
        public string? Value { get; init; }

        /// <summary>
        /// Provider CLR type name
        /// </summary>
        public required string ProviderType { get; init; }

        /// <summary>
        /// Provider
        /// </summary>
        [JsonIgnore]
        public Type? Provider => TypeHelper.Instance.GetType(ProviderType);

        /// <summary>
        /// Provider static dictionary field name
        /// </summary>
        public required string ProviderField { get; init; }

        /// <summary>
        /// Provider instance key
        /// </summary>
        public string ProviderKey { get; init; } = string.Empty;

        /// <summary>
        /// Execute the call
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public async Task<object?> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            // Find the object instance and the object type (for static properties only), then get the user setting property
            object? instance = null;
            Type? providerType = Provider ?? throw new InvalidDataException("Failed to get the instance provider type");
            FieldInfoExt fi = providerType.GetFieldCached(ProviderField, BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidDataException($"Failed to find instance table field in {providerType}");
            if (fi.GetCustomAttributeCached<InstanceTableAttribute>() is null)
                throw new InvalidDataException($"{providerType}.{fi.Name} is missing the {typeof(InstanceTableAttribute)}");
            Type fieldType = InstanceTables.IsValidTableType(fi.FieldType)
                ? fi.FieldType
                : fi.FieldType.GetBaseTypes().FirstOrDefault(t => InstanceTables.IsValidTableType(t))
                    ?? throw new InvalidProgramException($"Invalid instance table field type {fi.FieldType} for {providerType}"),
                valueType = fieldType.GetGenericArgumentsCached()[1];
            PropertyInfoExt pi;
            if (ProviderKey.Length > 0)
            {
                instance = InstanceTables.FindInstance(fi, ProviderKey);
                if (instance is null)
                    throw new InvalidOperationException("The object instance with the given key wasn't found in the instance provider table");
                if (!valueType.IsAssignableFrom(instance.GetType()))
                    throw new InvalidProgramException($"{providerType}.{fi.Name} is hosting an incompatible {instance.GetType()} instance (value type is {valueType})");
                pi = instance.GetType().GetPropertyCached(Property, BindingFlags.Public | BindingFlags.Instance)
                    ?? throw new InvalidDataException("User action instance property not found");
            }
            else
            {
                pi = valueType.GetType().GetPropertyCached(ProviderKey, BindingFlags.Public | BindingFlags.Static)
                    ?? throw new InvalidDataException("User action static property not found");
            }
            if (Get && (pi.Property.GetMethod is null || !pi.Property.GetMethod.IsPublic))
                throw new InvalidOperationException($"{valueType.GetType()}.{pi.Name} has no public getter");
            if (!Get && (pi.Property.SetMethod is null || !pi.Property.SetMethod.IsPublic))
                throw new InvalidOperationException($"{valueType.GetType()}.{pi.Name} has no public setter");
            if (pi.GetCustomAttributeCached<UserSettingAttribute>() is null)
                throw new UnauthorizedAccessException($"{valueType.GetType()}.{pi.Name} isn't an user setting property");
            // Validate the call
            object? value = null;
            if (!Get)
            {
                if (Value is null)
                {
                    if (!pi.IsNullable())
                        throw new InvalidDataException($"{valueType.GetType()}.{pi.Name} requires a non-null value");
                }
                else if (pi.PropertyType == typeof(CancellationToken))
                {
                    value = cancellationToken;
                }
                else if (StringValueConverter.CanConvertFromString(pi.PropertyType))
                {
                    value = StringValueConverter.Convert(pi.PropertyType, Value);
                }
                else
                {
                    throw new InvalidDataException($"{valueType.GetType()}.{pi.Name} CLR type {pi.PropertyType} isn't supported");
                }
            }
            // Execute the getter/setter
            try
            {
                if (Get) return pi.Getter!(instance);
                pi.Setter!(instance, value);
                return null;
            }
            catch (Exception ex)
            {
                throw new AggregateException("User setting execution failed", ex);
            }
        }
    }
}

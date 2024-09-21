using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;

namespace wan24.Core
{
    /// <summary>
    /// User setting information
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public sealed record class UserSettingInfo()
    {
        /// <summary>
        /// <see cref="Status"/> key for an array of <see cref="UserSettingInfo"/>
        /// </summary>
        public const string STATE_KEY = "__userSettings";

        /// <summary>
        /// Property name
        /// </summary>
        public required string Property { get; init; }

        /// <summary>
        /// CLR type name
        /// </summary>
        public required string ClrType { get; init; }

        /// <summary>
        /// Type
        /// </summary>
        [JsonIgnore]
        public Type? Type => TypeHelper.Instance.GetType(ClrType);

        /// <summary>
        /// If the parameter value is nullable
        /// </summary>
        public bool IsNullable { get; init; }

        /// <summary>
        /// If the property has a public getter
        /// </summary>
        public bool CanGet { get; init; }

        /// <summary>
        /// If the property has a public setter
        /// </summary>
        public bool CanSet { get; init; }

        /// <summary>
        /// Action title
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Object provider type name
        /// </summary>
        public required string ProviderType { get; init; }

        /// <summary>
        /// Provider
        /// </summary>
        [JsonIgnore]
        public Type? Provider => TypeHelper.Instance.GetType(ProviderType, throwOnError: true);

        /// <summary>
        /// Object provider concurrent dictionary static field name
        /// </summary>
        public required string ProviderField { get; init; }

        /// <summary>
        /// Object provider dictionary key
        /// </summary>
        public required string ProviderKey { get; init; }

        /// <summary>
        /// Create a call
        /// </summary>
        /// <param name="get">If to call the getter</param>
        /// <param name="value">Value to set</param>
        /// <returns>Call</returns>
        public UserSettingCall CreateCall(in bool get, in string? value = null)
            => new()
            {
                Property = Property,
                Get = get,
                Value = value,
                ProviderType = ProviderType,
                ProviderField = ProviderField,
                ProviderKey = ProviderKey
            };

        /// <summary>
        /// Create from a property
        /// </summary>
        /// <param name="pi">User setting property</param>
        /// <param name="providerType">Provider type name</param>
        /// <param name="providerField">Static provider dictionary field name</param>
        /// <param name="providerKey">Provider instance key</param>
        /// <param name="nic">Nullability context</param>
        /// <returns>User setting information</returns>
        public static UserSettingInfo FromProperty(
            in PropertyInfoExt pi, 
            in string providerType, 
            in string providerField, 
            in string providerKey,
            NullabilityInfoContext? nic = null
            )
        {
            if (pi.GetCustomAttributeCached<UserSettingAttribute>() is null) throw new ArgumentException("Not an user setting property", nameof(pi));
            return new()
            {
                Property = pi.Name,
                ClrType = pi.PropertyType.ToString(),
                IsNullable = pi.Property.IsNullable(nic),
                CanGet = pi.Property.GetMethod is not null && pi.Property.GetMethod.IsPublic,
                CanSet = pi.Property.SetMethod is not null && pi.Property.SetMethod.IsPublic,
                Title = pi.GetDisplayText(),
                Description = pi.GetCustomAttributeCached<DescriptionAttribute>()?.Description,
                ProviderType = providerType,
                ProviderField = providerField,
                ProviderKey = providerKey
            };
        }

        /// <summary>
        /// Create from a property
        /// </summary>
        /// <param name="pi">User setting property</param>
        /// <param name="providerKey">Provider instance key</param>
        /// <param name="template">User setting information template (must include provider type and static dictionary field name)</param>
        /// <param name="nic">Nullability context</param>
        /// <returns>User setting information</returns>
        public static UserSettingInfo FromProperty(in PropertyInfoExt pi, in string providerKey, in UserSettingInfo template, NullabilityInfoContext? nic = null)
        {
            if (pi.GetCustomAttributeCached<UserSettingAttribute>() is null) throw new ArgumentException("Not an user setting property", nameof(pi));
            return template with
            {
                Property = pi.Name,
                ClrType = pi.PropertyType.ToString(),
                IsNullable = pi.Property.IsNullable(nic),
                CanGet = pi.Property.GetMethod is not null && pi.Property.GetMethod.IsPublic,
                CanSet = pi.Property.SetMethod is not null && pi.Property.SetMethod.IsPublic,
                Title = pi.GetDisplayText(),
                Description = pi.GetCustomAttributeCached<DescriptionAttribute>()?.Description,
                ProviderKey = providerKey
            };
        }
    }
}

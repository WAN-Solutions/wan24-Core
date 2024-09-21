using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;

namespace wan24.Core
{
    /// <summary>
    /// User action information
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public sealed record class UserActionInfo()
    {
        /// <summary>
        /// <see cref="Status"/> key for an array of <see cref="UserActionInfo"/>
        /// </summary>
        public const string STATE_KEY = "__userActions";
        
        /// <summary>
        /// Method name
        /// </summary>
        public required string Method { get; init; }

        /// <summary>
        /// Parameter information
        /// </summary>
        public required UserActionParameterInfo[] Parameters { get; init; }

        /// <summary>
        /// If the action is allowed to be executed for multiple instances
        /// </summary>
        public bool MultiAction { get; init; }

        /// <summary>
        /// If the user action is the default action for an instance
        /// </summary>
        public bool IsDefault { get; init; }

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
        /// <param name="parameters">Parameters</param>
        /// <returns>Call</returns>
        public UserActionCall CreateCall(params string?[] parameters)
            => new()
            {
                Method = Method,
                Parameters = parameters,
                ProviderType = ProviderType,
                ProviderField = ProviderField,
                ProviderKey = ProviderKey
            };

        /// <summary>
        /// Create from a method
        /// </summary>
        /// <param name="mi">User action method</param>
        /// <param name="providerType">Provider type name</param>
        /// <param name="providerField">Static provider dictionary field name</param>
        /// <param name="providerKey">Provider instance key</param>
        /// <returns>User action information</returns>
        public static UserActionInfo FromMethod(in MethodInfo mi, in string providerType, in string providerField, in string providerKey)
        {
            UserActionAttribute attr = mi.GetCustomAttributeCached<UserActionAttribute>() ?? throw new ArgumentException("Not an user action method", nameof(mi));
            return new()
            {
                Method = mi.Name,
                Parameters = [.. CreateParameterInfo([..mi.GetParametersCached()])],
                MultiAction = attr.MultiAction,
                IsDefault = attr.IsDefault,
                Title = mi.GetDisplayText(),
                Description=mi.GetCustomAttributeCached<DescriptionAttribute>()?.Description,
                ProviderType = providerType,
                ProviderField = providerField,
                ProviderKey = providerKey
            };
        }

        /// <summary>
        /// Create from a method
        /// </summary>
        /// <param name="mi">User action method</param>
        /// <param name="providerKey">Provider instance key</param>
        /// <param name="template">User action information template (must include provider type and static dictionary field name)</param>
        /// <returns>User action information</returns>
        public static UserActionInfo FromMethod(in MethodInfo mi, in string providerKey, in UserActionInfo template)
        {
            UserActionAttribute attr = mi.GetCustomAttributeCached<UserActionAttribute>() ?? throw new ArgumentException("Not an user action method", nameof(mi));
            return template with
            {
                Method = mi.Name,
                Parameters = [.. CreateParameterInfo([..mi.GetParametersCached()])],
                MultiAction = attr.MultiAction,
                IsDefault = attr.IsDefault,
                Title = mi.GetDisplayText(),
                Description = mi.GetCustomAttributeCached<DescriptionAttribute>()?.Description,
                ProviderKey= providerKey
            };
        }

        /// <summary>
        /// Create user action parameter information
        /// </summary>
        /// <param name="param">Parameters</param>
        /// <returns>Parameter information</returns>
        public static IEnumerable<UserActionParameterInfo> CreateParameterInfo(params ParameterInfo[] param)
        {
            NullabilityInfoContext nic = new();
            foreach(ParameterInfo pi in param)
            {
                if (pi.ParameterType == typeof(CancellationToken)) continue;
                if (pi.ParameterType != typeof(bool) && pi.ParameterType != typeof(string) && pi.ParameterType != typeof(int) && pi.ParameterType != typeof(long))
                {
                    if (pi.HasDefaultValue) continue;
                    throw new InvalidProgramException($"Parameter {pi.Name} type {pi.ParameterType} isn't supported");
                }
                yield return new()
                {
                    Title = pi.GetDisplayText(),
                    Description = pi.GetCustomAttributeCached<DescriptionAttribute>()?.Description,
                    ClrType = pi.ParameterType.ToString(),
                    DefaultValue = pi.HasDefaultValue ? pi.DefaultValue?.ToString() : null,
                    IsNullable = pi.IsNullable(nic)
                };
            }
        }

        /// <summary>
        /// User action parameter information
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        public sealed class UserActionParameterInfo()
        {
            /// <summary>
            /// Title
            /// </summary>
            public required string Title { get; init; }

            /// <summary>
            /// Description
            /// </summary>
            public string? Description { get; init; }

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
            /// Default value
            /// </summary>
            public string? DefaultValue { get; init; }

            /// <summary>
            /// If the parameter value is nullable
            /// </summary>
            public bool IsNullable { get; init; }
        }
    }
}

﻿using System.Reflection;
using System.Text.Json.Serialization;

namespace wan24.Core
{
    /// <summary>
    /// User action call
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public sealed class UserActionCall()
    {
        /// <summary>
        /// Method name
        /// </summary>
        public required string Method { get; init; }

        /// <summary>
        /// Parameters
        /// </summary>
        public required string?[] Parameters { get; init; }

        /// <summary>
        /// Provider CLR type name
        /// </summary>
        public required string ProviderType { get; init; }

        /// <summary>
        /// Provider
        /// </summary>
        [JsonIgnore]
        public Type? Provider => TypeHelper.Instance.GetType(ProviderType, throwOnError: true);

        /// <summary>
        /// Provider static dictionary field name
        /// </summary>
        public required string ProviderField { get; init; }

        /// <summary>
        /// Provider instance key
        /// </summary>
        public string ProviderKey { get; init; } = string.Empty;

        /// <summary>
        /// Execute the user action
        /// </summary>
        /// <param name="parameters">Unparsed parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task ExecuteAsync(string?[] parameters, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            // Find the object instance and the object type (for static methods only), then get the user action method
            object? instance = null;
            Type? providerType = Provider ?? throw new InvalidDataException("Failed to get the instance provider type");
            FieldInfo fi = providerType.GetFieldCached(ProviderField, BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidDataException($"Failed to find instance table field in {providerType}");
            if (fi.GetCustomAttributeCached<InstanceTableAttribute>() is null)
                throw new InvalidDataException($"{providerType}.{fi.Name} is missing the {typeof(InstanceTableAttribute)}");
            Type fieldType = InstanceTables.IsValidTableType(fi.FieldType)
                ? fi.FieldType
                : fi.FieldType.GetBaseTypes().FirstOrDefault(t => InstanceTables.IsValidTableType(t))
                    ?? throw new InvalidProgramException($"Invalid instance table field type {fi.FieldType} for {providerType}"),
                valueType = fieldType.GetGenericArguments()[1];
            MethodInfo mi;
            if (ProviderKey.Length > 0)
            {
                instance = InstanceTables.FindInstance(fi, ProviderKey);
                if (instance is null)
                    throw new InvalidOperationException("The object instance with the given key wasn't found in the instance provider table");
                if (!valueType.IsAssignableFrom(instance.GetType()))
                    throw new InvalidProgramException($"{providerType}.{fi.Name} is hosting an incompatible {instance.GetType()} instance (value type is {valueType})");
                mi = instance.GetType().GetMethodCached(Method, BindingFlags.Public | BindingFlags.Instance)
                    ?? throw new InvalidDataException("User action instance method not found");
            }
            else
            {
                mi = valueType.GetType().GetMethodCached(Method, BindingFlags.Public | BindingFlags.Static)
                    ?? throw new InvalidDataException("User action static method not found");
            }
            if (mi.GetCustomAttributeCached<UserActionAttribute>() is null)
                throw new UnauthorizedAccessException($"{valueType.GetType()}.{mi.Name} isn't an user action method");
            // Prepare execution parameters
            ParameterInfo[] pis = mi.GetParameters();
            List<object?> parameterList = new(pis.Length);
            int index = 0;
            NullabilityInfoContext nic = new();
            foreach (ParameterInfo pi in pis)
                if (pi.ParameterType == typeof(CancellationToken))
                {
                    parameterList.Add(cancellationToken);
                }
                else if (pi.ParameterType == typeof(bool) && index < parameters.Length && bool.TryParse(parameters[index], out bool boolValue))
                {
                    parameterList.Add(boolValue);
                    index++;
                }
                else if (pi.ParameterType == typeof(string) && index < parameters.Length)
                {
                    if (parameters[index] is null && !pi.IsNullable(nic))
                        throw new InvalidDataException($"Parameter {pi.Name} isn't nullable", new ArgumentNullException(pi.Name));
                    parameterList.Add(parameters[index]);
                    index++;
                }
                else if (pi.ParameterType == typeof(int) && index < parameters.Length && int.TryParse(parameters[index], out int intValue))
                {
                    parameterList.Add(intValue);
                    index++;
                }
                else if (pi.ParameterType == typeof(long) && index < parameters.Length && long.TryParse(parameters[index], out long longValue))
                {
                    parameterList.Add(longValue);
                    index++;
                }
                else if (!pi.HasDefaultValue)
                {
                    throw new InvalidDataException($"Parameter {pi.Name} is missing", new ArgumentNullException(pi.Name));
                }
                else
                {
                    parameterList.Add(pi.DefaultValue);
                }
            // Execute the method
            try
            {
                object? returnValue = mi.Invoke(instance, [.. parameterList]);
                while (returnValue is Task task)
                {
                    await task.DynamicContext();
                    returnValue = task.GetType().IsGenericType
                        ? task.GetResult(task.GetType().GetGenericArguments()[0])
                        : null;
                }
                if (returnValue is not null)
                    await returnValue.TryDisposeAsync().DynamicContext();
            }
            catch (Exception ex)
            {
                throw new AggregateException("User action execution failed", ex);
            }
        }
    }
}

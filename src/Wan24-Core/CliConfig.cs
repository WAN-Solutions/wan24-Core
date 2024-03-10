using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using static wan24.Core.Logging;

namespace wan24.Core
{
    /// <summary>
    /// CLI argument runtime configuration
    /// </summary>
    public static class CliConfig
    {
        /// <summary>
        /// CLR type name of the logger to use (with or without namespace; may be appended to the configured <see cref="LoggerBase"/>)
        /// </summary>
        [CliConfig]
        [StringLength(byte.MaxValue), Required]
        public static string LoggerType
        {
            set
            {
                // Load the type
                Type type = (value.Contains('.')
                    ? TypeHelper.Instance.GetType(value)
                    : (from ass in TypeHelper.Instance.Assemblies
                       from t in ass.GetTypes()
                       where t.CanConstruct() &&
                        typeof(ILogger).IsAssignableFrom(t) &&
                        t.Name.Equals(value, StringComparison.OrdinalIgnoreCase)
                       select t).FirstOrDefault())
                    ?? throw new ArgumentException($"Logger type not found: {value}", $"{typeof(CliConfig)}.{nameof(LoggerType)}");
                // Create and activate the logger
                ILogger logger = type.ConstructAuto() as ILogger ?? throw new ArgumentException($"Failed to instance logger {type}", $"{typeof(CliConfig)}.{nameof(LoggerType)}");
                if(Logging.Logger?.GetFinalLogger() is LoggerBase parentLogger)
                {
                    parentLogger.Next = parentLogger;
                }
                else
                {
                    ClearLogger();
                    Logging.Logger = logger;
                }
            }
        }

        /// <summary>
        /// The path to the logfile to use (file logger may be appended to the configured <see cref="LoggerBase"/>)
        /// </summary>
        [CliConfig]
        [StringLength(short.MaxValue), Required]
        public static string LogFile
        {
            set
            {
                FileLogger logger = FileLogger.CreateAsync(value).Result;
                if (Logging.Logger?.GetFinalLogger() is LoggerBase parentLogger)
                {
                    parentLogger.Next = logger;
                }
                else
                {
                    ClearLogger();
                    Logging.Logger = logger;
                }
            }
        }

        /// <summary>
        /// Log level for the last <see cref="Logging.Logger"/> (must be a <see cref="LoggerBase"/>)
        /// </summary>
        [CliConfig]
        public static LogLevel LogLevel
        {
            set
            {
                if (Logging.Logger is null) throw new InvalidOperationException("No logger configured");
                if (Logging.Logger.GetFinalLogger() is LoggerBase logger)
                {
                    logger.Level = value;
                }
                else
                {
                    throw new InvalidOperationException($"Last configured logger {Logging.Logger.GetFinalLogger().GetType()} can't be configured with another log level");
                }
            }
        }

        /// <summary>
        /// Current locale to set to <see cref="Translation.Current"/>
        /// </summary>
        [CliConfig]
        public static string Locale
        {
            set => Translation.Current = Translation.Locales[value];
        }

        /// <summary>
        /// Apply a configuration from CLI arguments to public static properties with a <see cref="CliConfigAttribute"/> attribute
        /// </summary>
        /// <param name="ca">CLI arguments</param>
        public static void Apply(CliArguments? ca = null)
        {
            string[] temp;
            if(ca is null)
            {
                temp = ENV.CliArguments;
                if (temp.Length > 0) temp = [.. temp.AsSpan(1)];
                ca = new(temp);
            }
            ReadOnlyCollection<string> values;
            string typeName,
                propertyName;
            Type? type,
                et;
            PropertyInfoExt? prop;
            Array arr;
            object? value;
            List<ValidationResult?> validationResults = [];
            ValidationContext vc = new(new object());
            foreach (string key in ca.Arguments.Keys)
            {
                if (!key.Contains('.')) continue;// Fully qualified public static property name required
                // Get type and property name
                temp = key.Split('.');
                propertyName = temp[^1];
                typeName = string.Join('.', temp[0..^1]);
                // Load the type
                type = TypeHelper.Instance.GetType(typeName);
                if (type is null) continue;
                // Load and validate the property
                prop = type.GetPropertyCached(propertyName, BindingFlags.Static | BindingFlags.Public);
                if (prop is null) continue;
                if (prop.GetCustomAttributeCached<CliConfigAttribute>() is null)
                    throw new ArgumentException($"{type}.{prop.Name} is missing the {typeof(CliConfigAttribute)}", key);
                if (prop.Setter is null) throw new InvalidProgramException($"{type}.{prop.Property.Name} has no setter");
                // Get the property value to set
                if (prop.PropertyType == typeof(bool))
                {
                    // Boolean
                    if (!ca.IsBoolean(key)) throw new ArgumentException("Value expected", key);
                    value = true;
                }
                else if (ca.IsBoolean(key))
                {
                    // Boolean required
                    throw new ArgumentException("Flag expected", key);
                }
                else if (prop.PropertyType == typeof(string))
                {
                    // String
                    if (ca.ValueCount(key) != 1) throw new ArgumentException("Single value expected", key);
                    value = ca.Single(key);
                }
                else if (prop.PropertyType == typeof(string[]))
                {
                    // String array
                    value = ca.All(key).ToArray();
                }
                else if (prop.PropertyType.IsArray)
                {
                    // Array of JSON encoded items
                    values = ca.All(key);
                    et = prop.PropertyType.GetElementType()!;
                    arr = Array.CreateInstance(et, values.Count);
                    for (int i = 0, len = values.Count; i < len; arr.SetValue(JsonHelper.DecodeObject(et, JsonHelper.MayBeJson(values[i]) ? values[i] : JsonHelper.Encode(values[i])), i), i++) ;
                    value = arr;
                }
                else
                {
                    // JSON encoded value
                    if (ca.ValueCount(key) != 1) throw new ArgumentException("Single value expected", key);
                    values = ca.All(key);
                    value = JsonHelper.DecodeObject(prop.PropertyType, JsonHelper.MayBeJson(values[0]) ? values[0] : JsonHelper.Encode(values[0]));
                }
                // Validate the value
                vc.MemberName = $"{type}.{prop.Name}";
                validationResults.Clear();
                validationResults.AddRange(prop.GetCustomAttributesCached<ValidationAttribute>().Select(a => a.GetValidationResult(value, vc)));
                if (validationResults.Any(r => r is not null))
                    throw new ArgumentException(validationResults.Where(r => r is not null).Select(r => r!.ErrorMessage ?? "Argument validation failed").First(), vc.MemberName);
                // Set the property value
                if (Trace) Logging.WriteTrace($"Setting {prop.Property.DeclaringType}.{prop.Name} value from CLI configuration");
                prop.Setter(null, value);
            }
        }

        /// <summary>
        /// Remove CLI arguments
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>Arguments</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static CliArguments RemoveCliConfigArguments(this CliArguments args)
        {
            foreach (string key in args.Arguments.Keys.Where(k => k.Contains('.') && TypeHelper.Instance.GetType(string.Join('.', k.Split('.')[0..^1])) is not null).ToArray())
                args.Arguments.Remove(key);
            return args;
        }

        /// <summary>
        /// Clear the existing logger
        /// </summary>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static void ClearLogger()
        {
            if (Logging.Logger is null) return;
            if (Debug) Logging.WriteDebug($"Overriding {typeof(Logging)}.{nameof(Logging.Logger)} ({Logging.Logger.GetType()}) from CLI");
            Logging.Logger.TryDispose();
        }
    }
}

using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// CLI argument runtime configuration
    /// </summary>
    public static class CliConfig
    {
        /// <summary>
        /// CLR type name of the logger to use (with or without namespace; may be appended to the configured logger)
        /// </summary>
        [CliConfig]
        [StringLength(byte.MaxValue), Required]
        public static string LoggerType
        {
            set
            {
                Type type = (value.Contains('.')
                    ? TypeHelper.Instance.GetType(value)
                    : (from ass in TypeHelper.Instance.Assemblies
                       from t in ass.GetTypes()
                       where t.CanConstruct() &&
                        typeof(ILogger).IsAssignableFrom(t) &&
                        t.Name.Equals(value, StringComparison.OrdinalIgnoreCase)
                       select t).FirstOrDefault())
                    ?? throw new ArgumentException($"Logger type not found: {value}", $"{typeof(CliConfig)}.{nameof(LoggerType)}");
                if (!typeof(ILogger).IsAssignableFrom(type)) throw new ArgumentException($"{type} isn't an {typeof(ILogger)}", $"{typeof(CliConfig)}.{nameof(LoggerType)}");
                ILogger logger = type.ConstructAuto() as ILogger ?? throw new ArgumentException($"Failed to instance logger {type}", $"{typeof(CliConfig)}.{nameof(LoggerType)}");
                if(Logging.Logger?.GetFinalLogger() is LoggerBase parentLogger)
                {
                    parentLogger.Next = parentLogger;
                }
                else
                {
                    Logging.Logger?.TryDispose();
                    Logging.Logger = logger;
                }
            }
        }

        /// <summary>
        /// The path to the logfile to use (file logger may be appended to the configured logger)
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
                    Logging.Logger?.TryDispose();
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
                if (Logging.Logger?.GetFinalLogger() is LoggerBase logger)
                {
                    logger.Level = value;
                }
                else
                {
                    throw new InvalidOperationException($"Configured logger {Logging.Logger!.GetType()} can't be configured with another log level");
                }
            }
        }

        /// <summary>
        /// Apply a configuration from CLI arguments to public static properties with a <see cref="CliConfigAttribute"/> attribute
        /// </summary>
        /// <param name="ca">CLI arguments</param>
        public static void Apply(CliArguments? ca = null)
        {
            ca ??= new(Environment.GetCommandLineArgs());
            string[] temp;
            ReadOnlyCollection<string> values;
            string typeName,
                propertyName;
            Type? type,
                et;
            PropertyInfoExt? prop;
            PropertyInfo pi;
            Action<object?, object?> setter;
            Array arr;
            object? value;
            List<ValidationResult?> validationResults = [];
            ValidationContext vc = new(new object());
            foreach (string key in ca.Arguments.Keys)
            {
                if (!key.Contains('.')) continue;
                temp = key.Split('.');
                propertyName = temp[^1];
                typeName = string.Join('.', temp[0..^1]);
                type = TypeHelper.Instance.GetType(typeName);
                if (type is null) continue;
                prop = type.GetPropertyCached(propertyName, BindingFlags.Static | BindingFlags.Public);
                if (prop is null) continue;
                (setter, pi) = (prop.Setter ?? throw new InvalidProgramException($"{type}.{prop.Property.Name} has no setter"), prop.Property);
                if (pi.GetCustomAttributeCached<CliConfigAttribute>() is null)
                    throw new ArgumentException($"{type}.{pi.Name} is missing the {typeof(CliConfigAttribute)}", key);
                if (pi.PropertyType == typeof(bool))
                {
                    if (!ca.IsBoolean(key)) throw new ArgumentException("Value expected", key);
                    value = true;
                }
                else if (ca.IsBoolean(key))
                {
                    throw new ArgumentException("Flag expected", key);
                }
                else if (pi.PropertyType == typeof(string))
                {
                    if (ca.ValueCount(key) != 1) throw new ArgumentException("Single value expected", key);
                    value = ca.Single(key);
                }
                else if (pi.PropertyType == typeof(string[]))
                {
                    value = ca.All(key).ToArray();
                }
                else if (pi.PropertyType.IsArray)
                {
                    values = ca.All(key);
                    et = pi.PropertyType.GetElementType()!;
                    arr = Array.CreateInstance(et, values.Count);
                    for (int i = 0, len = values.Count; i < len; arr.SetValue(JsonHelper.DecodeObject(et, values[i]), i), i++) ;
                    value = arr;
                }
                else
                {
                    if (ca.ValueCount(key) != 1) throw new ArgumentException("Single value expected", key);
                    value = JsonHelper.DecodeObject(pi.PropertyType, ca.Single(key));
                }
                vc.MemberName = $"{type}.{pi.Name}";
                validationResults.Clear();
                validationResults.AddRange(pi.GetCustomAttributesCached<ValidationAttribute>().Select(a => a.GetValidationResult(value, vc)));
                if (validationResults.Any(r => r is not null))
                    throw new ArgumentException(validationResults.Where(r => r is not null).Select(r => r!.ErrorMessage ?? "Argument validation failed").First(), vc.MemberName);
                setter(null, value);
            }
        }
    }
}

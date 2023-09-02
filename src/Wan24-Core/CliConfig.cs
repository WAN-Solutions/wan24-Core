using System.Collections.ObjectModel;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// CLI argument runtime configuration
    /// </summary>
    public static class CliConfig
    {
        /// <summary>
        /// Applya configuration from CLI arguments
        /// </summary>
        /// <param name="ca">CLI arguments</param>
        public static void Apply(in CliArguments ca)
        {
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
            foreach (string key in ca.Arguments.Keys)
            {
                if (key.IndexOf('.') < 0) continue;
                temp = key.Split('.');
                propertyName = temp[^1];
                typeName = string.Join('.', temp[0..^1]);
                type = TypeHelper.Instance.GetType(typeName);
                if (type is null) continue;
                prop = type.GetPropertyCached(propertyName, BindingFlags.Static | BindingFlags.Public);
                if (prop is null) continue;
                (setter, pi) = (prop.Setter ?? throw new InvalidProgramException($"{prop.Property.DeclaringType}.{prop.Property.Name} has no setter"), prop.Property);
                if (pi.GetCustomAttributeCached<CliConfigAttribute>() is null)
                    throw new ArgumentException($"{type}.{pi.Name} is missing the {typeof(CliConfigAttribute)}", key);
                if (pi.PropertyType == typeof(bool))
                {
                    if (!ca.IsBoolean(key)) throw new ArgumentException("Value expected", key);
                    setter(null, true);
                    continue;
                }
                if (ca.IsBoolean(key)) throw new ArgumentException("Flag expected", key);
                if (pi.PropertyType == typeof(string))
                {
                    if (ca.ValueCount(key) != 1) throw new ArgumentException("Single value expected", key);
                    setter(null, ca.Single(key));
                }
                else if (pi.PropertyType == typeof(string[]))
                {
                    setter(null, ca.All(key).ToArray());
                }
                else if (pi.PropertyType.IsArray)
                {
                    values = ca.All(key);
                    et = pi.PropertyType.GetElementType()!;
                    arr = Array.CreateInstance(et, values.Count);
                    for (int i = 0, len = values.Count; i < len; arr.SetValue(JsonHelper.DecodeObject(et, values[i]), i), i++) ;
                    setter(null, arr);
                }
                else
                {
                    if (ca.ValueCount(key) != 1) throw new ArgumentException("Single value expected", key);
                    setter(null, JsonHelper.DecodeObject(pi.PropertyType, ca.Single(key)));
                }
            }
        }
    }
}

using System.Collections.Specialized;
using System.Runtime;
using System.Web;

namespace wan24.Core
{
    /// <summary>
    /// Dictionary extensions
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Merge two dictionaries (will overwrite existing items)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="dict">Input</param>
        /// <param name="other">Source</param>
        /// <param name="prefix">Key prefix</param>
        /// <param name="existingOnly">Existing keys only?</param>
        /// <param name="overwrite">Overwrite existing keys?</param>
        /// <returns>Input</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static Dictionary<string, T> Merge<T>(
            this Dictionary<string, T> dict, 
            in Dictionary<string, T> other, 
            string? prefix = null, 
            in bool existingOnly = false,
            in bool overwrite = true
            )
        {
            prefix ??= string.Empty;
            string key;
            foreach (var kvp in other)
            {
                key = $"{prefix}{kvp.Key}";
                if (overwrite)
                {
                    if (!existingOnly || dict.ContainsKey(key)) dict[key] = kvp.Value;
                }
                else if (!dict.ContainsKey(key))
                {
                    dict[key] = kvp.Value;
                }
            }
            return dict;
        }

        /// <summary>
        /// Merge an enumerable (with the item index as key)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="dict">Input</param>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="prefix">Key prefix</param>
        /// <param name="existingOnly">Existing keys only?</param>
        /// <param name="overwrite">Overwrite existing keys?</param>
        /// <returns>Input</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static Dictionary<string, T> Merge<T>(
            this Dictionary<string, T> dict, 
            in IEnumerable<T> enumerable, 
            string? prefix = null, 
            in bool existingOnly = false,
            in bool overwrite = true
            )
        {
            prefix ??= string.Empty;
            int index = 0;
            string key;
            foreach (T item in enumerable)
            {
                key = $"{prefix}{index}";
                if (overwrite)
                {
                    if (!existingOnly || dict.ContainsKey(key)) dict[key] = item;
                }
                else if (!dict.ContainsKey(key))
                {
                    dict[key] = item;
                }
                index++;
            }
            return dict;
        }

        /// <summary>
        /// Find differences between two dictionaries (different keys and values)
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="original">Original</param>
        /// <param name="other">Other (values which mismatch the source count)</param>
        /// <param name="existingOriginalKeysOnly">Existing keys (in the original) only?</param>
        /// <param name="existingOtherKeysOnly">Existing keys (in the other) only?</param>
        /// <returns>New dictionary which contains the different key/values</returns>
        public static Dictionary<tKey, tValue> Diff<tKey, tValue>(
            this Dictionary<tKey, tValue> original, 
            in Dictionary<tKey, tValue> other, 
            in bool existingOriginalKeysOnly = false,
            in bool existingOtherKeysOnly = false
            )
            where tKey : notnull
        {
            Dictionary<tKey, tValue> res = [];
            HashSet<tKey> seen = [];
            foreach (var kvp in original)
            {
                bool exists = other.TryGetValue(kvp.Key, out tValue? value);
                if (!exists && existingOtherKeysOnly) continue;
                if (!exists || (kvp.Value is null != value is null) || !(kvp.Value?.Equals(value) ?? true))
                {
                    res[kvp.Key] = value!;
                    seen.Add(kvp.Key);
                }
            }
            foreach (var kvp in other.Where(kvp => !seen.Contains(kvp.Key)))
            {
                bool exists = original.TryGetValue(kvp.Key, out tValue? value);
                if (!exists && existingOriginalKeysOnly) continue;
                if (!exists || (kvp.Value is null != value is null) || !(kvp.Value?.Equals(value) ?? true))
                    res[kvp.Key] = value!;
            }
            return res;
        }

        /// <summary>
        /// Get as query string (http GET request parameters)
        /// </summary>
        /// <param name="dict">Dictionary</param>
        /// <returns>Query string</returns>
        public static string AsQueryString(this Dictionary<string, string> dict)
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString(string.Empty);
            foreach (var kvp in dict) nvc.Add(kvp.Key, kvp.Value);
            return nvc.ToString()!;
        }
    }
}

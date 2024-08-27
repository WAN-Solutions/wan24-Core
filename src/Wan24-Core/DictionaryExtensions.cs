using System.Collections.Frozen;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
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
        /// <param name="existingOriginalKeysOnly">Existing keys (in the original) only? (the value of the result will be <see langword="null"/>, if the key wasn't 
        /// found in the <c>other</c>)</param>
        /// <param name="existingOtherKeysOnly">Existing keys (in the other) only? (the value of the result will be <see langword="null"/>, if the key wasn't found in the 
        /// <c>original</c>)</param>
        /// <returns>New dictionary which contains the different key/values (if the value is not <see langword="null"/>, it's the <c>other</c> different value)</returns>
        public static Dictionary<tKey, tValue?> Diff<tKey, tValue>(
            this Dictionary<tKey, tValue> original, 
            in Dictionary<tKey, tValue> other, 
            in bool existingOriginalKeysOnly = false,
            in bool existingOtherKeysOnly = false
            )
            where tKey : notnull
        {
            Dictionary<tKey, tValue?> res = [];
            HashSet<tKey> seen = [];
            foreach (var kvp in original)
            {
                bool exists = other.TryGetValue(kvp.Key, out tValue? value);
                if (!exists && existingOtherKeysOnly) continue;
                if (!exists || (kvp.Value is null != value is null) || !(kvp.Value?.Equals(value) ?? true))
                {
                    res[kvp.Key] = value;
                    seen.Add(kvp.Key);
                }
            }
            foreach (var kvp in other.Where(kvp => !seen.Contains(kvp.Key)))
            {
                bool exists = original.TryGetValue(kvp.Key, out tValue? value);
                if (!exists && existingOriginalKeysOnly) continue;
                if (!exists || (kvp.Value is null != value is null) || !(kvp.Value?.Equals(value) ?? true))
                    res[kvp.Key] = value;
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

        /// <summary>
        /// Determine if a value is contained
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="value">Value</param>
        /// <returns>If the value is contained</returns>
        public static bool ContainsValue<tKey, tValue>(this IDictionary<tKey, tValue> dict, tValue value) where tKey : notnull
            => dict.Values.Any(v => (v is null && value is null) || (v is not null && v.Equals(value)));

        /// <summary>
        /// Determine if a value is contained
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="value">Value</param>
        /// <returns>If the value is contained</returns>
        public static bool ContainsValue<tKey, tValue>(this IReadOnlyDictionary<tKey, tValue> dict, tValue value) where tKey : notnull
            => dict.Values.Any(v => (v is null && value is null) || (v is not null && v.Equals(value)));

        /// <summary>
        /// Determine if a value is contained
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="value">Value</param>
        /// <returns>If the value is contained</returns>
        public static bool ContainsValue<tKey, tValue>(this FrozenDictionary<tKey, tValue> dict, tValue value) where tKey : notnull
            => dict.Values.Any(v => (v is null && value is null) || (v is not null && v.Equals(value)));

        /// <summary>
        /// Get the first key of a value
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="value">Value</param>
        /// <returns>Key</returns>
        /// <exception cref="KeyNotFoundException">Key not found</exception>
        public static tKey GetKeyOfValue<tKey, tValue>(this IDictionary<tKey, tValue> dict, in tValue value) where tKey : notnull
        {
            if (!TryGetKeyOfValue(dict, value, out tKey? res))
                throw new KeyNotFoundException();
            return res;
        }

        /// <summary>
        /// Get the first key of a value
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="value">Value</param>
        /// <returns>Key</returns>
        /// <exception cref="KeyNotFoundException">Key not found</exception>
        public static tKey GetKeyOfValue<tKey, tValue>(this FrozenDictionary<tKey, tValue> dict, in tValue value) where tKey : notnull
        {
            if (!TryGetKeyOfValue(dict, value, out tKey? res))
                throw new KeyNotFoundException();
            return res;
        }

        /// <summary>
        /// Get the first key of a value
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="value">Value</param>
        /// <returns>Key</returns>
        /// <exception cref="KeyNotFoundException">Key not found</exception>
        public static tKey GetKeyOfValue<tKey, tValue>(this IReadOnlyDictionary<tKey, tValue> dict, in tValue value) where tKey : notnull
        {
            if (!TryGetKeyOfValue(dict, value, out tKey? res))
                throw new KeyNotFoundException();
            return res;
        }

        /// <summary>
        /// Get the first key of a value
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="value">Value</param>
        /// <param name="key">Found key</param>
        /// <returns>If succeed</returns>
        public static bool TryGetKeyOfValue<tKey, tValue>(this IDictionary<tKey, tValue> dict, in tValue value, [MaybeNullWhen(returnValue: false)] out tKey key) where tKey : notnull
        {
            foreach (KeyValuePair<tKey, tValue> kvp in dict)
                if (kvp.Value?.Equals(value) ?? value is null)
                {
                    key = kvp.Key;
                    return true;
                }
            key = default;
            return false;
        }

        /// <summary>
        /// Get the first key of a value
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="value">Value</param>
        /// <param name="key">Found key</param>
        /// <returns>If succeed</returns>
        public static bool TryGetKeyOfValue<tKey, tValue>(this FrozenDictionary<tKey, tValue> dict, in tValue value, [MaybeNullWhen(returnValue: false)] out tKey key) where tKey : notnull
        {
            foreach (KeyValuePair<tKey, tValue> kvp in dict)
                if (kvp.Value?.Equals(value) ?? value is null)
                {
                    key = kvp.Key;
                    return true;
                }
            key = default;
            return false;
        }

        /// <summary>
        /// Get the first key of a value
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="value">Value</param>
        /// <param name="key">Found key</param>
        /// <returns>If succeed</returns>
        public static bool TryGetKeyOfValue<tKey, tValue>(this IReadOnlyDictionary<tKey, tValue> dict, in tValue value, [MaybeNullWhen(returnValue: false)] out tKey key) where tKey : notnull
        {
            foreach (KeyValuePair<tKey, tValue> kvp in dict)
                if (kvp.Value?.Equals(value) ?? value is null)
                {
                    key = kvp.Key;
                    return true;
                }
            key = default;
            return false;
        }

        /// <summary>
        /// Get the keys of a value
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="value">Value</param>
        /// <returns>Keys</returns>
        public static IEnumerable<tKey> GetKeysOfValue<tKey, tValue>(this IDictionary<tKey, tValue> dict, tValue value) where tKey : notnull
        {
            foreach (KeyValuePair<tKey, tValue> kvp in dict)
                if (kvp.Value?.Equals(value) ?? value is null)
                    yield return kvp.Key;
        }

        /// <summary>
        /// Get the keys of a value
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="value">Value</param>
        /// <returns>Keys</returns>
        public static IEnumerable<tKey> GetKeysOfValue<tKey, tValue>(this FrozenDictionary<tKey, tValue> dict, tValue value) where tKey : notnull
        {
            foreach (KeyValuePair<tKey, tValue> kvp in dict)
                if (kvp.Value?.Equals(value) ?? value is null)
                    yield return kvp.Key;
        }

        /// <summary>
        /// Get the keys of a value
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="value">Value</param>
        /// <returns>Keys</returns>
        public static IEnumerable<tKey> GetKeysOfValue<tKey, tValue>(this IReadOnlyDictionary<tKey, tValue> dict, tValue value) where tKey : notnull
        {
            foreach (KeyValuePair<tKey, tValue> kvp in dict)
                if (kvp.Value?.Equals(value) ?? value is null)
                    yield return kvp.Key;
        }
    }
}

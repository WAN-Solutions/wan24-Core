﻿using System.Runtime;

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
        /// <returns>Input</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static Dictionary<string, T> Merge<T>(this Dictionary<string, T> dict, in Dictionary<string, T> other, string? prefix = null, in bool existingOnly = false)
        {
            prefix ??= string.Empty;
            foreach (var kvp in other) if (!existingOnly || dict.ContainsKey($"{prefix}{kvp.Key}")) dict[$"{prefix}{kvp.Key}"] = kvp.Value;
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
        /// <returns>Input</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static Dictionary<string, T> Merge<T>(this Dictionary<string, T> dict, in IEnumerable<T> enumerable, string? prefix = null, in bool existingOnly = false)
        {
            prefix ??= string.Empty;
            int index = 0;
            foreach (T item in enumerable)
            {
                if (!existingOnly || dict.ContainsKey($"{prefix}{index}"))
                    dict[$"{prefix}{index}"] = item;
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
    }
}

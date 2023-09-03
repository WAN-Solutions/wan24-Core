using System.Runtime;

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
        /// <returns>Input</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static Dictionary<string, T> Merge<T>(this Dictionary<string, T> dict, in Dictionary<string, T> other, string? prefix = null)
        {
            prefix ??= string.Empty;
            foreach (var kvp in other) dict[$"{prefix}{kvp.Key}"] = kvp.Value;
            return dict;
        }

        /// <summary>
        /// Merge an enumerable (with the item index as key)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="dict">Input</param>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="prefix">Key prefix</param>
        /// <returns>Input</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static Dictionary<string, T> Merge<T>(this Dictionary<string, T> dict, in IEnumerable<T> enumerable, string? prefix = null)
        {
            prefix ??= string.Empty;
            int index = 0;
            foreach (T item in enumerable)
            {
                dict[$"{prefix}{index}"] = item;
                index++;
            }
            return dict;
        }
    }
}

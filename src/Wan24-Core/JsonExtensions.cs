using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// JSON extensions
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        /// Get as JSON string
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="prettify">Prettify?</param>
        /// <returns>JSON string</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToJson(this object obj, in bool prettify = false) => JsonHelper.Encode(obj, prettify);

        /// <summary>
        /// Decode a JSON string
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="json">JSON string</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object? FromJson(this Type type, in string json) => JsonHelper.DecodeObject(type, json);

        /// <summary>
        /// Decode JSON
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="json">JSON string</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T? DecodeJson<T>(this string json) => JsonHelper.Decode<T>(json);

        /// <summary>
        /// Get a value from a deep JSON dictionary (an object)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="jsonDict">JSON dictionary</param>
        /// <param name="path">Path</param>
        /// <param name="pathSeparator">Path property name separator</param>
        /// <returns>Value (<see langword="null"/>, if the path doesn't exist or the value type doesn't match)</returns>
        /// <exception cref="ArgumentException">Invalid path</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public static T? GetJsonValue<T>(this IReadOnlyDictionary<string, object?> jsonDict, in string path, in char pathSeparator = '.')
        {
            if (path.Length < 1) throw new ArgumentException("Path required", nameof(path));
            string[] parts = path.Split(pathSeparator);
            object? current = jsonDict;
            for (int i = 0, len = parts.Length; i < len; i++)
            {
                if (parts[i].Length < 1) throw new ArgumentException($"Invalid path element #{i}", nameof(path));
                if (current is not IReadOnlyDictionary<string, object?> dict)
                {
                    current = null;
                    break;
                }
                dict.TryGetValue(parts[i], out current);
            }
            return current is T res ? res : default(T?);
        }

        /// <summary>
        /// Get a value from a deep JSON dictionary (an object)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="jsonDict">JSON dictionary</param>
        /// <param name="path">Path (dot separated property names)</param>
        /// <param name="result">Value</param>
        /// <param name="pathSeparator">Path property name separator</param>
        /// <returns>If the value was found (anyway, the value may be <see langword="null"/>)</returns>
        /// <exception cref="ArgumentException">Invalid path</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool TryGetJsonValue<T>(this IReadOnlyDictionary<string, object?> jsonDict, in string path, out T? result, in char pathSeparator = '.')
        {
            if (path.Length < 1) throw new ArgumentException("Path required", nameof(path));
            string[] parts = path.Split(pathSeparator);
            object? current = jsonDict;
            bool res = true;
            for (int i = 0, len = parts.Length; i < len; i++)
            {
                if (parts[i].Length < 1) throw new ArgumentException($"Invalid path element #{i}", nameof(path));
                if (current is not IReadOnlyDictionary<string, object?> dict)
                {
                    current = null;
                    res = false;
                    break;
                }
                dict.TryGetValue(parts[i], out current);
            }
            result = current is T value ? value : default(T?);
            return res;
        }

        /// <summary>
        /// Determine if a deep JSON dictionary (an object) contains a value
        /// </summary>
        /// <param name="jsonDict">JSON dictionary</param>
        /// <param name="path">Path (dot separated property names)</param>
        /// <param name="pathSeparator">Path property name separator</param>
        /// <returns>If the given path has a value</returns>
        /// <exception cref="ArgumentException">Invalid path</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool HasJsonValue(this IReadOnlyDictionary<string, object?> jsonDict, in string path, in char pathSeparator = '.')
        {
            if (path.Length < 1) throw new ArgumentException("Path required", nameof(path));
            string[] parts = path.Split(pathSeparator);
            object? current = jsonDict;
            bool res = true;
            for (int i = 0, len = parts.Length; i < len; i++)
            {
                if (parts[i].Length < 1) throw new ArgumentException($"Invalid path element #{i}", nameof(path));
                if (current is not IReadOnlyDictionary<string, object?> dict)
                {
                    res = false;
                    break;
                }
                dict.TryGetValue(parts[i], out current);
            }
            return res;
        }

        /// <summary>
        /// Set a value in a deep JSON dictionary (an object; missing sub-objects will be created)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="jsonDict">JSON dictionary</param>
        /// <param name="path">Path (dot separated property names)</param>
        /// <param name="pathSeparator">Path property name separator</param>
        /// <param name="value">Value to set to the path</param>
        /// <returns>JSON dictionary</returns>
        /// <exception cref="ArgumentException">An element in the path is not a JSON dictionary, or the path is invalid</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public static IDictionary<string, object?> SetJsonValue<T>(this IDictionary<string, object?> jsonDict, in string path, in T value, in char pathSeparator = '.')
        {
            if (path.Length < 1) throw new ArgumentException("Path required", nameof(path));
            string[] parts = path.Split(pathSeparator);
            object? current = jsonDict;
            IDictionary<string, object?> last = jsonDict;
            for (int i = 0, len = parts.Length - 1; i < len; i++)
            {
                if (parts[i].Length < 1) throw new ArgumentException($"Invalid path element #{i}", nameof(path));
                if (current is not IDictionary<string, object?> dict)
                    throw new ArgumentException($"Path \"{string.Join(pathSeparator, parts.Take(i))}\" is not a JSON dictionary", nameof(path));
                last = dict;
                if (!last.TryGetValue(parts[i], out current)) current = last[parts[i]] = new Dictionary<string, object?>();
            }
            last[parts[^1]] = value;
            return jsonDict;
        }

        /// <summary>
        /// Set a value in a deep JSON dictionary (an object; missing sub-objects will be created)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="jsonDict">JSON dictionary</param>
        /// <param name="path">Path (dot separated property names)</param>
        /// <param name="pathSeparator">Path property name separator</param>
        /// <param name="value">Value to set to the path</param>
        /// <returns>If the value was set</returns>
        /// <exception cref="ArgumentException">Invalid path</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool TrySetJsonValue<T>(this IDictionary<string, object?> jsonDict, in string path, in T value, in char pathSeparator = '.')
        {
            if (path.Length < 1) throw new ArgumentException("Path required", nameof(path));
            string[] parts = path.Split(pathSeparator);
            object? current = jsonDict;
            IDictionary<string, object?> last = jsonDict;
            for (int i = 0, len = parts.Length - 1; i < len; i++)
            {
                if (parts[i].Length < 1) throw new ArgumentException($"Invalid path element #{i}", nameof(path));
                if (current is not IDictionary<string, object?> dict) return false;
                last = dict;
                if (!last.TryGetValue(parts[i], out current)) current = last[parts[i]] = new Dictionary<string, object?>();
            }
            last[parts[^1]] = value;
            return true;
        }

        /// <summary>
        /// Create an object from a JSON dictionary
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="jsonDict">JSON dictionary</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T ToObject<T>(this IReadOnlyDictionary<string, object?> jsonDict)
        {
            using MemoryPoolStream ms = new();
            JsonHelper.Encode(jsonDict, ms);
            ms.Position = 0;
            return JsonHelper.Decode<T>(ms) ?? throw new InvalidDataException($"Failed to create a {typeof(T)} from a JSON dictionary");
        }

        /// <summary>
        /// Create an object from a JSON dictionary
        /// </summary>
        /// <param name="jsonDict">JSON dictionary</param>
        /// <param name="type">Object type</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object ToObject(this IReadOnlyDictionary<string, object?> jsonDict, in Type type)
        {
            using MemoryPoolStream ms = new();
            JsonHelper.Encode(jsonDict, ms);
            ms.Position = 0;
            return JsonHelper.DecodeObject(type, ms) ?? throw new InvalidDataException($"Failed to create a {type} from a JSON dictionary");
        }
    }
}

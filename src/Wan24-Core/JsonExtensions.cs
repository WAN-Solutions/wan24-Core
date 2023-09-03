using System.Runtime;

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
        public static string ToJson(this object obj, in bool prettify = false) => JsonHelper.Encode(obj, prettify);

        /// <summary>
        /// Decode a JSON string
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="json">JSON string</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object? FromJson(this Type type, in string json) => JsonHelper.DecodeObject(type, json);

        /// <summary>
        /// Decode JSON
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="json">JSON string</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static T? DecodeJson<T>(this string json) => JsonHelper.Decode<T>(json);
    }
}

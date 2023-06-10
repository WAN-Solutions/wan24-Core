using System.Runtime;
using System.Text.Json;

namespace wan24.Core
{
    /// <summary>
    /// JSON helper
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// JSON encoder
        /// </summary>
        public static Encoder_Delegate Encoder { get; set; } = (obj, prettify) => JsonSerializer.Serialize(obj, obj?.GetType() ?? typeof(string), new JsonSerializerOptions()
        {
            WriteIndented = prettify
        });

        /// <summary>
        /// JSON decoder
        /// </summary>
        public static Decoder_Delegate Decoder { get; set; } = (type, json) => JsonSerializer.Deserialize(json, type);

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="prettify">Prettify?</param>
        /// <returns>JSON string</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string Encode(object? obj, bool prettify = false) => Encoder(obj, prettify);

        /// <summary>
        /// Decode
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="json">JSON string</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static T? Decode<T>(string json) => (T?)DecodeObject(typeof(T), json);

        /// <summary>
        /// Decode an object
        /// </summary>
        /// <param name="type">Expected type</param>
        /// <param name="json">JSON string</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object? DecodeObject(Type type, string json) => Decoder(type, json);

        /// <summary>
        /// Delegate for a JSON encoder
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="prettify">Prettify?</param>
        /// <returns>JSON string</returns>
        public delegate string Encoder_Delegate(object? obj, bool prettify);

        /// <summary>
        /// Delegate for a JSON decoder
        /// </summary>
        /// <param name="type">Expected type</param>
        /// <param name="json">JSON string</param>
        /// <returns>Result</returns>
        public delegate object? Decoder_Delegate(Type type, string json);
    }
}

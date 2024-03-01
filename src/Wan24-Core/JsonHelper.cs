using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace wan24.Core
{
    /// <summary>
    /// JSON helper
    /// </summary>
    public static partial class JsonHelper
    {
        /// <summary>
        /// JSON <see langword="null"/> value
        /// </summary>
        public const string NULL = "null";
        /// <summary>
        /// JSON <see langword="true"/> value
        /// </summary>
        public const string TRUE = "true";
        /// <summary>
        /// JSON <see langword="false"/> value
        /// </summary>
        public const string FALSE = "false";

        /// <summary>
        /// Non-intending options
        /// </summary>
        private static readonly JsonSerializerOptions NotIntendedOptions = new();
        /// <summary>
        /// Intending options
        /// </summary>
        private static readonly JsonSerializerOptions IntendedOptions = new()
        {
            WriteIndented = true
        };
        /// <summary>
        /// Decoder options
        /// </summary>
        private static readonly JsonSerializerOptions DecoderOptions;
        /// <summary>
        /// Regular expression to match a JSON integer value
        /// </summary>
        private static readonly Regex RxJsonInt = RxJsonInt_Generator();
        /// <summary>
        /// Regular expression to match a JSON float value
        /// </summary>
        private static readonly Regex RxJsonFloat = RxJsonFloat_Generator();
        /// <summary>
        /// Regular expression to match a JSON string value
        /// </summary>
        private static readonly Regex RxJsonString = RxJsonString_Generator();
        /// <summary>
        /// Regular expression to match a JSON object value
        /// </summary>
        private static readonly Regex RxJsonObject = RxJsonObject_Generator();
        /// <summary>
        /// Regular expression to match a JSON array value
        /// </summary>
        private static readonly Regex RxJsonArray = RxJsonArray_Generator();

        /// <summary>
        /// Constructor
        /// </summary>
        static JsonHelper()
        {
            DecoderOptions = new();
            DecoderOptions.Converters.Add(new JsonStringEnumConverter());
        }

        /// <summary>
        /// JSON encoder
        /// </summary>
        public static Encoder_Delegate Encoder { get; set; }
            = (obj, prettify) => JsonSerializer.Serialize(obj, obj?.GetType() ?? typeof(string), prettify ? IntendedOptions : NotIntendedOptions);

        /// <summary>
        /// JSON encoder
        /// </summary>
        public static StreamEncoder_Delegate StreamEncoder { get; set; }
            = (obj, stream, prettify) => JsonSerializer.Serialize(stream, obj, obj?.GetType() ?? typeof(string), prettify ? IntendedOptions : NotIntendedOptions);

        /// <summary>
        /// JSON encoder
        /// </summary>
        public static EncoderAsync_Delegate EncoderAsync { get; set; }
            = async (obj, target, prettify, ct) =>
            {
                if (target is null)
                {
                    using MemoryPoolStream ms = new();
                    await JsonSerializer.SerializeAsync(ms, obj, obj?.GetType() ?? typeof(string), prettify ? IntendedOptions : NotIntendedOptions, ct).DynamicContext();
                    using RentedArrayStructSimple<byte> buffer = new((int)ms.Length, clean: false);
                    ms.Position = 0;
                    ms.ReadExactly(buffer.Span);
                    return Encoding.UTF8.GetString(buffer.Span);
                }
                await JsonSerializer.SerializeAsync(target, obj, obj?.GetType() ?? typeof(string), prettify ? IntendedOptions : NotIntendedOptions, ct).DynamicContext();
                return string.Empty;
            };

        /// <summary>
        /// JSON decoder
        /// </summary>
        public static Decoder_Delegate Decoder { get; set; } = (type, json) => JsonSerializer.Deserialize(json, type, DecoderOptions);

        /// <summary>
        /// JSON decoder
        /// </summary>
        public static StreamDecoder_Delegate StreamDecoder { get; set; } = (type, stream) => JsonSerializer.Deserialize(stream, type, DecoderOptions);

        /// <summary>
        /// JSON decoder
        /// </summary>
        public static StringDecoderAsync_Delegate StringDecoderAsync { get; set; }
            = async (type, json, ct) =>
            {
                using RentedArrayStructSimple<byte> buffer = new(Encoding.UTF8.GetByteCount(json), clean: false);
                using MemoryStream ms = new(buffer.Array);
                return await JsonSerializer.DeserializeAsync(ms, type, DecoderOptions, ct).DynamicContext();
            };

        /// <summary>
        /// JSON decoder
        /// </summary>
        public static StreamDecoderAsync_Delegate StreamDecoderAsync { get; set; }
            = async (type, json, ct) => await JsonSerializer.DeserializeAsync(json, type, DecoderOptions, ct).DynamicContext();

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="prettify">Prettify?</param>
        /// <returns>JSON string</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string Encode(in object? obj, in bool prettify = false) => Encoder(obj, prettify);

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="target">Target stream</param>
        /// <param name="prettify">Prettify?</param>
        /// <returns>JSON string</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void Encode(in object? obj, in Stream target, in bool prettify = false) => StreamEncoder(obj, target, prettify);

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="target">Target stream</param>
        /// <param name="prettify">Prettify?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>JSON string or empty, when <c>target</c> was given</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static Task<string> EncodeAsync(object? obj, Stream? target = null, bool prettify = false, CancellationToken cancellationToken = default)
            => EncoderAsync(obj, target, prettify, cancellationToken);

        /// <summary>
        /// Decode
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="json">JSON string</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static T? Decode<T>(in string json) => (T?)DecodeObject(typeof(T), json);

        /// <summary>
        /// Decode
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="source">Source stream</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static T? Decode<T>(in Stream source) => (T?)StreamDecoder(typeof(T), source);

        /// <summary>
        /// Decode
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="json">JSON string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static async Task<T?> DecodeAsync<T>(string json, CancellationToken cancellationToken = default)
            => (T?)await DecodeObjectAsync(typeof(T), json, cancellationToken).DynamicContext();

        /// <summary>
        /// Decode
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="json">JSON stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static async Task<T?> DecodeAsync<T>(Stream json, CancellationToken cancellationToken = default)
            => (T?)await DecodeObjectAsync(typeof(T), json, cancellationToken).DynamicContext();

        /// <summary>
        /// Decode an object
        /// </summary>
        /// <param name="type">Expected type</param>
        /// <param name="json">JSON string</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object? DecodeObject(in Type type, in string json) => Decoder(type, json);

        /// <summary>
        /// Decode an object
        /// </summary>
        /// <param name="type">Expected type</param>
        /// <param name="source">Source stream</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object? DecodeObject(in Type type, in Stream source) => StreamDecoder(type, source);

        /// <summary>
        /// Decode an object
        /// </summary>
        /// <param name="type">Expected type</param>
        /// <param name="json">JSON string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static Task<object?> DecodeObjectAsync(Type type, string json, CancellationToken cancellationToken = default)
            => StringDecoderAsync(type, json, cancellationToken);

        /// <summary>
        /// Decode an object
        /// </summary>
        /// <param name="type">Expected type</param>
        /// <param name="json">JSON stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static Task<object?> DecodeObjectAsync(Type type, Stream json, CancellationToken cancellationToken = default)
            => StreamDecoderAsync(type, json, cancellationToken);

        /// <summary>
        /// Determine if a string may be JSON
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>If the string may be JSON (if <see langword="false"/>, it's not proper JSON for sure - if <see langword="true"/>, there is a possibility for false positives!)</returns>
        public static bool MayBeJson(this string json) => !string.IsNullOrWhiteSpace(json) && RegularExpressions.RX_JSON.IsMatch(json);

        /// <summary>
        /// Determine if a string is a JSON <see langword="null"/> value
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Is <see langword="null"/>?</returns>
        public static bool IsJsonNull(this string json) => json.Equals(NULL, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Determine if a string is a JSON <see cref="bool"/> value
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Is <see cref="bool"/>?</returns>
        public static bool IsJsonBoolean(this string json) => json.Trim().ToLower().In([TRUE, FALSE]);

        /// <summary>
        /// Determine if a string is a JSON <see cref="int"/> value
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Is <see cref="int"/>?</returns>
        public static bool IsJsonInt(this string json) => RxJsonInt.IsMatch(json);

        /// <summary>
        /// Determine if a string is a JSON <see cref="float"/> value
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Is <see cref="float"/>?</returns>
        public static bool IsJsonFloat(this string json) => RxJsonFloat.IsMatch(json);

        /// <summary>
        /// Determine if a string is a JSON numeric (<see cref="int"/>/<see cref="float"/>) value
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Is numeric?</returns>
        public static bool IsJsonNumeric(this string json) => IsJsonInt(json) || IsJsonFloat(json);

        /// <summary>
        /// Determine if a string is a JSON <see cref="float"/> value
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Is an <see cref="float"/> (may return false positives)?</returns>
        public static bool IsJsonString(this string json) => RxJsonString.IsMatch(json);

        /// <summary>
        /// Determine if a string is a JSON object value
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Is an object (may return false positives)?</returns>
        public static bool IsJsonObject(this string json) => RxJsonObject.IsMatch(json);

        /// <summary>
        /// Determine if a string is a JSON array value
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Is an array (may return false positives)?</returns>
        public static bool IsJsonArray(this string json) => RxJsonArray.IsMatch(json);

        /// <summary>
        /// Delegate for a JSON encoder
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="prettify">Prettify?</param>
        /// <returns>JSON string</returns>
        public delegate string Encoder_Delegate(object? obj, bool prettify);

        /// <summary>
        /// Delegate for a JSON encoder
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="target">Target stream</param>
        /// <param name="prettify">Prettify?</param>
        public delegate void StreamEncoder_Delegate(object? obj, Stream target, bool prettify);

        /// <summary>
        /// Delegate for a JSON encoder
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="target">Target stream</param>
        /// <param name="prettify">Prettify?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>JSON string or empty if <c>target</c> was given</returns>
        public delegate Task<string> EncoderAsync_Delegate(object? obj, Stream? target, bool prettify, CancellationToken cancellationToken);

        /// <summary>
        /// Delegate for a JSON decoder
        /// </summary>
        /// <param name="type">Expected type</param>
        /// <param name="json">JSON string</param>
        /// <returns>Result</returns>
        public delegate object? Decoder_Delegate(Type type, string json);

        /// <summary>
        /// Delegate for a JSON decoder
        /// </summary>
        /// <param name="type">Expected type</param>
        /// <param name="stream">Stream</param>
        /// <returns>Result</returns>
        public delegate object? StreamDecoder_Delegate(Type type, Stream stream);

        /// <summary>
        /// Delegate for a JSON decoder
        /// </summary>
        /// <param name="type">Expected type</param>
        /// <param name="json">JSON string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public delegate Task<object?> StringDecoderAsync_Delegate(Type type, string json, CancellationToken cancellationToken);

        /// <summary>
        /// Delegate for a JSON decoder
        /// </summary>
        /// <param name="type">Expected type</param>
        /// <param name="source">JSON stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public delegate Task<object?> StreamDecoderAsync_Delegate(Type type, Stream source, CancellationToken cancellationToken);

        /// <summary>
        /// Regular expression to match a JSON integer value
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"^\s*\d+\s*$", RegexOptions.Singleline | RegexOptions.Compiled)]
        private static partial Regex RxJsonInt_Generator();

        /// <summary>
        /// Regular expression to match a JSON float value
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"^\s*\d+(\.\d+)?\s*$", RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex RxJsonFloat_Generator();

        /// <summary>
        /// Regular expression to match a JSON string value
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex("^\\s*\\\"[^\\n]*\\\"\\s*$", RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex RxJsonString_Generator();

        /// <summary>
        /// Regular expression to match a JSON object value
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"^\s*\{.*\}\s*$", RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex RxJsonObject_Generator();

        /// <summary>
        /// Regular expression to match a JSON array value
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"^\s*\[.*\]\s*$", RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex RxJsonArray_Generator();
    }
}

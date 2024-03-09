namespace wan24.Core
{
    // Delegates
    public static partial class JsonHelper
    {
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
    }
}

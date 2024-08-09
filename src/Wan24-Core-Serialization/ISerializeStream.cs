namespace wan24.Core
{
    /// <summary>
    /// Interface for types that can (de)serialize to/from a <see cref="Stream"/>
    /// </summary>
    public interface ISerializeStream
    {
        /// <summary>
        /// Serialize to a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="options">Options</param>
        void SerializeTo(Stream stream, SerializerOptions? options);
        /// <summary>
        /// Serialize to a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SerializeToAsync(Stream stream, SerializerOptions? options, CancellationToken cancellationToken);
        /// <summary>
        /// Deserialize from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Serializer version number</param>
        /// <param name="options">Options</param>
        void DeserializeFrom(Stream stream, int version, DeserializerOptions? options);
        /// <summary>
        /// Deserialize from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Serializer version number</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeserializeFromAsync(Stream stream, int version, DeserializerOptions? options, CancellationToken cancellationToken);
    }
}

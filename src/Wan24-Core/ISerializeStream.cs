namespace wan24.Core
{
    /// <summary>
    /// Interface for an object which (de)serializes to/from a <see cref="Stream"/>
    /// </summary>
    public interface ISerializeStream
    {
        /// <summary>
        /// Get the serialized bytes of this instance
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of bytes written to the buffer</returns>
        virtual int GetBytes(in Memory<byte> buffer)
        {
            using MemoryPoolStream ms = new(new MemoryOwner<byte>(buffer), returnData: false);
            using LimitedLengthStream lengthLimited = new(ms, buffer.Length);
            SerializeTo(lengthLimited);
            return (int)ms.Position;
        }
        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="stream">Target stream</param>
        void SerializeTo(in Stream stream);
        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="stream">Target stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SerializeToAsync(Stream stream, CancellationToken cancellationToken = default);
        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        void DeserializeFrom(in Stream stream, in int version);
        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeserializeFromAsync(Stream stream, int version, CancellationToken cancellationToken = default);
        /// <summary>
        /// Deserialize from serialized bytes
        /// </summary>
        /// <param name="data">Serialized bytes</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Instance</returns>
        public static abstract ISerializeStream ObjectFromBytes(in ReadOnlyMemory<byte> data, in int version);
        /// <summary>
        /// Deserialize from serialized bytes
        /// </summary>
        /// <typeparam name="T">Interface implementing type</typeparam>
        /// <param name="data">Serialized bytes</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Instance</returns>
        public static ISerializeStream ObjectFromBytes<T>(in ReadOnlyMemory<byte> data, in int version) where T : ISerializeStream => T.ObjectFromBytes(data, version);
    }

    /// <summary>
    /// Interface for an object which (de)serializes to/from a <see cref="Stream"/>
    /// </summary>
    /// <typeparam name="T">Serializable object type</typeparam>
    public interface ISerializeStream<T> : ISerializeStream where T : ISerializeStream<T>
    {
        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        public static abstract T Deserialize(in Stream stream, in int version);
        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static abstract Task<T> DeserializeAsync(Stream stream, int version, CancellationToken cancellationToken = default);
        /// <summary>
        /// Deserialize
        /// </summary>
        /// <typeparam name="tType">Interface implementing type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        public static T Deserialize<tType>(in Stream stream, in int version) where tType : ISerializeStream<T> => tType.Deserialize(stream, version);
        /// <summary>
        /// Deserialize
        /// </summary>
        /// <typeparam name="tType">Interface implementing type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static Task<T> DeserializeAsync<tType>(Stream stream, int version, CancellationToken cancellationToken = default) where tType : ISerializeStream<T>
            => tType.DeserializeAsync(stream, version, cancellationToken);
        /// <summary>
        /// Deserialize from serialized bytes
        /// </summary>
        /// <param name="data">Serialized bytes</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Instance</returns>
        public static abstract T FromBytes(in ReadOnlyMemory<byte> data, int version);
        /// <summary>
        /// Deserialize from serialized bytes
        /// </summary>
        /// <typeparam name="tType">Interface implementing type</typeparam>
        /// <param name="data">Serialized bytes</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Instance</returns>
        public static T FromBytes<tType>(in ReadOnlyMemory<byte> data, int version) where tType : ISerializeStream<T> => tType.FromBytes(data, version);
    }
}

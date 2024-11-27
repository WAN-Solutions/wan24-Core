using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Interface for a type which can serialize binary
    /// </summary>
    public interface ISerializeBinary
    {
        /// <summary>
        /// Maximum binary structure size in bytes for all instances (or <see langword="null"/>, if the size is too variable)
        /// </summary>
        public abstract static int? MaxStructureSize { get; }
        /// <summary>
        /// Binary serialized structure size in bytes (or <see langword="null"/>, if the size is variable)
        /// </summary>
        int? StructureSize { get; }
        /// <summary>
        /// Get as serialized data
        /// </summary>
        /// <returns>Serialized data</returns>
        byte[] GetBytes();
        /// <summary>
        /// Get as serialized data
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of bytes written to the buffer</returns>
        int GetBytes(in Span<byte> buffer);
        /// <summary>
        /// Deserialize from previously serialized data
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Instance</returns>
        public abstract static object DeserializeFrom(in ReadOnlySpan<byte> buffer);
        /// <summary>
        /// Deserialize from previously serialized data
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="result">Instance</param>
        /// <returns>If succeed</returns>
        public abstract static bool TryDeserializeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(returnValue: true)] out object? result);
        /// <summary>
        /// Deserialize from previously serialized data
        /// </summary>
        /// <typeparam name="T">Interface implementing type</typeparam>
        /// <param name="buffer">Buffer</param>
        /// <returns>Instance</returns>
        public static object DeserializeFrom<T>(in ReadOnlySpan<byte> buffer) where T : ISerializeBinary => T.DeserializeFrom(buffer);
        /// <summary>
        /// Deserialize from previously serialized data
        /// </summary>
        /// <typeparam name="T">Interface implementing type</typeparam>
        /// <param name="buffer">Buffer</param>
        /// <param name="result">Instance</param>
        /// <returns>If succeed</returns>
        public static bool TryDeserializeFrom<T>(in ReadOnlySpan<byte> buffer, [NotNullWhen(returnValue: true)] out object? result) where T : ISerializeBinary
            => T.TryDeserializeFrom(buffer, out result);
        /// <summary>
        /// Get the maximum binary structure size in bytes for all instances
        /// </summary>
        /// <typeparam name="T">Interface implementing type</typeparam>
        /// <returns>Size or <see langword="null"/>, if the size is too variable</returns>
        public static int? GetMaxStructureSize<T>() where T : ISerializeBinary => T.MaxStructureSize;
    }

    /// <summary>
    /// Interface for a type which can serialize binary
    /// </summary>
    /// <typeparam name="T">Serializable type</typeparam>
    public interface ISerializeBinary<T> : ISerializeBinary where T : ISerializeBinary<T>
    {
        /// <summary>
        /// Deserialize from previously serialized data
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Instance</returns>
        public abstract static T DeserializeTypeFrom(in ReadOnlySpan<byte> buffer);
        /// <summary>
        /// Deserialize from previously serialized data
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="result">Instance</param>
        /// <returns>If succeed</returns>
        public abstract static bool TryDeserializeTypeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(returnValue: true)] out T? result);
        /// <summary>
        /// Deserialize from previously serialized data
        /// </summary>
        /// <typeparam name="tType">Interface implementing type</typeparam>
        /// <param name="buffer">Buffer</param>
        /// <returns>Instance</returns>
        public static T DeserializeTypeFrom<tType>(in ReadOnlySpan<byte> buffer) where tType : ISerializeBinary<T> => tType.DeserializeTypeFrom(buffer);
        /// <summary>
        /// Deserialize from previously serialized data
        /// </summary>
        /// <typeparam name="tType">Interface implementing type</typeparam>
        /// <param name="buffer">Buffer</param>
        /// <param name="result">Instance</param>
        /// <returns>If succeed</returns>
        public static bool TryDeserializeTypeFrom<tType>(in ReadOnlySpan<byte> buffer, [NotNullWhen(returnValue: true)] out T? result) where tType : ISerializeBinary<T>
            => tType.TryDeserializeTypeFrom(buffer, out result);
    }
}

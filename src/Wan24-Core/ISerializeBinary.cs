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
    }

    /// <summary>
    /// Interface for a type which can serialize binary
    /// </summary>
    /// <typeparam name="T">Serializable type</typeparam>
    public interface ISerializeBinary<T> : ISerializeBinary
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
    }
}

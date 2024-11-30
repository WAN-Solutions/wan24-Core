namespace wan24.Core
{
    // Bool
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Write a boolean
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        public static void Write(this Stream stream, in bool value)
            => stream.Write((byte)value.GetSerializedType(isNullable: false, useInterfaces: false));

        /// <summary>
        /// Write a boolean
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        public static void Write(this Stream stream, in bool? value)
            => stream.Write((byte)(value?.GetSerializedType(isNullable: false, useInterfaces: false) ?? SerializedObjectTypes.None));

        /// <summary>
        /// Write a boolean
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, bool value, CancellationToken cancellationToken = default)
            => await stream.WriteAsync(
                (byte)value.GetSerializedType(isNullable: false, useInterfaces: false), 
                cancellationToken: cancellationToken
                ).DynamicContext();

        /// <summary>
        /// Write a boolean
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, bool? value, CancellationToken cancellationToken = default)
            => await stream.WriteAsync(
                (byte)(value?.GetSerializedType(isNullable: false, useInterfaces: false) ?? SerializedObjectTypes.None), 
                cancellationToken: cancellationToken
                ).DynamicContext();

        /// <summary>
        /// Read a boolean
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
        public static bool ReadBoolean(this Stream stream, in int version)
            => (bool)(((SerializedObjectTypes)stream.ReadOneByte(version)).GetValue() ?? throw new InvalidDataException("Unexpected NULL value"));

        /// <summary>
        /// Read a boolean
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
        public static bool? ReadBooleanNullable(this Stream stream, in int version)
            => (bool?)((SerializedObjectTypes)stream.ReadOneByte(version)).GetValue();

        /// <summary>
        /// Read a boolean
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<bool> ReadBooleanAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
            => (bool)(((SerializedObjectTypes)await stream.ReadOneByteAsync(version, buffer, cancellationToken).DynamicContext()).GetValue() 
                ?? throw new InvalidDataException("Unexpected NULL value"));

        /// <summary>
        /// Read a boolean
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<bool?> ReadBooleanNullableAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
            => (bool?)((SerializedObjectTypes)await stream.ReadOneByteAsync(version, buffer, cancellationToken).DynamicContext()).GetValue();
    }
}

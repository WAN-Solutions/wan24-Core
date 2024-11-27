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
        public static void Write(this Stream stream, in bool value) => stream.Write((byte)(value ? 1 : 0));

        /// <summary>
        /// Write a boolean
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        public static void Write(this Stream stream, in bool? value) => stream.Write((byte)(value.HasValue ? value.Value ? 1 : 0 : 2));

        /// <summary>
        /// Write a boolean
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, bool value, CancellationToken cancellationToken = default)
            => await stream.WriteAsync((byte)(value ? 1 : 0), cancellationToken: cancellationToken).DynamicContext();

        /// <summary>
        /// Write a boolean
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, bool? value, CancellationToken cancellationToken = default)
            => await stream.WriteAsync((byte)(value.HasValue ? value.Value ? 1 : 0 : 2), cancellationToken: cancellationToken).DynamicContext();

        /// <summary>
        /// Read a boolean
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
        public static bool ReadBoolean(this Stream stream, in int version)
        {
            byte value = stream.ReadOneByte(version);
            return value switch
            {
                0 => false,
                1 => true,
                _ => throw new InvalidDataException($"{value} isn't a valid boolean value")
            };
        }

        /// <summary>
        /// Read a boolean
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
        public static bool? ReadBooleanNullable(this Stream stream, in int version)
        {
            byte value = stream.ReadOneByte(version);
            return value switch
            {
                0 => false,
                1 => true,
                2 => null,
                _ => throw new InvalidDataException($"{value} isn't a valid boolean value")
            };
        }

        /// <summary>
        /// Read a boolean
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<bool> ReadBooleanAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            byte value = await stream.ReadOneByteAsync(version, buffer, cancellationToken).DynamicContext();
            return value switch
            {
                0 => false,
                1 => true,
                _ => throw new InvalidDataException($"{value} isn't a valid boolean value")
            };
        }

        /// <summary>
        /// Read a boolean
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<bool?> ReadBooleanNullableAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            byte value = await stream.ReadOneByteAsync(version, buffer, cancellationToken).DynamicContext();
            return value switch
            {
                0 => false,
                1 => true,
                2 => null,
                _ => throw new InvalidDataException($"{value} isn't a valid boolean value")
            };
        }
    }
}

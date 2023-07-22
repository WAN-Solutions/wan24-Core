using System.Buffers;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="Stream"/> extensions
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Get the number of remaining bytes until the streams end
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Remaining number of bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static long GetRemainingBytes(this Stream stream) => stream.Length - stream.Position;

        /// <summary>
        /// Copy partial to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="target">Target stream</param>
        /// <param name="count">Number of bytes to copy</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <returns>Number of left bytes ('cause the source stream didn't deliver enough data)</returns>
        public static long CopyPartialTo(this Stream stream, Stream target, long count, int? bufferSize = null)
        {
            bufferSize ??= Settings.BufferSize;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize.Value);
            try
            {
                int read,
                    red;
                while (count != 0)
                {
                    read = (int)Math.Min(count, bufferSize.Value);
                    red = stream.Read(buffer.AsSpan(0, read));
                    if (red != 0) target.Write(buffer.AsSpan(0, red));
                    count -= red;
                    if (red < read) break;
                }
                return count;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Copy partial to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="target">Target stream</param>
        /// <param name="count">Number of bytes to copy</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of left bytes ('cause the source stream didn't deliver enough data)</returns>
        public static async Task<long> CopyPartialToAsync(this Stream stream, Stream target, long count, int? bufferSize = null, CancellationToken cancellationToken = default)
        {
            bufferSize ??= Settings.BufferSize;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize.Value);
            try
            {
                int read,
                    red;
                while (count != 0)
                {
                    read = (int)Math.Min(count, bufferSize.Value);
                    red = await stream.ReadAsync(buffer.AsMemory(0, read), cancellationToken).DynamicContext();
                    if (red != 0) await target.WriteAsync(buffer.AsMemory(0, red), cancellationToken).DynamicContext();
                    count -= red;
                    if (red < read) break;
                }
                return count;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}

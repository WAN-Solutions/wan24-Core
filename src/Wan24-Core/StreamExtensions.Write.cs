namespace wan24.Core
{
    // Write
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Write zero
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes</param>
        public static void WriteZero(this Stream stream, in long count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return;
            using ZeroStream zero = new();
            zero.SetLength(count);
            zero.CopyTo(stream);
        }

        /// <summary>
        /// Write zero
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteZeroAsync(this Stream stream, long count, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return;
            using ZeroStream zero = new();
            zero.SetLength(count);
            await zero.CopyToAsync(stream, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write random bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes</param>
        public static void WriteRandom(this Stream stream, in long count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return;
            RandomStream.Instance.CopyPartialTo(stream, count);
        }

        /// <summary>
        /// Write random bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteRandomAsync(this Stream stream, long count, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return;
            await RandomStream.Instance.CopyPartialToAsync(stream, count, cancellationToken: cancellationToken).DynamicContext();
        }
    }
}

namespace wan24.Core
{
    // Copy
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Copy partial to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="target">Target stream</param>
        /// <param name="count">Number of bytes to copy</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="progress">Progress</param>
        /// <returns>Number of left bytes ('cause the source stream didn't deliver enough data)</returns>
        public static long CopyPartialTo(this Stream stream, in Stream target, long count, int? bufferSize = null, ProcessingProgress? progress = null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return 0;
            bufferSize ??= Settings.BufferSize;
            bufferSize = (int)Math.Min(count, bufferSize.Value);
            using RentedMemoryRef<byte> buffer = new(bufferSize.Value, clean: false);
            Span<byte> bufferSpan = buffer.Span;
            for (int red = 1; count > 0 && red > 0; count -= red)
            {
                red = stream.Read(bufferSpan[..(int)Math.Min(count, bufferSize.Value)]);
                if (red != 0)
                {
                    target.Write(bufferSpan[..red]);
                    progress?.Update(red);
                }
            }
            return count;
        }

        /// <summary>
        /// Copy partial to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="target">Target stream</param>
        /// <param name="count">Number of bytes to copy</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="progress">Progress</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of left bytes ('cause the source stream didn't deliver enough data)</returns>
        public static async Task<long> CopyPartialToAsync(
            this Stream stream,
            Stream target,
            long count,
            int? bufferSize = null,
            ProcessingProgress? progress = null,
            CancellationToken cancellationToken = default
            )
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return 0;
            if (progress is not null) cancellationToken = progress.GetCancellationToken(cancellationToken);
            bufferSize ??= Settings.BufferSize;
            bufferSize = (int)Math.Min(count, bufferSize.Value);
            using RentedMemory<byte> buffer = new(bufferSize.Value, clean: false);
            for (int red = 1; count > 0 && red > 0; count -= red)
            {
                red = await stream.ReadAsync(buffer.Memory[..(int)Math.Min(count, bufferSize.Value)], cancellationToken).DynamicContext();
                if (red != 0)
                {
                    await target.WriteAsync(buffer.Memory[..red], cancellationToken).DynamicContext();
                    progress?.Update(red);
                }
            }
            return count;
        }

        /// <summary>
        /// Copy partial to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="target">Target stream</param>
        /// <param name="count">Number of bytes to copy</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="progress">Progress</param>
        public static void CopyExactlyPartialTo(this Stream stream, in Stream target, long count, int? bufferSize = null, ProcessingProgress? progress = null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return;
            bufferSize ??= Settings.BufferSize;
            bufferSize = (int)Math.Min(count, bufferSize.Value);
            using RentedArray<byte> buffer = new(bufferSize.Value, clean: false);
            Span<byte> bufferSpan = buffer.Memory.Span;
            for (int red = 1; count > 0 && red > 0; count -= red)
            {
                red = stream.Read(bufferSpan[..(int)Math.Min(count, bufferSize.Value)]);
                if (red != 0)
                {
                    target.Write(bufferSpan[..red]);
                    progress?.Update(red);
                }
            }
            if (count != 0) throw new IOException("Not enough data");
        }

        /// <summary>
        /// Copy partial to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="target">Target stream</param>
        /// <param name="count">Number of bytes to copy</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="progress">Progress</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task CopyExactlyPartialToAsync(
            this Stream stream,
            Stream target,
            long count,
            int? bufferSize = null,
            ProcessingProgress? progress = null,
            CancellationToken cancellationToken = default
            )
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return;
            if (progress is not null) cancellationToken = progress.GetCancellationToken(cancellationToken);
            bufferSize ??= Settings.BufferSize;
            bufferSize = (int)Math.Min(count, bufferSize.Value);
            using RentedArrayStructSimple<byte> buffer = new(bufferSize.Value, clean: false);
            for (int red = 1; count > 0 && red > 0; count -= red)
            {
                red = await stream.ReadAsync(buffer.Memory[..(int)Math.Min(count, bufferSize.Value)], cancellationToken).DynamicContext();
                if (red != 0)
                {
                    await target.WriteAsync(buffer.Memory[..red], cancellationToken).DynamicContext();
                    progress?.Update(red);
                }
            }
            if (count != 0) throw new IOException("Not enough data");
        }

        /// <summary>
        /// Generic copy to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="destination">Target</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="progress">Progress</param>
        public static void GenericCopyTo(this Stream stream, in Stream destination, in int bufferSize = 81_920, ProcessingProgress? progress = null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(bufferSize, 1);
            using RentedMemoryRef<byte> buffer = new(bufferSize, clean: false);
            Span<byte> bufferSpan = buffer.Span;
            for (int red = bufferSize; red == bufferSize;)
            {
                red = stream.Read(bufferSpan);
                if (red != 0)
                {
                    destination.Write(bufferSpan[..red]);
                    progress?.Update(red);
                }
            }
        }

        /// <summary>
        /// Generic copy to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="destination">Target</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="progress">Progress</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task GenericCopyToAsync(
            this Stream stream,
            Stream destination,
            int bufferSize = 81_920,
            ProcessingProgress? progress = null,
            CancellationToken cancellationToken = default
            )
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(bufferSize, 1);
            if (progress is not null) cancellationToken = progress.GetCancellationToken(cancellationToken);
            using RentedArrayStructSimple<byte> buffer = new(bufferSize, clean: false);
            for (int red = bufferSize; red == bufferSize;)
            {
                red = await stream.ReadAsync(buffer.Memory, cancellationToken).DynamicContext();
                if (red != 0)
                {
                    await destination.WriteAsync(buffer.Memory[..red], cancellationToken).DynamicContext();
                    progress?.Update(red);
                }
            }
        }
    }
}

using System.Runtime.CompilerServices;
using System.Runtime;

namespace wan24.Core
{
    // Read
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Read a number of bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes to read</param>
        /// <returns>Red bytes</returns>
        public static byte[] Read(this Stream stream, in int count)
        {
            using RentedMemoryRef<byte> buffer = new(count, clean: false);
            Span<byte> bufferSpan = buffer.Span;
            int red = stream.Read(bufferSpan);
            if (red < 1) return [];
            byte[] res = new byte[red];
            bufferSpan[..red].CopyTo(res);
            return res;
        }

        /// <summary>
        /// Read a number of bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes to read</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Red bytes</returns>
        public static async Task<byte[]> ReadAsync(this Stream stream, int count, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte> buffer = new(count, clean: false);
            int red = await stream.ReadAsync(buffer.Memory, cancellationToken).DynamicContext();
            if (red < 1) return [];
            byte[] res = new byte[red];
            buffer.Memory.Span[..red].CopyTo(res);
            return res;
        }

        /// <summary>
        /// Read an exact number of bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes to read</param>
        /// <returns>Red bytes</returns>
        public static byte[] ReadExactly(this Stream stream, in int count)
        {
            byte[] res = new byte[count];
            stream.ReadExactly(res);
            return res;
        }

        /// <summary>
        /// Read an exact number of bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes to read</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Red bytes</returns>
        public static async Task<byte[]> ReadExactlyAsync(this Stream stream, int count, CancellationToken cancellationToken = default)
        {
            byte[] res = new byte[count];
            await stream.ReadExactlyAsync(res, cancellationToken).DynamicContext();
            return res;
        }

        /// <summary>
        /// Read to the end
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cleanBuffer">If to clean used buffers</param>
        /// <param name="progress">Progress</param>
        /// <returns>Red bytes</returns>
        public static byte[] ReadToEnd(this Stream stream, in bool cleanBuffer = false, in ProcessingProgress? progress = null)
        {
            if (stream.CanSeek)
            {
                long remaining = GetRemainingBytes(stream);
                if (remaining < 1) return [];
                if (remaining > int.MaxValue) throw new OutOfMemoryException();
                if (progress is not null) progress.Total += remaining;
                byte[] res = new byte[remaining];
                stream.ReadExactly(res);
                progress?.Update((int)remaining);
                return res;
            }
            else
            {
                using MemoryPoolStream ms = new()
                {
                    CleanReturned = cleanBuffer
                };
                stream.GenericCopyTo(ms, progress: progress);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Read to the end
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cleanBuffer">If to clean used buffers</param>
        /// <param name="progress">Progress</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Red bytes</returns>
        public static async Task<byte[]> ReadToEndAsync(this Stream stream, bool cleanBuffer = false, ProcessingProgress? progress = null, CancellationToken cancellationToken = default)
        {
            if (stream.CanSeek)
            {
                long remaining = GetRemainingBytes(stream);
                if (remaining < 1) return [];
                if (remaining > int.MaxValue) throw new OutOfMemoryException();
                if (progress is not null) progress.Total += remaining;
                byte[] res = new byte[remaining];
                await stream.ReadExactlyAsync(res, cancellationToken).DynamicContext();
                progress?.Update((int)remaining);
                return res;
            }
            else
            {
                using MemoryPoolStream ms = new()
                {
                    CleanReturned = cleanBuffer
                };
                await stream.GenericCopyToAsync(ms, progress: progress, cancellationToken: cancellationToken).DynamicContext();
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Read until a condition stops reading, or the buffer is full, or nothing was red (and there's no timeout)
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="chunkSize">Chunk size in bytes</param>
        /// <param name="timeout">Timeout to wait if nothing was red</param>
        /// <param name="condition">Condition (gets the stream, the buffer, the last number of red bytes and the total number of red bytes; needs to return if to continue reading)</param>
        /// <returns>Number of red bytes</returns>
        public static int ReadUntil(this Stream stream, Memory<byte> buffer, int chunkSize, TimeSpan timeout, Func<Stream, Memory<byte>, int, int, bool> condition)
        {
            int res = 0,// Total number of red bytes
                red;// Current number of red bytes
            while (true)
            {
                red = stream.Read(buffer.Span.Slice(res, Math.Min(chunkSize, buffer.Length - res)));
                if (red == 0)
                {
                    if (timeout == TimeSpan.Zero) return res;
                    Thread.Sleep(timeout);
                    continue;
                }
                res += red;
                if (res == buffer.Length || !condition(stream, buffer, red, res)) return res;
            }
        }

        /// <summary>
        /// Read until a condition stops reading, or the buffer is full, or nothing was red (and there's no timeout)
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="chunkSize">Chunk size in bytes</param>
        /// <param name="timeout">Timeout to wait if nothing was red</param>
        /// <param name="condition">Condition (gets the stream, the red buffer, the last number of red bytes and the total number of red bytes; needs to return if to continue reading)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of red bytes</returns>
        public static async Task<int> ReadUntilAsync
            (this Stream stream,
            Memory<byte> buffer,
            int chunkSize,
            TimeSpan timeout,
            Func<Stream, Memory<byte>, int, int, CancellationToken, bool> condition,
            CancellationToken cancellationToken = default
            )
        {
            int res = 0,// Total number of red bytes
                red;// Current number of red bytes
            while (!cancellationToken.GetIsCancellationRequested())
            {
                red = await stream.ReadAsync(buffer.Slice(res, Math.Min(chunkSize, buffer.Length - res)), cancellationToken).DynamicContext();
                if (red == 0)
                {
                    if (timeout == TimeSpan.Zero) return res;
                    await Task.Delay(timeout, cancellationToken);
                    continue;
                }
                res += red;
                if (res == buffer.Length || !condition(stream, buffer, red, res, cancellationToken)) return res;
            }
            return res;
        }

        /// <summary>
        /// Read until a condition stops reading, or the buffer is full, or nothing was red (and there's no timeout)
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="chunkSize">Chunk size in bytes</param>
        /// <param name="timeout">Timeout to wait if nothing was red</param>
        /// <param name="condition">Condition (gets the stream, the red buffer, the last number of red bytes and the total number of red bytes; needs to return if to continue reading)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of red bytes</returns>
        public static async Task<int> ReadUntilAsync
            (this Stream stream,
            Memory<byte> buffer,
            int chunkSize,
            TimeSpan timeout,
            Func<Stream, Memory<byte>, int, int, CancellationToken, Task<bool>> condition,
            CancellationToken cancellationToken = default
            )
        {
            int res = 0,// Total number of red bytes
                red;// Current number of red bytes
            while (!cancellationToken.GetIsCancellationRequested())
            {
                red = await stream.ReadAsync(buffer.Slice(res, Math.Min(chunkSize, buffer.Length - res)), cancellationToken).DynamicContext();
                if (red == 0)
                {
                    if (timeout == TimeSpan.Zero) return res;
                    await Task.Delay(timeout, cancellationToken);
                    continue;
                }
                res += red;
                if (res == buffer.Length || !await condition(stream, buffer, red, res, cancellationToken).DynamicContext()) return res;
            }
            return res;
        }

        /// <summary>
        /// Read a stream with length information (8 byte)
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <returns>Partial stream (must be fully red before reading more from <c>stream</c>)</returns>
        public static PartialStream<T> ReadStreamWithLengthInfo<T>(this T stream) where T : Stream
        {
            using RentedMemoryRef<byte> buffer = new(len: sizeof(long));
            Span<byte> bufferSpan = buffer.Span;
            stream.ReadExactly(bufferSpan);
            return new(stream, bufferSpan.ToLong(), leaveOpen: true);
        }

        /// <summary>
        /// Read a stream with length information (8 byte)
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Partial stream (must be fully red before reading more from <c>stream</c>)</returns>
        public static async Task<PartialStream<T>> ReadStreamWithLengthInfoAsync<T>(this T stream, CancellationToken cancellationToken = default) where T : Stream
        {
            using RentedMemory<byte> buffer = new(len: sizeof(long));
            Memory<byte> bufferMem = buffer.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return new(stream, bufferMem.Span.ToLong(), leaveOpen: true);
        }

        /// <summary>
        /// Read data with length information (4 byte)
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of red bytes into the buffer</returns>
        public static int ReadDataWithLengthInfo<T>(this T stream, Span<byte> buffer) where T : Stream
        {
            using RentedMemoryRef<byte> buffer2 = new(len: sizeof(int));
            Span<byte> bufferSpan = buffer2.Span;
            stream.ReadExactly(bufferSpan);
            int res = bufferSpan.ToInt();
            stream.ReadExactly(buffer[..res]);
            return res;
        }

        /// <summary>
        /// Read data with length information (4 byte)
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of red bytes into the buffer</returns>
        public static async Task<int> ReadDataWithLengthInfoAsync<T>(
            this T stream,
            Memory<byte> buffer,
            CancellationToken cancellationToken = default
            )
            where T : Stream
        {
            using RentedMemory<byte> buffer2 = new(len: sizeof(int));
            Memory<byte> bufferMem = buffer2.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            int res = bufferMem.Span.ToInt();
            await stream.ReadExactlyAsync(buffer[..res], cancellationToken).DynamicContext();
            return res;
        }

        /// <summary>
        /// Read the stream to the end
        /// </summary>
        /// <param name="stream">Stream</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ReadToEnd(this Stream stream) => stream.CopyTo(Stream.Null);

        /// <summary>
        /// Read the stream to the end
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task ReadToEndAsync(this Stream stream, CancellationToken cancellationToken = default)
            => await stream.CopyToAsync(Stream.Null, cancellationToken).DynamicContext();

        /// <summary>
        /// Read a stream with length information or chunked
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Red stream (must be fully red before reading more from <c>stream</c>)</returns>
        public static Stream ReadStream(this Stream stream)
        {
            int type = stream.ReadByte();
            return type switch
            {
                0 => stream.ReadStreamWithLengthInfo(),
                1 => ChunkStream.FromExisting(new CutStream(stream, leaveOpen: true)),
                _ => throw new InvalidDataException($"Invalid stream type #{type}"),
            };
        }

        /// <summary>
        /// Read a stream with length information or chunked
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Red stream (must be fully red before reading more from <c>stream</c>)</returns>
        public static async Task<Stream> ReadStreamAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            int type = stream.ReadByte();
            return type switch
            {
                0 => await stream.ReadStreamWithLengthInfoAsync(cancellationToken).DynamicContext(),
                1 => await ChunkStream.FromExistingAsync(new CutStream(stream, leaveOpen: true), cancellationToken: cancellationToken).DynamicContext(),
                _ => throw new InvalidDataException($"Invalid stream type #{type}"),
            };
        }
    }
}

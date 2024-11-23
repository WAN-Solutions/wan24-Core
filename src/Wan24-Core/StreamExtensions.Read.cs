using System.Runtime;
using System.Runtime.CompilerServices;

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
        /// Read a stream with length information
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Partial stream (must be fully red before reading more from <c>stream</c>)</returns>
        public static PartialStream<T> ReadStreamWithLengthInfo<T>(this T stream, in int version) where T : Stream
        {
            long len = stream.ReadNumeric<long>(version);
            if (len < 0) throw new InvalidDataException($"Invalid stream length {len} bytes");
            return new(stream, len, leaveOpen: true);
        }

        /// <summary>
        /// Read a stream with length information
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Partial stream (must be fully red before reading more from <c>stream</c>)</returns>
        public static PartialStream<T>? ReadStreamNullableWithLengthInfo<T>(this T stream, in int version) where T : Stream
        {
            long? len = stream.ReadNumericNullable<long>(version);
            if (len.HasValue && len.Value < 0) throw new InvalidDataException($"Invalid stream length {len} bytes");
            return len.HasValue
                ? new(stream, len.Value, leaveOpen: true)
                : null;
        }

        /// <summary>
        /// Read a stream with length information
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Partial stream (must be fully red before reading more from <c>stream</c>)</returns>
        public static async Task<PartialStream<T>> ReadStreamWithLengthInfoAsync<T>(this T stream, int version, CancellationToken cancellationToken = default)
            where T : Stream
        {
            long len = await stream.ReadNumericAsync<long>(version, cancellationToken: cancellationToken).DynamicContext();
            if (len < 0) throw new InvalidDataException($"Invalid stream length {len} bytes");
            return new(stream, len, leaveOpen: true);
        }

        /// <summary>
        /// Read a stream with length information
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Partial stream (must be fully red before reading more from <c>stream</c>)</returns>
        public static async Task<PartialStream<T>?> ReadStreamNullableWithLengthInfoAsync<T>(this T stream, int version, CancellationToken cancellationToken = default)
            where T : Stream
        {
            long? len = await stream.ReadNumericNullableAsync<long>(version, cancellationToken: cancellationToken).DynamicContext();
            if (len.HasValue && len.Value < 0) throw new InvalidDataException($"Invalid stream length {len} bytes");
            return len.HasValue
                ? new(stream, len.Value, leaveOpen: true)
                : null;
        }

        /// <summary>
        /// Read data with length information
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of red bytes into the buffer</returns>
        public static int ReadDataWithLengthInfo<T>(this T stream, in int version, in Span<byte> buffer) where T : Stream
        {
            int res = stream.ReadNumeric<int>(version);
            if (res < 0) throw new InvalidDataException($"Invalid data length {res} bytes");
            if (res > buffer.Length) throw new OutOfMemoryException($"Buffer too small for {res} bytes");
            stream.ReadExactly(buffer[..res]);
            return res;
        }

        /// <summary>
        /// Read data with length information
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of red bytes into the buffer or <c>-1</c>, if <see langword="null"/></returns>
        public static int ReadDataNullableWithLengthInfo<T>(this T stream, in int version, in Span<byte> buffer) where T : Stream
        {
            int? res = stream.ReadNumericNullable<int>(version);
            if (!res.HasValue) return -1;
            if (res.Value < 0) throw new InvalidDataException($"Invalid data length {res} bytes");
            if (res.Value > buffer.Length) throw new OutOfMemoryException($"Buffer too small for {res} bytes");
            stream.ReadExactly(buffer[..res.Value]);
            return res.Value;
        }

        /// <summary>
        /// Read data with length information
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of red bytes into the buffer</returns>
        public static async Task<int> ReadDataWithLengthInfoAsync<T>(
            this T stream,
            int version,
            Memory<byte> buffer,
            CancellationToken cancellationToken = default
            )
            where T : Stream
        {
            int res = await stream.ReadNumericAsync<int>(version, cancellationToken: cancellationToken).DynamicContext();
            if (res < 0) throw new InvalidDataException($"Invalid data length {res} bytes");
            if (res > buffer.Length) throw new OutOfMemoryException($"Buffer too small for {res} bytes");
            await stream.ReadExactlyAsync(buffer[..res], cancellationToken).DynamicContext();
            return res;
        }

        /// <summary>
        /// Read data with length information
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of red bytes into the buffer or <c>-1</c>, if <see langword="null"/></returns>
        public static async Task<int> ReadDataNullableWithLengthInfoAsync<T>(
            this T stream,
            int version,
            Memory<byte> buffer,
            CancellationToken cancellationToken = default
            )
            where T : Stream
        {
            int? res = await stream.ReadNumericNullableAsync<int>(version, cancellationToken: cancellationToken).DynamicContext();
            if (!res.HasValue) return -1;
            if (res.Value < 0) throw new InvalidDataException($"Invalid data length {res} bytes");
            if (res.Value > buffer.Length) throw new OutOfMemoryException($"Buffer too small for {res} bytes");
            await stream.ReadExactlyAsync(buffer[..res.Value], cancellationToken).DynamicContext();
            return res.Value;
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
        /// <param name="version">Data structure version</param>
        /// <returns>Red stream (must be fully red before reading more from <c>stream</c>)</returns>
        public static Stream ReadStream(this Stream stream, in int version)
        {
            int type = stream.ReadByte();
            return type switch
            {
                0 => stream.ReadStreamWithLengthInfo(version),
                1 => ChunkStream.FromExisting(new CutStream(stream, leaveOpen: true)),
                _ => throw new InvalidDataException($"Invalid stream type #{type}"),
            };
        }

        /// <summary>
        /// Read a stream with length information or chunked
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Red stream (must be fully red before reading more from <c>stream</c>)</returns>
        public static Stream? ReadStreamNullable(this Stream stream, in int version)
        {
            int type = stream.ReadByte();
            return type switch
            {
                0 => stream.ReadStreamWithLengthInfo(version),
                1 => ChunkStream.FromExisting(new CutStream(stream, leaveOpen: true)),
                2 => null,
                _ => throw new InvalidDataException($"Invalid stream type #{type}"),
            };
        }

        /// <summary>
        /// Read a stream with length information or chunked
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Red stream (must be fully red before reading more from <c>stream</c>)</returns>
        public static async Task<Stream> ReadStreamAsync(this Stream stream, int version, CancellationToken cancellationToken = default)
        {
            int type = stream.ReadByte();
            return type switch
            {
                0 => await stream.ReadStreamWithLengthInfoAsync(version, cancellationToken).DynamicContext(),
                1 => await ChunkStream.FromExistingAsync(new CutStream(stream, leaveOpen: true), cancellationToken: cancellationToken).DynamicContext(),
                _ => throw new InvalidDataException($"Invalid stream type #{type}"),
            };
        }

        /// <summary>
        /// Read a stream with length information or chunked
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Red stream (must be fully red before reading more from <c>stream</c>)</returns>
        public static async Task<Stream?> ReadStreamNullableAsync(this Stream stream, int version, CancellationToken cancellationToken = default)
        {
            int type = stream.ReadByte();
            return type switch
            {
                0 => await stream.ReadStreamWithLengthInfoAsync(version, cancellationToken).DynamicContext(),
                1 => await ChunkStream.FromExistingAsync(new CutStream(stream, leaveOpen: true), cancellationToken: cancellationToken).DynamicContext(),
                2 => null,
                _ => throw new InvalidDataException($"Invalid stream type #{type}"),
            };
        }
    }
}

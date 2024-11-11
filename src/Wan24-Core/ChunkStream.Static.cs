using System.Reflection;

namespace wan24.Core
{
    // Static
    public partial class ChunkStream<tStream, tFinal>
    {
        /// <summary>
        /// Final type constructor to use by the static methods
        /// </summary>
        protected static readonly ConstructorInfoExt FinalConstructorInfo;

        /// <summary>
        /// Static constructor
        /// </summary>
        static ChunkStream()
        {
            FinalConstructorInfo = typeof(tFinal).GetConstructorsCached(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(c => c.GetCustomAttributeCached<ConstructorAttribute>() is not null)
                .FirstOrDefault()
                ?? throw new InvalidProgramException($"{typeof(tFinal)} is missing a constructor with the {typeof(ConstructorAttribute)} attribute");
            if (FinalConstructorInfo.Invoker is null) throw new InvalidProgramException($"{FinalConstructorInfo.FullName} isn't usable");
        }

        /// <summary>
        /// Create a new writable chunk stream
        /// </summary>
        /// <param name="baseStream">Empty base stream</param>
        /// <param name="chunkSize">Chunk size in bytes</param>
        /// <param name="writeChunkSizeHeader">If to write the chunk size to the beginning of the stream</param>
        /// <param name="clearBuffers">If to clear buffers (if <see langword="null"/>, <see cref="Settings.ClearBuffers"/> will be used)</param>
        /// <param name="leaveOpen">If to leave the base stream open when disposing</param>
        /// <returns>Instance</returns>
        public static tFinal CreateNew(
            in tStream baseStream,
            in int chunkSize,
            in bool writeChunkSizeHeader = true,
            in bool? clearBuffers = null,
            in bool leaveOpen = false
            )
        {
            if (baseStream.CanSeek && baseStream.Length > 0) throw new ArgumentException("The base stream must be empty", nameof(baseStream));
            ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, other: 1, nameof(chunkSize));
            if (writeChunkSizeHeader)
            {
                using RentedMemoryRef<byte> buffer = new(sizeof(int), clean: false);
                chunkSize.GetBytes(buffer.Span);
                baseStream.Write(buffer.Span);
            }
            NullabilityInfoContext nic = new();
            object?[] param = FinalConstructorInfo.GetParameters().GetDiObjects(
                [
                    baseStream,
                    chunkSize,
                    true,
                    writeChunkSizeHeader,
                    clearBuffers,
                    leaveOpen
                ],
                serviceProvider: null,
                nic
                );
            tFinal res = (tFinal)FinalConstructorInfo.Invoker!(param);
            res.RedAll = true;
            if (writeChunkSizeHeader) res._Length = res._Position = sizeof(int);
            return res;
        }

        /// <summary>
        /// Create a new writable chunk stream
        /// </summary>
        /// <param name="baseStream">Empty base stream</param>
        /// <param name="chunkSize">Chunk size in bytes</param>
        /// <param name="writeChunkSizeHeader">If to write the chunk size to the beginning of the stream</param>
        /// <param name="clearBuffers">If to clear buffers (if <see langword="null"/>, <see cref="Settings.ClearBuffers"/> will be used)</param>
        /// <param name="leaveOpen">If to leave the base stream open when disposing</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Instance</returns>
        public static async Task<tFinal> CreateNewAsync(
            tStream baseStream,
            int chunkSize,
            bool writeChunkSizeHeader = true,
            bool? clearBuffers = null,
            bool leaveOpen = false,
            CancellationToken cancellationToken = default
            )
        {
            if (baseStream.CanSeek && baseStream.Length > 0) throw new ArgumentException("The base stream must be empty", nameof(baseStream));
            ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, other: 1, nameof(chunkSize));
            if (writeChunkSizeHeader)
            {
                using RentedMemory<byte> buffer = new(sizeof(int), clean: false);
                chunkSize.GetBytes(buffer.Memory.Span);
                await baseStream.WriteAsync(buffer.Memory, cancellationToken).DynamicContext();
            }
            NullabilityInfoContext nic = new();
            object?[] param = await FinalConstructorInfo.GetParameters().GetDiObjectsAsync(
                [
                    baseStream,
                    chunkSize,
                    true,
                    writeChunkSizeHeader,
                    clearBuffers,
                    leaveOpen
                ],
                serviceProvider: null,
                nic,
                cancellationToken: cancellationToken
                ).DynamicContext();
            tFinal res = (tFinal)FinalConstructorInfo.Invoker!(param);
            res.RedAll = true;
            if (writeChunkSizeHeader) res._Length = res._Position = sizeof(int);
            return res;
        }

        /// <summary>
        /// Create a new read-only chunk stream instance from a stream which contains chunked contents
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="containsChunkSizeHeader">If the chunk size is contained at the beginning of the stream</param>
        /// <param name="maxChunkSize">Chunk size in bytes (is the maximum, when reading the chunk size header!)</param>
        /// <param name="clearBuffers">If to clear buffers (if <see langword="null"/>, <see cref="Settings.ClearBuffers"/> will be used)</param>
        /// <param name="leaveOpen">If to leave the base stream open when disposing</param>
        /// <returns>Instance</returns>
        public static tFinal FromExisting(
            in tStream baseStream,
            in bool containsChunkSizeHeader = true,
            int maxChunkSize = 0,
            in bool? clearBuffers = null,
            in bool leaveOpen = false
            )
        {
            if (containsChunkSizeHeader)
            {
                using RentedMemoryRef<byte> buffer = new(sizeof(int), clean: false);
                baseStream.ReadExactly(buffer.Span);
                int max = maxChunkSize;
                maxChunkSize = buffer.Span.ToInt();
                if (maxChunkSize < 1 || (max > 0 && maxChunkSize > max)) throw new InvalidDataException($"Invalid chunk size {maxChunkSize}");
            }
            else
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(maxChunkSize, other: 1, nameof(maxChunkSize));
            }
            NullabilityInfoContext nic = new();
            tFinal res = (tFinal)FinalConstructorInfo.Invoker!(
                FinalConstructorInfo.GetParameters().GetDiObjects(
                    [
                        baseStream,
                        maxChunkSize,
                        false,
                        containsChunkSizeHeader,
                        clearBuffers,
                        leaveOpen
                    ],
                    serviceProvider: null,
                    nic
                    )
                );
            if (containsChunkSizeHeader) res._Position = sizeof(int);
            if (baseStream.CanSeek) res._Length = baseStream.Length;
            res.IsFlushedFinally = true;
            return res;
        }

        /// <summary>
        /// Create a new read-only chunk stream instance from a stream which contains chunked contents
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="containsChunkSizeHeader">If the chunk size is contained at the beginning of the stream</param>
        /// <param name="maxChunkSize">Chunk size in bytes (is the maximum, when reading the chunk size header!)</param>
        /// <param name="clearBuffers">If to clear buffers (if <see langword="null"/>, <see cref="Settings.ClearBuffers"/> will be used)</param>
        /// <param name="leaveOpen">If to leave the base stream open when disposing</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Instance</returns>
        public static async Task<tFinal> FromExistingAsync(
            tStream baseStream,
            bool containsChunkSizeHeader = true,
            int maxChunkSize = 0,
            bool? clearBuffers = null,
            bool leaveOpen = false,
            CancellationToken cancellationToken = default
            )
        {
            if (containsChunkSizeHeader)
            {
                using RentedMemory<byte> buffer = new(sizeof(int), clean: false);
                await baseStream.ReadExactlyAsync(buffer.Memory, cancellationToken).DynamicContext();
                int max = maxChunkSize;
                maxChunkSize = buffer.Memory.Span.ToInt();
                if (maxChunkSize < 1 || (max > 0 && maxChunkSize > max)) throw new InvalidDataException($"Invalid chunk size {maxChunkSize}");
            }
            else
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(maxChunkSize, other: 1, nameof(maxChunkSize));
            }
            NullabilityInfoContext nic = new();
            tFinal res = (tFinal)FinalConstructorInfo.Invoker!(
                await FinalConstructorInfo.GetParameters().GetDiObjectsAsync(
                    [
                        baseStream,
                        maxChunkSize,
                        false,
                        containsChunkSizeHeader,
                        clearBuffers,
                        leaveOpen
                    ],
                    serviceProvider: null,
                    nic,
                    cancellationToken: cancellationToken
                    ).DynamicContext()
                );
            if (containsChunkSizeHeader) res._Position = sizeof(int);
            if (baseStream.CanSeek) res._Length = baseStream.Length;
            res.IsFlushedFinally = true;
            return res;
        }

        /// <summary>
        /// Calculate the last chunk index and length from a data length
        /// </summary>
        /// <param name="len">Data length in bytes</param>
        /// <param name="chunkSize">Chunk size in bytes</param>
        /// <returns>Last chunk index and length in bytes</returns>
        public static (long LastChunk, int LastChunkLength) CalculateLastChunkAndLength(in long len, in int chunkSize)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(len, nameof(len));
            ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, other: 1, nameof(chunkSize));
            long chunk = (long)Math.Floor((decimal)len / chunkSize),
                length = len % chunkSize;
            if (length < 1 && chunk > 0)
            {
                chunk--;
                length = chunkSize;
            }
            return (chunk, (int)length);
        }

        /// <summary>
        /// Calculate the length of the chunk meta data from a data length (excluding chunk meta data)
        /// </summary>
        /// <param name="len">Data (excluding chunk meta data) length in bytes</param>
        /// <param name="chunkSize">Chunk size in bytes</param>
        /// <param name="useChunkSizeHeader">If the chunk size is going to be used as header</param>
        /// <returns>Chunk meta data length in bytes</returns>
        public static long CalculateChunkMetaDataLength(in long len, in int chunkSize, in bool useChunkSizeHeader = true)
        {
            (long chunk, int length) = CalculateLastChunkAndLength(len, chunkSize);
            long res = (chunk + 1) * sizeof(byte);
            if (useChunkSizeHeader) res += sizeof(int);
            if (length != chunkSize) res += sizeof(int);
            return res;
        }
    }
}

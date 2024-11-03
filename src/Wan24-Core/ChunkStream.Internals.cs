namespace wan24.Core
{
    // Internals
    public partial class ChunkStream<tStream, tFinal>
    {
        /// <summary>
        /// Chunk buffer
        /// </summary>
        protected readonly MemoryPoolStream? Buffer;
        /// <summary>
        /// Current base stream byte offset (required in case the base stream isn't seekable)
        /// </summary>
        protected long _Position = 0;
        /// <summary>
        /// Current base stream length in bytes (required in case the base stream isn't seekable; if <c>-1</c>, the chunk stream is read-only and the base stream isn't 
        /// seekable also (getting the length is impossible in that case, unless the stream was red to the end))
        /// </summary>
        protected long _Length = -1;

        /// <summary>
        /// Get the data byte offset of a byte position excluding chunk meta data
        /// </summary>
        /// <param name="pos">Stream byte offset</param>
        /// <returns>Data byte offset</returns>
        protected long GetDataOffset(in long pos)
        {
            if (!RedAll) throw new InvalidOperationException();
            ArgumentOutOfRangeException.ThrowIfNegative(pos, nameof(pos));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(pos, _Length, nameof(pos));
            long offset = pos;
            if (WriteChunkSizeHeader)
            {
                if (offset < sizeof(int)) throw new ArgumentOutOfRangeException(nameof(pos), $"Offset {offset} within stream header");
                offset -= sizeof(int);
            }
            long chunk = -1,
                lastChunk = LastChunk - 1;
            int chunkSize = sizeof(byte) + ChunkSize;
            for (; offset >= chunkSize && chunk < lastChunk; chunk++, offset -= chunkSize) ;
            if (chunk < 0)
            {
                offset -= sizeof(byte);
                if (LastChunk == 0 && LastChunkLength != ChunkSize) offset -= sizeof(int);
                if (offset < 0) throw new ArgumentOutOfRangeException(nameof(pos), $"Offset {offset} within chunk header");
                return offset;
            }
            chunk++;
            if (chunk < LastChunk) return chunk * ChunkSize + offset;
            offset -= sizeof(byte);
            if (LastChunkLength == ChunkSize)
            {
                if (offset != ChunkSize) throw new InvalidProgramException($"Last chunk offset {offset} should be equal to the chunk size");
                return (chunk + 1) * ChunkSize;
            }
            offset -= sizeof(int);
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(pos), $"Last chunk offset {offset} within chunk header");
            if (offset > ChunkSize)
                throw new InvalidProgramException($"Last chunk offset {offset} exceeds chunk size");
            return chunk * ChunkSize + offset;
        }

        /// <summary>
        /// Get the stream byte position of a data byte position excluding chunk meta data
        /// </summary>
        /// <param name="pos">Data byte offset</param>
        /// <returns>Stream byte offset</returns>
        protected long GetStreamPosition(in long pos)
        {
            if (!RedAll) throw new InvalidOperationException();
            long chunk = CalculateLastChunkAndLength(pos, ChunkSize).LastChunk,
                res = pos + CalculateChunkMetaDataLength(pos, ChunkSize, WriteChunkSizeHeader);
            if (chunk != LastChunk || LastChunkLength == ChunkSize) res -= sizeof(int);
            if (res > _Length) throw new ArgumentOutOfRangeException(nameof(pos), "Data byte offset exceeds stream length");
            return res;
        }

        /// <summary>
        /// Set <see cref="CurrentChunk"/>, <see cref="CurrentChunkLength"/> and <see cref="CurrentChunkPosition"/> from a data byte offset
        /// </summary>
        /// <param name="pos">Data byte offset</param>
        protected void SetChunkOffset(in long pos)
        {
            if (!RedAll || !IsFlushedFinally) throw new InvalidOperationException();
            (long chunk, int offset) = CalculateLastChunkAndLength(pos, ChunkSize);
            if (chunk > LastChunk) throw new ArgumentOutOfRangeException(nameof(pos), "Last chunk exceeded");
            if (chunk == LastChunk && offset > LastChunkLength) throw new ArgumentOutOfRangeException(nameof(pos), "Last chunk length exceeded");
            (CurrentChunk, CurrentChunkPosition) = (chunk, offset);
            CurrentChunkLength = CurrentChunk == LastChunk
                ? LastChunkLength
                : ChunkSize;
        }

        /// <summary>
        /// Flush the current chunk
        /// </summary>
        protected virtual void FlushChunk()
        {
            if (!CanWrite || LastChunkLength != ChunkSize) throw new InvalidOperationException();
            BaseStream.WriteByte((byte)ChunkTypes.Full);
            Buffer!.Position = 0;
            Buffer.CopyTo(BaseStream);
            Buffer.SetLength(0);
            CurrentChunk = ++LastChunk;
            CurrentChunkPosition = LastChunkLength = 0;
            _Length = ++_Position;
            RaiseOnChunkWritten(LastChunk - 1, ChunkTypes.Full, ChunkSize);
        }

        /// <summary>
        /// Flush the current chunk
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task FlushChunkAsync(CancellationToken cancellationToken = default)
        {
            if (!CanWrite || LastChunkLength != ChunkSize) throw new InvalidOperationException();
            BaseStream.WriteByte((byte)ChunkTypes.Full);
            Buffer!.Position = 0;
            await Buffer.CopyToAsync(BaseStream, cancellationToken).DynamicContext();
            Buffer.SetLength(0);
            CurrentChunk = ++LastChunk;
            CurrentChunkPosition = LastChunkLength = 0;
            _Length = ++_Position;
            RaiseOnChunkWritten(LastChunk - 1, ChunkTypes.Full, ChunkSize);
        }

        /// <summary>
        /// Read the next chunk header
        /// </summary>
        protected virtual void ReadChunkHeader()
        {
            if (!CanRead) throw new InvalidOperationException();
            int temp = BaseStream.ReadByte();
            if (temp < 0)
                throw new InvalidDataException("Failed to read chunk type");
            _Position++;
            ChunkTypes type = (ChunkTypes)temp;
            bool isFinal = false;
            switch (type)
            {
                case ChunkTypes.Full:
                    CurrentChunkLength = ChunkSize;
                    break;
                case ChunkTypes.Full | ChunkTypes.Final:
                    isFinal = true;
                    CurrentChunkLength = ChunkSize;
                    break;
                case ChunkTypes.Partial | ChunkTypes.Final:
                    isFinal = true;
                    using (RentedMemoryRef<byte> buffer = new(sizeof(int), clean: false))
                    {
                        BaseStream.ReadExactly(buffer.Span);
                        _Position += sizeof(int);
                        CurrentChunkLength = buffer.Span.ToInt();
                        if (CurrentChunkLength < 1) throw new InvalidDataException($"Invalid final chunk length {CurrentChunkLength}");
                    }
                    break;
                default:
                    throw new InvalidDataException($"Invalid chunk type {type}");
            }
            CurrentChunk++;
            CurrentChunkPosition = 0;
            if (!RedAll)
            {
                LastChunk++;
                LastChunkLength = CurrentChunkLength;
                if (isFinal)
                {
                    IsFinalChunk = true;
                    RedAll = true;
                    long len = _Position + LastChunkLength;
                    if (_Length >= 0 && len > _Length) throw new InvalidDataException("Last chunk length exceeds stream length");
                    _Length = len;
                }
            }
        }

        /// <summary>
        /// Read the next chunk header
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task ReadChunkHeaderAsync(CancellationToken cancellationToken)
        {
            if (!CanRead) throw new InvalidOperationException();
            int temp = BaseStream.ReadByte();
            if (temp < 0)
                throw new InvalidDataException("Failed to read chunk type");
            _Position++;
            ChunkTypes type = (ChunkTypes)temp;
            bool isFinal = false;
            switch (type)
            {
                case ChunkTypes.Full:
                    CurrentChunkLength = ChunkSize;
                    break;
                case ChunkTypes.Full | ChunkTypes.Final:
                    isFinal = true;
                    CurrentChunkLength = ChunkSize;
                    break;
                case ChunkTypes.Partial | ChunkTypes.Final:
                    isFinal = true;
                    using (RentedMemory<byte> buffer = new(sizeof(int), clean: false))
                    {
                        await BaseStream.ReadExactlyAsync(buffer.Memory, cancellationToken).DynamicContext();
                        _Position += sizeof(int);
                        CurrentChunkLength = buffer.Memory.Span.ToInt();
                        if (CurrentChunkLength < 1) throw new InvalidDataException($"Invalid final chunk length {CurrentChunkLength}");
                    }
                    break;
                default:
                    throw new InvalidDataException($"Invalid chunk type {type}");
            }
            CurrentChunk++;
            CurrentChunkPosition = 0;
            if (!RedAll)
            {
                LastChunk++;
                LastChunkLength = CurrentChunkLength;
                if (isFinal)
                {
                    IsFinalChunk = true;
                    RedAll = true;
                    long len = _Position + LastChunkLength;
                    if (_Length >= 0 && len > _Length) throw new InvalidDataException("Last chunk length exceeds stream length");
                    _Length = len;
                }
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Buffer?.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            Buffer?.Dispose();
        }
    }
}

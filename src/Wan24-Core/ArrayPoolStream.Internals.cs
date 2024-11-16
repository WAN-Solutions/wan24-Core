using System.Buffers;

namespace wan24.Core
{
    // Internals
    public partial class ArrayPoolStream
    {
        /// <summary>
        /// Buffers
        /// </summary>
        protected readonly List<byte[]> Buffers;
        /// <summary>
        /// Total buffer lengths in bytes
        /// </summary>
        protected readonly List<long> TotalBufferLengths;
        /// <summary>
        /// If to return the first buffer to the pool when disposing
        /// </summary>
        protected readonly bool ReturnFirstBuffer = true;
        /// <summary>
        /// Pool
        /// </summary>
        protected readonly ArrayPool<byte> Pool;
        /// <summary>
        /// Buffer sequence
        /// </summary>
        protected ReadOnlySequence<byte> BufferSequence = ReadOnlySequence<byte>.Empty;
        /// <summary>
        /// If having a new length, and the <see cref="BufferSequence"/> is invalid
        /// </summary>
        protected bool HasLengthChanged = true;
        /// <summary>
        /// Buffer index (current reading/writing offset)
        /// </summary>
        protected int BufferIndex = 0;
        /// <summary>
        /// Buffer byte offset (current reading/writing offset)
        /// </summary>
        protected int BufferOffset = 0;
        /// <summary>
        /// Last buffer index
        /// </summary>
        protected int _LastBufferIndex = 0;
        /// <summary>
        /// Last buffer byte offset (EOF)
        /// </summary>
        protected int _LastBufferOffset = 0;
        /// <summary>
        /// Buffer size in bytes
        /// </summary>
        protected int _BufferSize = _DefaultBufferSize;
        /// <summary>
        /// Length in bytes
        /// </summary>
        protected int _Length = 0;
        /// <summary>
        /// Byte offset
        /// </summary>
        protected int _Position = 0;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            ReturnBuffersOnDisposing();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            ReturnBuffersOnDisposing();
        }

        /// <summary>
        /// Update the buffer indexes and offsets after writing and ensure there's buffer space for writing more
        /// </summary>
        protected void UpdateBufferOffsetAfterWriting()
        {
            List<byte[]> buffers = Buffers;
            int bufferIndex = BufferIndex,
                bufferOffset = BufferOffset,
                lastBufferIndex = _LastBufferIndex;
            // Write to the next buffer, if possible
            if (bufferOffset >= buffers[bufferIndex].Length)
            {
                BufferIndex = ++bufferIndex;
                BufferOffset = bufferOffset = 0;
            }
            // Add a new buffer, if required
            if (bufferIndex >= buffers.Count)
            {
                byte[] buffer = Pool.Rent(BufferSize);
                buffers.Add(buffer);
                List<long> totalBufferLengths = TotalBufferLengths;
                totalBufferLengths.Add(totalBufferLengths[^1] + buffer.Length);
            }
            // Update the last buffer index and offset, if required
            if (bufferIndex < lastBufferIndex) return;
            if (bufferIndex > lastBufferIndex)
            {
                _LastBufferIndex = bufferIndex;
                _LastBufferOffset = bufferOffset;
            }
            else if (bufferOffset > _LastBufferOffset)
            {
                _LastBufferOffset = bufferOffset;
            }
        }

        /// <summary>
        /// Set a new length
        /// </summary>
        /// <param name="value">New length in bytes</param>
        /// <param name="clear">Clear new buffers?</param>
        protected void SetLength(int value, in bool clear)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            if (value == int.MaxValue) throw new OutOfMemoryException();
            int length = _Length;
            if (value == length) return;
            if (value == 0)
            {
                // Delete all data
                (_Length, _Position, BufferIndex, BufferOffset, _LastBufferOffset) = (0, 0, 0, 0, 0);
                if (_LastBufferIndex != 0)
                {
                    _LastBufferIndex = 0;
                    if (ReturnUnusedBuffers) Optimize();
                }
                BufferSequence = ReadOnlySequence<byte>.Empty;
                HasLengthChanged = false;
                return;
            }
            List<long> totalBufferLengths = TotalBufferLengths;
            if (value < totalBufferLengths[^1])
            {
                // Delete some data as required
                int prevLastBufferIndex = _LastBufferIndex,
                    prevLastBufferOffset = _LastBufferOffset;
                (int lastBufferIndex, int lastBufferOffset) = GetBufferIndexAndOffset(value);
                (_LastBufferIndex, _LastBufferOffset) = (lastBufferIndex, lastBufferOffset);
                _Length = value;
                if (_Position > value)
                {
                    _Position = value;
                    (BufferIndex, BufferOffset) = (lastBufferIndex, lastBufferOffset);
                }
                if (prevLastBufferIndex < lastBufferIndex && ReturnUnusedBuffers) Optimize();
                if (clear && value > length)
                {
                    List<byte[]> buffers = Buffers;
                    byte[] buffer;
                    value -= length;
                    for (int i = prevLastBufferIndex, bufferLen, offset, count; i <= lastBufferIndex && value > 0; value -= count, i++)
                    {
                        buffer = buffers[i];
                        bufferLen = buffer.Length;
                        offset = i == prevLastBufferIndex
                            ? prevLastBufferOffset
                            : 0;
                        count = i == lastBufferIndex
                            ? lastBufferOffset - offset
                            : bufferLen - offset;
                        buffer.AsSpan(offset, count).Clear();
                    }
                }
            }
            else
            {
                // Add buffers as required
                List<byte[]> buffers = Buffers;
                ArrayPool<byte> pool = Pool;
                int add = value - length,
                    lastBufferIndex = _LastBufferIndex,
                    lastBufferOffset = _LastBufferOffset,
                    lastBufferChunk,
                    bufferSize = BufferSize,
                    bufferIndex,
                    bufferLen;
                byte[] buffer = buffers[lastBufferIndex];
                bufferLen = buffer.Length;
                lastBufferChunk = bufferLen - lastBufferOffset;
                if (clear) Array.Clear(buffer, lastBufferOffset, lastBufferChunk);
                add -= lastBufferChunk;
                bufferIndex = lastBufferIndex;
                for (int len = buffers.Count; ; add -= lastBufferOffset)
                {
                    bufferIndex++;
                    if (bufferIndex == len)
                    {
                        // Add a new buffer
                        buffer = pool.Rent(bufferSize);
                        bufferLen = buffer.Length;
                        buffers.Add(buffer);
                        totalBufferLengths.Add(totalBufferLengths[^1] + bufferLen);
                        len++;
                    }
                    else
                    {
                        // Use the next spare buffer
                        buffer = buffers[bufferIndex];
                        bufferLen = buffer.Length;
                    }
                    lastBufferOffset = Math.Min(add, bufferLen);
                    if (clear) (lastBufferOffset == bufferLen ? buffer : buffer.AsSpan(0, add)).Clear();
                    if (lastBufferOffset == add) break;
                }
                (_LastBufferIndex, _LastBufferOffset) = (bufferIndex, lastBufferOffset);
                _Length = value;
                (BufferIndex, BufferOffset) = GetBufferIndexAndOffset(_Position);
                UpdateBufferOffsetAfterWriting();
            }
            HasLengthChanged = true;
        }

        /// <summary>
        /// Get the last used buffer index for a stream byte offset
        /// </summary>
        /// <param name="offset">Stream byte offset</param>
        /// <returns>Buffer index or <c>-1</c>, if the <c>len</c> exceeds the currently used buffers total length</returns>
        protected int GetBufferIndex(in long offset)
        {
            List<long> totalBufferLengths = TotalBufferLengths;
            long total;
            for (int res = 0, buffers = totalBufferLengths.Count; res < buffers; res++)
            {
                total = totalBufferLengths[res];
                if (total < offset) continue;
                return total > offset
                    ? res
                    : res + 1;
            }
            return -1;
        }

        /// <summary>
        /// Get the last used buffer index for a stream byte offset
        /// </summary>
        /// <param name="offset">Stream byte offset</param>
        /// <returns>Buffer index or <c>-1</c>, if the <c>len</c> exceeds the currently used buffers total length, and the offset (or <c>-1</c>)</returns>
        protected (int BufferIndex, int BufferOffset) GetBufferIndexAndOffset(in long offset)
        {
            List<long> totalBufferLengths = TotalBufferLengths;
            long total;
            for (int res = 0, buffers = totalBufferLengths.Count; res < buffers; res++)
            {
                total = totalBufferLengths[res];
                if (total < offset) continue;
                return total > offset
                    ? (res, (int)(offset - total + Buffers[res].Length))
                    : (res + 1, 0);
            }
            return (-1, -1);
        }

        /// <summary>
        /// Update the <see cref="BufferSequence"/> (<see cref="HasLengthChanged"/> signals an invalid <see cref="BufferSequence"/>)
        /// </summary>
        /// <returns>Sequence</returns>
        protected ReadOnlySequence<byte> UpdateBufferSequence()
        {
            if (!HasLengthChanged) return BufferSequence;
            try
            {
                ReadOnlySequence<byte> res;
                if (_Length == 0)
                {
                    BufferSequence = res = ReadOnlySequence<byte>.Empty;
                    return res;
                }
                int len = Buffers.Count - 1;
                List<byte[]> buffers = Buffers;
                if (len == 0)
                {
                    BufferSequence = res = new(buffers[0].AsMemory(0, _LastBufferOffset));
                }
                else
                {
                    int lastBufferOffset = _LastBufferOffset;
                    MemorySequenceSegment<byte> first = new(buffers[0]),
                        last = first;
                    for (int i = 1; i <= len; last = last.Append(i == len ? buffers[i].AsMemory(0, lastBufferOffset) : buffers[i]), i++) ;
                    BufferSequence = res = new(first, startIndex: 0, last, endIndex: lastBufferOffset);
                }
                return res;
            }
            finally
            {
                HasLengthChanged = false;
            }
        }

        /// <summary>
        /// Return buffers when disposing
        /// </summary>
        private void ReturnBuffersOnDisposing()
        {
            ArrayPool<byte> pool = Pool;
            List<byte[]> buffers = Buffers;
            if (CleanReturned)
            {
                byte[] buffer;
                for (int i = ReturnFirstBuffer ? 0 : 1, len = buffers.Count; i < len; buffer = buffers[i], buffer.Clear(), pool.Return(buffer), i++) ;
            }
            else
            {
                for (int i = ReturnFirstBuffer ? 0 : 1, len = buffers.Count; i < len; pool.Return(buffers[i]), i++) ;
            }
            buffers.Clear();
            TotalBufferLengths.Clear();
        }
    }
}

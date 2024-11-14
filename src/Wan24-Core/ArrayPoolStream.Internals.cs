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
        /// If having updates, and the <see cref="BufferSequence"/> is invalid
        /// </summary>
        protected bool HasUpdates = true;
        /// <summary>
        /// Buffer index
        /// </summary>
        protected int BufferIndex = 0;
        /// <summary>
        /// Buffer byte offset
        /// </summary>
        protected int BufferOffset = 0;
        /// <summary>
        /// Last buffer byte offset
        /// </summary>
        protected int LastBufferOffset = 0;
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
            byte[] buffer;
            for (int i = ReturnFirstBuffer ? 0 : 1, len = Buffers.Count; i < len; i++)
            {
                buffer = Buffers[i];
                if (CleanReturned) buffer.Clear();
                Pool.Return(buffer);
            }
            Buffers.Clear();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            byte[] buffer;
            for (int i = ReturnFirstBuffer ? 0 : 1, len = Buffers.Count; i < len; i++)
            {
                buffer = Buffers[i];
                if (CleanReturned) buffer.Clear();
                Pool.Return(buffer);
            }
            Buffers.Clear();
            await base.DisposeCore().DynamicContext();
        }

        /// <summary>
        /// Update the buffer offset after reading
        /// </summary>
        protected void UpdateBufferOffsetAfterReading()
        {
            if (BufferIndex == Buffers.Count - 1 || BufferOffset != Buffers[BufferIndex].Length) return;
            BufferIndex++;
            BufferOffset = 0;
        }

        /// <summary>
        /// Update the buffer offset after writing
        /// </summary>
        protected void UpdateBufferOffsetAfterWriting()
        {
            // Inline
            if (BufferIndex != Buffers.Count - 1)
            {
                if (BufferOffset != Buffers[BufferIndex].Length) return;
                // Increase the buffer index
                BufferIndex++;
                BufferOffset = 0;
                return;
            }
            // Ensure a correct last buffer offset
            if (BufferOffset <= LastBufferOffset) return;
            LastBufferOffset = BufferOffset;
            HasUpdates = true;
            // Ensure the last buffer has space left (or add a new buffer)
            if (LastBufferOffset != Buffers[BufferIndex].Length) return;
            BufferIndex++;
            if (BufferIndex >= Buffers.Count) Buffers.Add(Pool.Rent(BufferSize));
            BufferOffset = LastBufferOffset = 0;
        }

        /// <summary>
        /// Set a new length
        /// </summary>
        /// <param name="value">New length in bytes</param>
        /// <param name="clear">Clear new buffers?</param>
        protected void SetLength(in int value, in bool clear)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfEqual(value, int.MaxValue, nameof(value));
            if (value == _Length) return;
            if (value == 0)
            {
                // Delete all data
                if (ReturnUnusedBuffers)
                {
                    byte[] buffer;
                    for (int i = 1, len = Buffers.Count; i < len; i++)
                    {
                        buffer = Buffers[i];
                        if (CleanReturned) buffer.Clear();
                        Pool.Return(buffer);
                    }
                    if (Buffers.Count > 1) Buffers.RemoveRange(1, Buffers.Count - 1);
                }
                _Length = _Position = BufferIndex = BufferOffset = LastBufferOffset = 0;
                BufferSequence = ReadOnlySequence<byte>.Empty;
                HasUpdates = false;
            }
            else if (value < _Length)
            {
                // Delete some data
                int pos = 0,
                    add,
                    len;
                for (BufferIndex = 0; ; BufferIndex++)
                {
                    len = Buffers[BufferIndex].Length;
                    add = Math.Min(len, value - pos);
                    pos += add;
                    if (add == len && pos != value) continue;
                    BufferOffset = add;
                    break;
                }
                LastBufferOffset = BufferOffset;
                len = Buffers.Count;
                if (ReturnUnusedBuffers && len > BufferIndex + 1)
                {
                    byte[] buffer;
                    for (int i = BufferIndex + 1; i < len; i++)
                    {
                        buffer = Buffers[i];
                        if (CleanReturned) buffer.Clear();
                        Pool.Return(buffer);
                    }
                    Buffers.RemoveRange(BufferIndex + 1, len - BufferIndex - 1);
                }
                _Length = value;
                if (_Position > value)
                {
                    _Position = value;
                }
                else if (_Position != value)
                {
                    long oldPos = _Position;
                    _Position = -1;
                    Position = oldPos;
                }
                HasUpdates = true;
            }
            else
            {
                // Add buffers, if required
                int len = value - _Length,
                    lastIndex = LastBufferIndex;
                Span<byte> buffer = Buffers[lastIndex];
                len -= Math.Min(buffer.Length - LastBufferOffset, len);
                if (len == 0)
                {
                    // Still enough space in the buffer
                    int add = value - _Length;
                    if (clear) Buffers[lastIndex].AsSpan(LastBufferOffset, add).Clear();
                    LastBufferOffset += add;
                }
                else
                {
                    // Add new buffers as required
                    BufferIndex = lastIndex;
                    for (byte[] buff; len > 0; len -= LastBufferOffset)
                    {
                        BufferIndex++;
                        if (Buffers.Count <= BufferIndex)
                        {
                            buff = Pool.Rent(BufferSize);
                            Buffers.Add(buff);
                        }
                        else
                        {
                            buff = Buffers[BufferIndex];
                        }
                        LastBufferOffset = Math.Min(len, buff.Length);
                        if (clear) buff.AsSpan(0, LastBufferOffset).Clear();
                    }
                }
                UpdateBufferOffsetAfterWriting();
                _Length = value;
                long oldPos = _Position;
                _Position = -1;
                Position = oldPos;
                HasUpdates = true;
            }
        }

        /// <summary>
        /// Update the <see cref="BufferSequence"/> (<see cref="HasUpdates"/> signals an invalid <see cref="BufferSequence"/>)
        /// </summary>
        /// <returns>Sequence</returns>
        protected ReadOnlySequence<byte> UpdateBufferSequence()
        {
            if (_Length == 0)
            {
                BufferSequence = ReadOnlySequence<byte>.Empty;
                HasUpdates = false;
                return BufferSequence;
            }
            int len = Buffers.Count - 1;
            if (len == 0)
            {
                BufferSequence = new(Buffers[0].AsMemory(0, LastBufferOffset));
                HasUpdates = false;
                return BufferSequence;
            }
            MemorySequenceSegment<byte> first = new(Buffers[0]),
                last = first;
            for (int i = 1; i <= len; last = last.Append(i == len ? Buffers[i].AsMemory(0, LastBufferOffset) : Buffers[i]), i++) ;
            BufferSequence = new(first, startIndex: 0, last, endIndex: LastBufferOffset);
            HasUpdates = false;
            return BufferSequence;
        }
    }
}

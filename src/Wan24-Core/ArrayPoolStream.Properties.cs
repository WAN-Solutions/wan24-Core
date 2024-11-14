using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    // Properties
    public partial class ArrayPoolStream
    {
        /// <summary>
        /// Buffer size in bytes
        /// </summary>
        public int BufferSize
        {
            get => _BufferSize;
            set
            {
                EnsureUndisposed();
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
                _BufferSize = value;
            }
        }

        /// <summary>
        /// Total buffer length in bytes
        /// </summary>
        public long BufferLength => Buffers.Sum(b => (long)b.Length);

        /// <summary>
        /// Current number of buffers
        /// </summary>
        public int BufferCount => Buffers.Count;

        /// <summary>
        /// Index of the last buffer
        /// </summary>
        public int LastBufferIndex
        {
            get
            {
                EnsureUndisposed(allowDisposing: true);
                for (int i = 0, len = Buffers.Count, total = 0; i < len; i++)
                {
                    total += Buffers[i].Length;
                    if (total >= _Length) return i;
                }
                return Buffers.Count - 1;
            }
        }

        /// <summary>
        /// Clean returned buffers?
        /// </summary>
        public bool CleanReturned { get; set; } = Settings.ClearBuffers;

        /// <summary>
        /// If to return unused buffers to the pool
        /// </summary>
        public bool ReturnUnusedBuffers { get; set; } = true;

        /// <summary>
        /// Save the data on close?
        /// </summary>
        public bool SaveOnClose { get; set; }

        /// <summary>
        /// Saved data
        /// </summary>
        public byte[]? SavedData { get; protected set; }

        /// <inheritdoc/>
        public virtual IEnumerable<Status> State
        {
            get
            {
                yield return new(__("Name"), Name, __("Name of the stream"));
                yield return new(__("Type"), GetType(), __("CLR type"));
                if (StackInfo is not null)
                {
                    yield return new(__("Stack"), StackInfo.Stack, __("Instance creation stack"));
                    yield return new(__("Created"), StackInfo.Created, __("Instance creation time"));
                }
                yield return new(__("Length"), Length, __("Stream length in bytes"));
                yield return new(__("Position"), Position, __("Stream byte offset"));
                yield return new(__("Return"), ReturnUnusedBuffers, __("If unused buffers are returned to the pool"));
                yield return new(__("Buffers"), BufferCount, __("Number of buffers"));
                yield return new(__("Index"), LastBufferIndex, __("Zero based index of the last buffer"));
                yield return new(__("Buffer"), BufferLength, __("All buffers total length in bytes"));
                yield return new(__("Buffer size"), BufferSize, __("New buffer size in bytes"));
                yield return new(__("Current"), BufferIndex, __("Zero based index of the current buffer"));
                yield return new(__("Offset"), BufferOffset, __("Byte offset of the current buffer"));
                yield return new(__("Last offset"), LastBufferOffset, __("Byte offset of the last buffer"));
            }
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Length => _Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => _Position;
            set
            {
                EnsureUndisposed();
                ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(value));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other: _Length, nameof(value));
                if (value == _Position) return;
                int v = (int)value;
                _Position = v;
                // Find buffer index and byte offset
                if (v == 0)
                {
                    BufferIndex = 0;
                    BufferOffset = 0;
                }
                else
                {
                    BufferIndex = 0;
                    for (int pos = 0, add, len; ; BufferIndex++)
                    {
                        len = Buffers[BufferIndex].Length;
                        add = Math.Min(len, v - pos);
                        pos += add;
                        if (add == len && pos != v) continue;
                        BufferOffset = add;
                        break;
                    }
                    UpdateBufferOffsetAfterReading();
                }
            }
        }
    }
}

using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    // Properties
    public partial class FreezableArrayPoolStream
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
        public long BufferLength => TotalBufferLengths[^1];

        /// <summary>
        /// Current number of buffers
        /// </summary>
        public int BufferCount => Buffers.Count;

        /// <summary>
        /// Index of the last used buffer
        /// </summary>
        public int LastBufferIndex => _LastBufferIndex;

        /// <summary>
        /// Byte offset in the last used buffer
        /// </summary>
        public int LastBufferOffset => _LastBufferOffset;

        /// <summary>
        /// If to clean returned buffers
        /// </summary>
        public bool CleanReturned { get; set; } = Settings.ClearBuffers;

        /// <summary>
        /// If to return unused buffers to the pool (otherwise they'll be kept within this stream for later writing operations)
        /// </summary>
        public bool ReturnUnusedBuffers { get; set; } = true;

        /// <summary>
        /// Save the data to <see cref="SavedData"/> on close?
        /// </summary>
        public bool SaveOnClose { get; set; }

        /// <summary>
        /// Saved data (available after this stream was closed, if <see cref="SaveOnClose"/> was <see langword="true"/>)
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
                yield return new(__("Length"), _Length, __("Stream length in bytes"));
                yield return new(__("Position"), _Position, __("Stream byte offset"));
                yield return new(__("Buffer size"), BufferSize, __("New buffer size in bytes"));
                yield return new(__("Buffers"), Buffers.Count, __("Number of buffers"));
                yield return new(__("Buffer"), TotalBufferLengths[^1], __("All buffers total length in bytes"));
                yield return new(__("Index"), LastBufferIndex, __("Zero based index of the last used buffer"));
                yield return new(__("Last offset"), _LastBufferOffset, __("Byte offset of the last used buffer"));
                yield return new(__("Current"), BufferIndex, __("Zero based index of the current buffer"));
                yield return new(__("Offset"), BufferOffset, __("Byte offset of the current buffer"));
                yield return new(__("Clean"), CleanReturned, __("If returned buffers are being cleaned"));
                yield return new(__("Return"), ReturnUnusedBuffers, __("If unused buffers are being returned to the pool"));
                yield return new(__("Frozen"), Buffers.IsFrozen, __("If the buffers are frozen"));
            }
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => !Buffers.IsFrozen;

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
                if (value == 0)
                {
                    (_Position, BufferIndex, BufferOffset) = (0, 0, 0);
                }
                else
                {
                    _Position = (int)value;
                    (BufferIndex, BufferOffset) = GetBufferIndexAndOffset(value);
                }
            }
        }
    }
}

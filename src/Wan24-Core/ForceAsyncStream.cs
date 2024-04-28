namespace wan24.Core
{
    /// <summary>
    /// Forces all operations to be executed asynchronous
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class ForceAsyncStream(in Stream baseStream, in bool leaveOpen = false) : ForceAsyncStream<Stream>(baseStream, leaveOpen)
    {
    }

    /// <summary>
    /// Forces all operations to be executed asynchronous
    /// </summary>
    /// <typeparam name="T">Base stream type</typeparam>
    public class ForceAsyncStream<T> : WrapperStream, IDisposable where T : Stream
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing</param>
        public ForceAsyncStream(in T baseStream, in bool leaveOpen = false) : base(baseStream, leaveOpen)
        {
            UseOriginalByteIO = true;
            UseOriginalCopyTo = true;
        }

        /// <summary>
        /// Clear buffers after use?
        /// </summary>
        public bool ClearBuffers { get; set; }

        /// <inheritdoc/>
        public override void Flush()
        {
            EnsureUndisposed(allowDisposing: true);
            FlushAsync().GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            using RentedArrayStructSimple<byte> temp = new(buffer.Length, clean: false)
            {
                Clear = ClearBuffers
            };
            buffer.CopyTo(temp.Span);
            int res = ReadAsync(temp.Memory).AsTask().GetAwaiter().GetResult();
            if (res > 0) temp.Span[..res].CopyTo(buffer);
            return res;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            EnsureReadable();
            using RentedArrayStructSimple<byte> buffer = new(len: sizeof(byte), clean: false)
            {
                Clear = ClearBuffers
            };
            int red = ReadAsync(buffer.Memory).AsTask().GetAwaiter().GetResult();
            return red < 1 ? -1 : buffer.Span[0];
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => WriteAsync(buffer, offset, count).GetAwaiter().GetResult();

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            using RentedArrayStructSimple<byte> temp = new(buffer.Length, clean: false)
            {
                Clear = ClearBuffers
            };
            buffer.CopyTo(temp.Span);
            WriteAsync(temp.Memory).AsTask().GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            EnsureWritable();
            using RentedArrayStructSimple<byte> buffer = new(len: sizeof(byte), clean: false)
            {
                Clear = ClearBuffers
            };
            buffer.Span[0] = value;
            WriteAsync(buffer.Memory).AsTask().GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
#pragma warning disable CA1816 // Suppress finalize
        void IDisposable.Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
#pragma warning restore CA1816 // Suppress finalize

        /// <inheritdoc/>
        new public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposing)
            {
                DisposeCore().GetAwaiter().GetResult();
                base.Dispose(disposing);
            }
            else
            {
                DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
        }
    }
}

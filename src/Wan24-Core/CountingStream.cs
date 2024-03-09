using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Counting stream counts red/written bytes
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class CountingStream(in Stream baseStream, in bool leaveOpen = false) : CountingStream<Stream>(baseStream, leaveOpen)
    {
    }

    /// <summary>
    /// Counting stream counts red/written bytes
    /// </summary>
    /// <typeparam name="T">Wrapped stream type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class CountingStream<T>(in T baseStream, in bool leaveOpen = false) : WrapperStream<T>(baseStream, leaveOpen), IStatusProvider where T : Stream
    {
        /// <summary>
        /// Number of bytes red
        /// </summary>
        public virtual long Red { get; protected set; }

        /// <summary>
        /// Number of bytes written
        /// </summary>
        public virtual long Written { get; protected set; }

        /// <inheritdoc/>
        public override IEnumerable<Status> State
        {
            get
            {
                foreach (Status status in base.State) yield return status;
                yield return new(__("Red"), Red, __("Number of red bytes"));
                yield return new(__("Written"), Written, __("Number of written bytes"));
            }
        }

        /// <summary>
        /// Reset the red bytes counter
        /// </summary>
        public virtual void ResetRed()
        {
            EnsureUndisposed();
            Red = 0;
        }

        /// <summary>
        /// Reset the written bytes counter
        /// </summary>
        public virtual void ResetWritten()
        {
            EnsureUndisposed();
            Written = 0;
        }

        /// <summary>
        /// Reset the red/written counters
        /// </summary>
        public virtual void ResetCounter()
        {
            EnsureUndisposed();
            Red = 0;
            Written = 0;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);
            Written += count;
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            base.Write(buffer);
            Written += buffer.Length;
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await base.WriteAsync(buffer, offset, count, cancellationToken).DynamicContext();
            Written += count;
        }

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await base.WriteAsync(buffer, cancellationToken).DynamicContext();
            Written += buffer.Length;
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            base.WriteByte(value);
            Written++;
        }


        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int res = base.Read(buffer, offset, count);
            Red += res;
            return res;
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            int res = base.Read(buffer);
            Red += res;
            return res;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int res = await base.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
            Red += res;
            return res;
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int res = await base.ReadAsync(buffer, cancellationToken).DynamicContext();
            Red += res;
            return res;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            int res = base.ReadByte();
            if (res != -1) Red++;
            return res;
        }

        /// <inheritdoc/>
        public override void CopyTo(Stream destination, int bufferSize)
        {
            EnsureUndisposed();
            EnsureReadable();
            this.GenericCopyTo(destination, bufferSize);
        }

        /// <inheritdoc/>
        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            await this.GenericCopyToAsync(destination, bufferSize, cancellationToken: cancellationToken).DynamicContext();
        }
    }
}

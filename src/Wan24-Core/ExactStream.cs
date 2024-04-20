namespace wan24.Core
{
    /// <summary>
    /// Exact stream (tries to read exactly the given number of bytes until no bytes were red)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class ExactStream(in Stream baseStream, in bool leaveOpen = false) : ExactStream<Stream>(baseStream, leaveOpen)
    {
    }

    /// <summary>
    /// Exact stream (tries to read exactly the given number of bytes until no bytes were red)
    /// </summary>
    /// <typeparam name="T">Base stream type</typeparam>
    public class ExactStream<T> : WrapperStream<T> where T : Stream
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public ExactStream(in T baseStream, in bool leaveOpen = false) : base(baseStream, leaveOpen)
        {
            if (!baseStream.CanRead) throw new ArgumentException("Readable base stream required", nameof(baseStream));
            UseOriginalBeginRead = true;
            UseOriginalCopyTo = true;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureReadable();
            int res = base.Read(buffer, offset, count);
            if (res > 0 && res != count)
            {
                offset += res;
                for (int left = count - res, red = res; red > 0 && res != count; offset += red, left -= red, res += red)
                {
                    red = base.Read(buffer, offset, left);
                    if (red < 1) break;
                }
            }
            return res;
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            int res = base.Read(buffer);
            if (res > 0 && res != buffer.Length)
            {
                buffer = buffer[res..];
                while (buffer.Length > 0)
                {
                    int red = base.Read(buffer);
                    if (red < 1) break;
                    buffer = buffer[red..];
                    res += red;
                }
            }
            return res;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            int res = await base.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
            if (res > 0 && res != count)
            {
                offset += res;
                for (int left = count - res, red = res; red > 0 && res != count; offset += red, left -= red, res += red)
                {
                    red = await base.ReadAsync(buffer, offset, left, cancellationToken).DynamicContext();
                    if (red < 1) break;
                }
            }
            return res;
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            int res = await base.ReadAsync(buffer, cancellationToken).DynamicContext();
            if (res > 0 && res != buffer.Length)
            {
                buffer = buffer[res..];
                while (buffer.Length > 0)
                {
                    int red = await base.ReadAsync(buffer, cancellationToken).DynamicContext();
                    if (red < 1) break;
                    buffer = buffer[red..];
                    res += red;
                }
            }
            return res;
        }
    }
}

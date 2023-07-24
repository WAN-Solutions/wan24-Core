using System.Security.Cryptography;

namespace wan24.Core
{
    /// <summary>
    /// Random stream (uses <see cref="RandomNumberGenerator"/> for reading random bytes into the given buffers)
    /// </summary>
    public sealed class RandomStream : StreamBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        private RandomStream() : base() { }

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static RandomStream Instance { get; } = new();

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Flush() { }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            RandomNumberGenerator.Fill(buffer.AsSpan(offset, count));
            return count;
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            RandomNumberGenerator.Fill(buffer);
            return buffer.Length;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void CopyTo(Stream destination, int bufferSize) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}

namespace wan24.Core
{
    /// <summary>
    /// Enumerable source stream
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="enumerator">Enumerator</param>
    public class EnumerableStream(in IEnumerator<byte> enumerator) : StreamBase()
    {
        /// <summary>
        /// Enumerator
        /// </summary>
        protected readonly IEnumerator<byte> Enumerator = enumerator;
        /// <summary>
        /// Position byte offset
        /// </summary>
        protected long _Position = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerable">Enumerable</param>
        public EnumerableStream(in IEnumerable<byte> enumerable) : this(enumerable.GetEnumerator()) { }

        /// <inheritdoc/>
        public bool EndOfStream { get; protected set; }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position { get => IfUndisposed(_Position); set => throw new NotSupportedException(); }

        /// <inheritdoc/>
        public override void Flush() => EnsureUndisposed();

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            if (EndOfStream) return 0;
            int res = 0;
            while (res != buffer.Length && !(EndOfStream = !Enumerator.MoveNext()))
            {
                buffer[res] = Enumerator.Current;
                res++;
            }
            if (EndOfStream) Enumerator.Dispose();
            _Position += res;
            return res;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Enumerator.Dispose();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            Enumerator.Dispose();
            await base.DisposeCore().DynamicContext();
        }
    }
}

namespace wan24.Core
{
    /// <summary>
    /// Enumerable source stream
    /// </summary>
    public class EnumerableStream : StreamBase
    {
        /// <summary>
        /// Enumerator
        /// </summary>
        protected readonly IEnumerator<byte> Enumerator;
        /// <summary>
        /// Position byte offset
        /// </summary>
        protected long _Position = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerable">Enumerable</param>
        public EnumerableStream(IEnumerable<byte> enumerable) : this(enumerable.GetEnumerator()) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerator">Enumerator</param>
        public EnumerableStream(IEnumerator<byte> enumerator) : base() => Enumerator = enumerator;

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
            if (IsDisposing) return;
            base.Dispose(disposing);
            Enumerator.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            Enumerator.Dispose();
        }
    }
}

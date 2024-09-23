namespace wan24.Core
{
    /// <summary>
    /// Pipeline element processing result which provides a stream
    /// </summary>
    /// <param name="element">Providing element</param>
    /// <param name="stream">Stream</param>
    /// <param name="next">Next element</param>
    public class PipelineResultStream(in PipelineElementBase element, in Stream stream, in PipelineElementBase? next = null)
        : PipelineResultBase(element, next), IPipelineResultStream
    {
        /// <inheritdoc/>
        public Stream Stream { get; } = stream;

        /// <summary>
        /// If to dispose the <see cref="Stream"/> when disposing
        /// </summary>
        public bool DisposeStream { get; init; } = true;

        /// <inheritdoc/>
        public override PipelineResultBase CreateCopy(in PipelineElementBase? element = null)
            => CreateCopyAsync(element, CancellationToken.None).GetAwaiter().GetResult();

        /// <inheritdoc/>
        public override async Task<PipelineResultBase> CreateCopyAsync(PipelineElementBase? element = null, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            PooledTempStream? stream = null;
            try
            {
                stream = new PooledTempStream(Stream.CanSeek ? Stream.GetRemainingBytes() : 0);
                await Stream.CopyToAsync(stream, cancellationToken).DynamicContext();
                if (Stream.CanSeek) Stream.Position = 0;
                return element?.CreateStreamResult(stream, processInParallel: element.ProcessResultInParallel)
                    ?? Element.CreateStreamResult(stream, processInParallel: Element.ProcessResultInParallel);
            }
            catch
            {
                if (stream is not null) await stream.DisposeAsync().DynamicContext();
                throw;
            }
        }

        /// <inheritdoc/>
        public override PipelineElementBase? GetNextElement(in PipelineElementBase currentElement)
        {
            EnsureUndisposed();
            return Element.GetNextElement(Stream);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (DisposeStream) Stream.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (DisposeStream) await Stream.DisposeAsync().DynamicContext();
        }
    }
}

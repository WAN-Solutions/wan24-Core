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

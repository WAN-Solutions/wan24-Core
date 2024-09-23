namespace wan24.Core
{
    /// <summary>
    /// Pipeline stream element processing result which provides a buffer
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="element">Providing element</param>
    /// <param name="buffer">Buffer (will be disposed)</param>
    /// <param name="next">Next element</param>
    public class PipelineResultRentedBuffer(in PipelineElementBase element, in RentedArray<byte> buffer, in PipelineElementBase? next = null)
        : PipelineResultBase(element, next), IPipelineResultBuffer
    {
        /// <summary>
        /// Rented buffer (will be disposed)
        /// </summary>
        public RentedArray<byte> RentedBuffer { get; } = buffer;

        /// <inheritdoc/>
        public ReadOnlyMemory<byte> Buffer => RentedBuffer.Memory;

        /// <inheritdoc/>
        public override PipelineResultBase CreateCopy(in PipelineElementBase? element = null)
        {
            EnsureUndisposed();
            RentedArray<byte> buffer = Element.Pipeline.CreateBuffer(Buffer.Length);
            RentedBuffer.Span.CopyTo(buffer.Span);
            return element?.CreateRentedBufferResult(buffer, processInParallel: element.ProcessResultInParallel)
                ?? Element.CreateRentedBufferResult(buffer, processInParallel: Element.ProcessResultInParallel);
        }

        /// <inheritdoc/>
        public override PipelineElementBase? GetNextElement(in PipelineElementBase currentElement)
        {
            EnsureUndisposed();
            return Element.GetNextElement(Buffer);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => RentedBuffer.Dispose();

        /// <inheritdoc/>
        protected override async Task DisposeCore() => await RentedBuffer.DisposeAsync().DynamicContext();
    }
}

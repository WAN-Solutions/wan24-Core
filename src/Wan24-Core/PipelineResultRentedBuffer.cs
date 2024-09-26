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
    public class PipelineResultRentedBuffer(in PipelineElementBase element, in RentedMemory<byte> buffer, in PipelineElementBase? next = null)
        : PipelineResultBase(element, next), IPipelineResultBuffer
    {
        /// <summary>
        /// Rented buffer (will be disposed)
        /// </summary>
        public RentedMemory<byte> RentedBuffer { get; } = buffer;

        /// <inheritdoc/>
        public ReadOnlyMemory<byte> Buffer => RentedBuffer.Memory;

        /// <inheritdoc/>
        public override PipelineResultBase CreateCopy(in PipelineElementBase? element = null)
        {
            EnsureUndisposed();
            RentedMemory<byte> buffer = Element.Pipeline.CreateBuffer(Buffer.Length);
            RentedBuffer.Memory.Span.CopyTo(buffer.Memory.Span);
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
        protected override Task DisposeCore()
        {
            RentedBuffer.Dispose();
            return Task.CompletedTask;
        }
    }
}

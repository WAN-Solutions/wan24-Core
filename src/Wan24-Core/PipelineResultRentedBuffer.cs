namespace wan24.Core
{
    /// <summary>
    /// Pipeline stream element processing result which provides a buffer
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="element">Providing element</param>
    /// <param name="buffer">Buffer</param>
    /// <param name="next">Next element</param>
    public class PipelineResultRentedBuffer(in PipelineElementBase element, in RentedArray<byte> buffer, in PipelineElementBase? next = null)
        : PipelineResultBase(element, next), IPipelineResultBuffer
    {
        /// <summary>
        /// Rented buffer (will be disposed)
        /// </summary>
        public RentedArray<byte> RentedBuffer { get; } = buffer;

        /// <inheritdoc/>
        public Memory<byte> Buffer => RentedBuffer.Memory;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => RentedBuffer.Dispose();

        /// <inheritdoc/>
        protected override async Task DisposeCore() => await RentedBuffer.DisposeAsync().DynamicContext();
    }
}

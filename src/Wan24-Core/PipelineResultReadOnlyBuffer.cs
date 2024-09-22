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
    public class PipelineResultReadOnlyBuffer(in PipelineElementBase element, in ReadOnlyMemory<byte> buffer, in PipelineElementBase? next = null)
        : PipelineResultBase(element, next), IPipelineResultBuffer
    {
        /// <inheritdoc/>
        public ReadOnlyMemory<byte> Buffer { get; } = buffer;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) { }

        /// <inheritdoc/>
        protected override Task DisposeCore() => Task.CompletedTask;
    }
}

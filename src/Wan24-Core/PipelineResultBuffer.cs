﻿namespace wan24.Core
{
    /// <summary>
    /// Pipeline stream element processing result which provides a buffer
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="element">Providing element</param>
    /// <param name="buffer">Buffer (may be cleared)</param>
    /// <param name="next">Next element</param>
    public class PipelineResultBuffer(in PipelineElementBase element, in Memory<byte> buffer, in PipelineElementBase? next = null)
        : PipelineResultBase(element, next), IPipelineResultBuffer
    {
        /// <summary>
        /// Buffer (may be cleared)
        /// </summary>
        protected readonly Memory<byte> _Buffer = buffer;

        /// <inheritdoc/>
        public ReadOnlyMemory<byte> Buffer => _Buffer;

        /// <summary>
        /// Clean the <see cref="Buffer"/> when disposing?
        /// </summary>
        public bool CleanBuffer { get; init; } = true;

        /// <inheritdoc/>
        public override PipelineResultBase CreateCopy(in PipelineElementBase? element = null)
        {
            EnsureUndisposed();
            RentedMemory<byte> buffer = Element.Pipeline.CreateBuffer(_Buffer.Length);
            _Buffer.Span.CopyTo(buffer.Memory.Span);
            return element?.CreateRentedBufferResult(buffer, processInParallel: element.ProcessResultInParallel)
                ?? Element.CreateRentedBufferResult(buffer, processInParallel: Element.ProcessResultInParallel);
        }

        /// <inheritdoc/>
        public override PipelineElementBase? GetNextElement(in PipelineElementBase currentElement)
        {
            EnsureUndisposed();
            return Element.GetNextElement(_Buffer);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (CleanBuffer || Element.Pipeline.ClearBuffers) _Buffer.Span.Clean();
        }

        /// <inheritdoc/>
        protected override Task DisposeCore()
        {
            if (CleanBuffer || Element.Pipeline.ClearBuffers) _Buffer.Span.Clean();
            return Task.CompletedTask;
        }
    }
}

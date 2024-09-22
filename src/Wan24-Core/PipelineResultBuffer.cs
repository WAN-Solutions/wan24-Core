﻿namespace wan24.Core
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
    public class PipelineResultBuffer(in PipelineElementBase element, in Memory<byte> buffer, in PipelineElementBase? next = null)
        : PipelineResultBase(element, next), IPipelineResultBuffer
    {
        /// <inheritdoc/>
        public Memory<byte> Buffer { get; } = buffer;

        /// <summary>
        /// Clean the <see cref="Buffer"/> when disposing?
        /// </summary>
        public bool CleanBuffer { get; init; } = true;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (CleanBuffer || Element.Pipeline.ClearBuffers) Buffer.Span.Clean();
        }

        /// <inheritdoc/>
        protected override Task DisposeCore()
        {
            if (CleanBuffer || Element.Pipeline.ClearBuffers) Buffer.Span.Clean();
            return Task.CompletedTask;
        }
    }
}

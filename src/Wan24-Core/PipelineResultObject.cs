using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Pipeline stream element result which hosts an object
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="element">Producing element</param>
    /// <param name="value">Object value (will be disposed, if possible and </param>
    /// <param name="next">Next element</param>
    public class PipelineResultObject<T>(in PipelineElementBase element, [NotNull] in T value, in PipelineElementBase? next = null)
        : PipelineResultBase(element, next), IPipelineResultObject, IPipelineResultObject<T>
    {
        /// <summary>
        /// If to dispose the <see cref="Object"/> when disposing
        /// </summary>
        public bool DisposeObject { get; set; } = true;

        /// <inheritdoc/>
        [NotNull]
        public T Object { get; } = value ?? throw new ArgumentNullException(nameof(value));

        /// <inheritdoc/>
        object IPipelineResultObject.ObjectValue => Object;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (DisposeObject) Object.TryDispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (DisposeObject) await Object.TryDisposeAsync().DynamicContext();
        }
    }
}

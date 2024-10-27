namespace wan24.Core
{
    /// <summary>
    /// Pipeline result
    /// </summary>
    public record class PipelineResult()
    {
        /// <summary>
        /// Pipeline processing runtime
        /// </summary>
        public required TimeSpan Runtime { get; init; }

        /// <summary>
        /// If the pipeline did finish processing all methods
        /// </summary>
        public bool DidFinish { get; init; }

        /// <summary>
        /// The method which did cancel the pipeline (<see langword="null"/>, if <see cref="DidFinish"/> is <see langword="true"/>)
        /// </summary>
        public Delegate? LastMethod { get; init; }

        /// <summary>
        /// The last method index (<c>-1</c>, if none)
        /// </summary>
        public int LastMethodIndex { get; init; } = -1;

        /// <summary>
        /// The value of <see cref="PipelineContextBase{tPipeline, tFinal}.Result"/>
        /// </summary>
        public object? Result { get; init; }

        /// <summary>
        /// Handled exception, if any
        /// </summary>
        public Exception? Exception { get; init; }

        /// <summary>
        /// Cast as <see cref="DidFinish"/>
        /// </summary>
        /// <param name="result">Result</param>
        public static implicit operator bool(in PipelineResult result) => result.DidFinish;

        /// <summary>
        /// Cast as <see cref="Runtime"/>
        /// </summary>
        /// <param name="result">Result</param>
        public static implicit operator TimeSpan(in PipelineResult result) => result.Runtime;
    }
}

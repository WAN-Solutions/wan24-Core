namespace wan24.Core
{
    /// <summary>
    /// Pipeline context
    /// </summary>
    /// <typeparam name="T">Pipeline type</typeparam>
    public sealed class PipelineContext<T>() : PipelineContextBase<T, PipelineContext<T>>() where T : PipelineBase<PipelineContext<T>, T> { }

    /// <summary>
    /// Base class for a pipeline context
    /// </summary>
    /// <typeparam name="tFinal">Final type</typeparam>
    /// <typeparam name="tPipeline">Pipeline type</typeparam>
    public abstract class PipelineContextBase<tPipeline, tFinal>() : SimpleDisposableBase()
        where tPipeline : PipelineBase<tFinal, tPipeline>
        where tFinal : PipelineContextBase<tPipeline, tFinal>
    {
        /// <summary>
        /// Pipeline
        /// </summary>
        public required tPipeline Pipeline { get; init; }

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime Created { get; } = DateTime.Now;

        /// <summary>
        /// Time of the current method call
        /// </summary>
        public DateTime MethodCall { get; set; }

        /// <summary>
        /// Finished time
        /// </summary>
        public DateTime Finished { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Pipeline processing runtime
        /// </summary>
        public TimeSpan Runtime => Finished == DateTime.MinValue 
            ? DateTime.Now - Created 
            : Finished - Created;

        /// <summary>
        /// Complete processing method stack
        /// </summary>
        public List<PipelineBase<tFinal, tPipeline>.Pipeline_Delegate> MethodStack { get; } = [];

        /// <summary>
        /// Current method
        /// </summary>
        public PipelineBase<tFinal, tPipeline>.Pipeline_Delegate CurrentMethod => MethodStack[^1];

        /// <summary>
        /// Current method index
        /// </summary>
        public int CurrentMethodIndex { get; set; }

        /// <summary>
        /// Next method (set to override)
        /// </summary>
        public PipelineBase<tFinal, tPipeline>.Pipeline_Delegate? NextMethod { get; set; }

        /// <summary>
        /// Any tagged data (values will be disposed, if <see cref="DisposeData"/> is <see langword="true"/>, which is the default)
        /// </summary>
        public Dictionary<string, object?> Data { get; } = [];

        /// <summary>
        /// If <see cref="Data"/> values should be disposed, when disposing the context
        /// </summary>
        public bool DisposeData { get; init; } = true;

        /// <summary>
        /// Cancellation token
        /// </summary>
        public CancellationToken CancelToken { get; set; }

        /// <summary>
        /// Result of the pipeline processing (wopn't be disposed)
        /// </summary>
        public object? Result { get; set; }

        /// <summary>
        /// Exception catched by the pipeline processor
        /// </summary>
        public Exception? LastException { get; set; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Finished = DateTime.Now;
            if (DisposeData) Data.Values.WhereNotNull().TryDisposeAll();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            Finished = DateTime.Now;
            if (DisposeData) await Data.Values.WhereNotNull().TryDisposeAllAsync().DynamicContext();
        }

        /// <summary>
        /// Cast as <see cref="Runtime"/>
        /// </summary>
        /// <param name="context">Context</param>
        public static implicit operator TimeSpan(in PipelineContextBase<tPipeline, tFinal> context) => context.Runtime;

        /// <summary>
        /// Cast as <see cref="CancelToken"/>
        /// </summary>
        /// <param name="context">Context</param>
        public static implicit operator CancellationToken(in PipelineContextBase<tPipeline, tFinal> context) => context.CancelToken;
    }
}

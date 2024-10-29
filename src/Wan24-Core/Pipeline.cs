using System.Collections.Immutable;
using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Pipeline
    /// </summary>
    /// <typeparam name="T">Context type</typeparam>
    public sealed class Pipeline<T>() : PipelineBase<T, Pipeline<T>>() where T : PipelineContextBase<Pipeline<T>, T> { }

    /// <summary>
    /// Base class for a pipeline
    /// </summary>
    /// <typeparam name="tContext">Context type</typeparam>
    /// <typeparam name="tFinal">Final type</typeparam>
    public abstract class PipelineBase<tContext, tFinal> : HostedServiceBase, IPipeline
        where tContext : PipelineContextBase<tFinal, tContext>
        where tFinal : PipelineBase<tContext, tFinal>
    {
        /// <summary>
        /// Pipeline methods
        /// </summary>
        protected readonly FreezableList<Pipeline_Delegate> Methods = [];
        /// <summary>
        /// Active contexts
        /// </summary>
        protected readonly ConcurrentHashSet<tContext> Contexts = [];

        /// <summary>
        /// Constructor
        /// </summary>
        protected PipelineBase() : base()
        {
            CanPause = true;
            PipelineTable.Pipelines[GUID] = this;
        }

        /// <inheritdoc/>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public int ContextCount => Contexts.Count;

        /// <inheritdoc/>
        public virtual IEnumerable<Status> State
        {
            get
            {
                yield return new(__("GUID"), GUID, __("Unique ID of the service object"));
                yield return new(__("Name"), Name ?? ToString(), __("Name of this service"));
                yield return new(__("Type"), GetType(), __("CLR type"));
                yield return new(__("Last exception"), LastException?.Message ?? LastException?.GetType().ToString(), __("Last exception message"));
                yield return new(__("Contexts"), Contexts.Count, __("Number of processing contexts"));
            }
        }

        /// <summary>
        /// Add a method (only possible as long as <see cref="Freeze"/> wasn't called)
        /// </summary>
        /// <param name="method">Method</param>
        /// <returns>This</returns>
        public virtual tFinal Add(in Pipeline_Delegate method)
        {
            EnsureUndisposed();
            Methods.Add(method);
            return (tFinal)this;
        }

        /// <summary>
        /// Freeze the configuration (after this <see cref="Add(in Pipeline_Delegate)"/> isn't callable anymore)
        /// </summary>
        /// <returns>This</returns>
        public virtual tFinal Freeze()
        {
            EnsureUndisposed();
            if (!Methods.IsFrozen) Methods.Freeze();
            return (tFinal)this;
        }

        /// <summary>
        /// Process a new context (<see cref="Freeze"/> must be called once before calling this method)
        /// </summary>
        /// <param name="context">Context (will be disposed)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public virtual async Task<PipelineResult> ProcessAsync(tContext context, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ObjectDisposedException.ThrowIf(context.IsDisposing, context);
            if (context.CancelToken.IsEqualTo(default))
            {
                context.CancelToken = cancellationToken;
            }
            else if (cancellationToken.IsEqualTo(default))
            {
                cancellationToken = context.CancelToken;
            }
            await RunEvent.WaitAsync(cancellationToken).DynamicContext();
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(
                [.. CancellationTokenExtensions.RemoveDoubles([cancellationToken, context.CancelToken, CancelToken])]
                );
            cancellationToken = cts.Token;
            context.CancelToken = cancellationToken;
            cancellationToken.ThrowIfCancellationRequested();
            await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
            EnsureUndisposed();
            if (!Methods.IsFrozen) throw new InvalidOperationException("The pipeline configuration wasn't frozen yet");
            Pipeline_Delegate? method = null;
            if (context.MethodStack.Count > 0) context.MethodStack.Clear();
            ImmutableArray<Pipeline_Delegate> methods = Methods.Frozen ?? throw new InvalidProgramException();
            int i = 0;
            if (!Contexts.Add(context)) throw new ArgumentException("Double context", nameof(context));
            try
            {
                for (int len = methods.Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                {
                    if (i > 0)
                    {
                        EnsureUndisposed();
                        await RunEvent.WaitAsync(cancellationToken).DynamicContext();
                        await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
                        EnsureUndisposed();
                    }
                    method = methods[i];
                    if (context.NextMethod is Pipeline_Delegate nextMethod && nextMethod != method) continue;
                    context.MethodStack.Add(method);
                    context.CurrentMethodIndex = i;
                    context.NextMethod = i == len - 1
                        ? null
                        : methods[i + 1];
                    context.MethodCall = DateTime.Now;
                    if (!await RunMethodAsync(context).DynamicContext())
                        break;
                    method = null;
                }
                return new()
                {
                    Runtime = context.Runtime,
                    DidFinish = method is null,
                    LastMethod = method,
                    LastMethodIndex = method is null
                        ? -1
                        : i,
                    Result = context.Result
                };
            }
            catch (Exception ex)
            {
                context.LastException = ex;
                if (!await HandleExceptionAsync(context, method).DynamicContext())
                    throw;
                return new()
                {
                    Runtime = context.Runtime,
                    DidFinish = false,
                    LastMethod = method,
                    LastMethodIndex = method is null 
                        ? -1 
                        : i,
                    Result = context.Result,
                    Exception = ex
                };
            }
            finally
            {
                Contexts.Remove(context);
                await context.TryDisposeAsync().DynamicContext();
                await FinalizeProcessingAsync(context).DynamicContext();
            }
        }

        /// <summary>
        /// Run a method
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>If to continue with the next method</returns>
        protected virtual async Task<bool> RunMethodAsync(tContext context) => await context.CurrentMethod(context).DynamicContext();

        /// <summary>
        /// Handle an exception (which was set to <see cref="PipelineContextBase{tPipeline, tFinal}.LastException"/>)
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="method">Executing method</param>
        /// <returns>If the exception was handled (won't be thrown, if <see langword="true"/>)</returns>
        protected virtual Task<bool> HandleExceptionAsync(tContext context, Pipeline_Delegate? method)
            => Task.FromResult(false);

        /// <summary>
        /// Finalize processing (called from a finally block, after the context was disposed)
        /// </summary>
        /// <param name="context">Context (disposed already!)</param>
        protected virtual Task FinalizeProcessingAsync(tContext context) => Task.CompletedTask;

        /// <inheritdoc/>
        protected override async Task WorkerAsync() => await CancelToken.WaitHandle.WaitAsync().DynamicContext();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            PipelineTable.Pipelines.TryRemove(GUID, out _);
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            PipelineTable.Pipelines.TryRemove(GUID, out _);
            await base.DisposeCore().DynamicContext();
        }

        /// <summary>
        /// Delegate for a pipeline method
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>If to continue with the next method</returns>
        public delegate Task<bool> Pipeline_Delegate(tContext context);

        /// <summary>
        /// Cast as <see cref="ContextCount"/>
        /// </summary>
        /// <param name="pipeline">Pipeline</param>
        public static implicit operator int(in PipelineBase<tContext, tFinal> pipeline) => pipeline.Contexts.Count;
    }
}

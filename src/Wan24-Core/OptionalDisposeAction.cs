namespace wan24.Core
{
    /// <summary>
    /// Optional dispose action
    /// </summary>
    public sealed class OptionalDisposeAction : DisposableBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="action">Action to execute when disposing</param>
        /// <param name="execute">Execute the dispose action?</param>
        public OptionalDisposeAction(in Action action, in bool execute = true) : base(asyncDisposing: false)
        {
            DisposeAction = action;
            ExecuteDisposeAction = execute;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="action">Action to execute when disposing</param>
        /// <param name="execute">Execute the dispose action?</param>
        public OptionalDisposeAction(in Func<Task> action, in bool execute = true) : base()
        {
            AsyncDisposeAction = action;
            ExecuteDisposeAction = execute;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="action">Action to execute when disposing synchronous</param>
        /// <param name="asyncAction">Asynchronous action to execute when disposing asynchronous</param>
        /// <param name="execute">Execute the dispose actions?</param>
        public OptionalDisposeAction(in Action action, in Func<Task> asyncAction, in bool execute = true) : base()
        {
            DisposeAction = action;
            AsyncDisposeAction = asyncAction;
            ExecuteDisposeAction = execute;
        }

        /// <summary>
        /// Action to execute when disposing
        /// </summary>
        public Action? DisposeAction { get; }

        /// <summary>
        /// Action to execute when disposing
        /// </summary>
        public Func<Task>? AsyncDisposeAction { get; }

        /// <summary>
        /// Execute the dispose action(s)?
        /// </summary>
        public bool ExecuteDisposeAction { get; set; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!ExecuteDisposeAction) return;
            if (DisposeAction is not null)
            {
                DisposeAction();
            }
            else
            {
                AsyncDisposeAction!().GetAwaiter().GetResult();
            }
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (!ExecuteDisposeAction) return;
            if (AsyncDisposeAction is not null)
            {
                await AsyncDisposeAction!().DynamicContext();
            }
            else
            {
                DisposeAction!();
            }
        }
    }
}

namespace wan24.Core
{
    // Running action result
    public sealed partial class ParallelTransaction
    {
        /// <summary>
        /// Running action result
        /// </summary>
        public sealed record class RunningActionResult
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="index">Action index</param>
            public RunningActionResult(in int index) => Index = index;

            /// <summary>
            /// Action index
            /// </summary>
            public int Index { get; }

            /// <summary>
            /// Action task
            /// </summary>
            public Task<object?> Task { get; internal set; } = null!;

            /// <summary>
            /// Exception
            /// </summary>
            public Exception? Exception { get; internal set; }

            /// <summary>
            /// Return value
            /// </summary>
            public object? ReturnValue { get; internal set; }
        }
    }
}

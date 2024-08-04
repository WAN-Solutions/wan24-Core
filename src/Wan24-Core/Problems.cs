namespace wan24.Core
{
    /// <summary>
    /// Collected problems
    /// </summary>
    public static class Problems
    {
        /// <summary>
        /// An object for static thread synchronization for accessing <see cref="Collected"/>
        /// </summary>
        public static readonly object SyncObject = new();
        /// <summary>
        /// Collected problems
        /// </summary>
        public static readonly HashSet<IProblemInfo> Collected = [];

        /// <summary>
        /// Number of collected problems
        /// </summary>
        public static int Count
        {
            get
            {
                lock (SyncObject) return Collected.Count;
            }
        }

        /// <summary>
        /// Add a problem to the <see cref="Collected"/>
        /// </summary>
        /// <param name="problem">Problem</param>
        /// <returns>If added</returns>
        public static bool Add(IProblemInfo problem)
        {
            IProblemInfo? prob = RaiseOnProblem(problem);
            if (prob is not null) lock (SyncObject) return Collected.Add(prob);
            return false;
        }

        /// <summary>
        /// Get all <see cref="Collected"/>
        /// </summary>
        /// <param name="clear">If to clear <see cref="Collected"/></param>
        /// <returns>All collected problem informations</returns>
        public static IProblemInfo[] Get(in bool clear = false)
        {
            lock (SyncObject)
            {
                IProblemInfo[] res = [.. Collected];
                if (clear) Collected.Clear();
                return res;
            }
        }

        /// <summary>
        /// Clear <see cref="Collected"/>
        /// </summary>
        public static void Clear()
        {
            lock (SyncObject) Collected.Clear();
        }

        /// <summary>
        /// Delegate for a problem handler
        /// </summary>
        /// <param name="e">Arguments</param>
        public delegate void Problem_Delegate(ProblemEventArgs e);
        /// <summary>
        /// Raised when <see cref="Add(IProblemInfo)"/> was called
        /// </summary>
        public static event Problem_Delegate? OnProblem;
        /// <summary>
        /// Raise the <see cref="OnProblem"/> event
        /// </summary>
        /// <param name="problem">Problem informations</param>
        /// <returns>Problem informations to use</returns>
        private static IProblemInfo? RaiseOnProblem(in IProblemInfo problem)
        {
            ProblemEventArgs e = new(problem);
            OnProblem?.Invoke(e);
            return e.Problem;
        }

        /// <summary>
        /// Arguments for the <see cref="OnProblem"/> event
        /// </summary>
        /// <param name="problem">Problem informations</param>
        public sealed class ProblemEventArgs(in IProblemInfo problem) : EventArgs()
        {
            /// <summary>
            /// Problem informations (set to <see langword="null"/> for not collecting the problem informations)
            /// </summary>
            public IProblemInfo? Problem { get; set; } = problem;
        }
    }
}

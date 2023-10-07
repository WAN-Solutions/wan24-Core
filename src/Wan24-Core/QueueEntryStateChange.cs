using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// State change
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public readonly record struct QueueEntryStateChange
    {
        /// <summary>
        /// Time
        /// </summary>
        public readonly DateTime Time = DateTime.Now;
        /// <summary>
        /// State
        /// </summary>
        public readonly QueueEntryStates State;
        /// <summary>
        /// Exception
        /// </summary>
        public readonly Exception? Exception;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state">State</param>
        /// <param name="ex">Exception</param>
        public QueueEntryStateChange(QueueEntryStates state, Exception? ex = null)
        {
            State = state;
            Exception = ex;
        }
    }
}

namespace wan24.Core
{
    /// <summary>
    /// Queue entry states
    /// </summary>
    public enum QueueEntryStates : byte
    {
        /// <summary>
        /// Enqueued
        /// </summary>
        Enqueued = 0,
        /// <summary>
        /// Processing
        /// </summary>
        Processing = 1,
        /// <summary>
        /// Processing error
        /// </summary>
        Error = 2,
        /// <summary>
        /// Processing paused
        /// </summary>
        Paused = 3,
        /// <summary>
        /// Processing done
        /// </summary>
        Done = 4
    }
}

using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace wan24.Core
{
    /// <summary>
    /// Interface for a queue entry
    /// </summary>
    public interface IQueueEntry : IStatusProvider
    {
        /// <summary>
        /// GUID
        /// </summary>
        string GUID { get; }
        /// <summary>
        /// Name
        /// </summary>
        string? Name { get; set; }
        /// <summary>
        /// Created time
        /// </summary>
        DateTime Created { get; }
        /// <summary>
        /// Item
        /// </summary>
        object Item { get; }
        /// <summary>
        /// Last processing time
        /// </summary>
        DateTime LastProcessed { get; }
        /// <summary>
        /// Last exception
        /// </summary>
        Exception? LastException { get; set; }
        /// <summary>
        /// Queue state
        /// </summary>
        QueueEntryStates QueueState { get; set; }
        /// <summary>
        /// Queue state changes
        /// </summary>
        ImmutableArray<QueueEntryStateChange> Changes { get; }
        /// <summary>
        /// Processing done time
        /// </summary>
        DateTime Done { get; }
        /// <summary>
        /// Last processing time
        /// </summary>
        TimeSpan LastProcessingTime { get; }
        /// <summary>
        /// Total processing time (until done)
        /// </summary>
        TimeSpan TotalProcessingTime { get; }
        /// <summary>
        /// Waiting processing time
        /// </summary>
        TimeSpan WaitingProcessingTime { get; }
    }
}

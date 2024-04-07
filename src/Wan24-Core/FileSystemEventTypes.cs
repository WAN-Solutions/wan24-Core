namespace wan24.Core
{
    /// <summary>
    /// File system event type flags
    /// </summary>
    [Flags]
    public enum FileSystemEventTypes : byte
    {
        /// <summary>
        /// None
        /// </summary>
        [DisplayText("No events")]
        None = 0,
        /// <summary>
        /// Created
        /// </summary>
        [DisplayText("Entry created")]
        Created = 1,
        /// <summary>
        /// Changes
        /// </summary>
        [DisplayText("Entry changed")]
        Changes = 2,
        /// <summary>
        /// Renamed
        /// </summary>
        [DisplayText("Entry renamed")]
        Renamed = 4,
        /// <summary>
        /// Deleted
        /// </summary>
        [DisplayText("Entry deleted")]
        Deleted = 8,
        /// <summary>
        /// All flags
        /// </summary>
        [DisplayText("All events")]
        All = Created | Changes | Renamed | Deleted
    }
}

namespace wan24.Core
{
    /// <summary>
    /// Processing information (don't forget to dispose!)
    /// </summary>
    public class ProcessingInfo : DisposableBase, IProcessingInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="tag">Any tagged object</param>
        public ProcessingInfo(in string description, in object? tag = null) : this(description, tag, asyncDisposing: false) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="tag">Any tagged object</param>
        /// <param name="asyncDisposing">Asynchronous disposing?</param>
        protected ProcessingInfo(in string description, in object? tag, in bool asyncDisposing) : base(asyncDisposing)
        {
            Description = description;
            ProcessTable.Processing[GUID] = this;
            Tag = tag;
        }

        /// <inheritdoc/>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public DateTime Started { get; } = DateTime.Now;

        /// <inheritdoc/>
        public virtual string Description { get; }

        /// <inheritdoc/>
        public virtual object? Tag { get; }

        /// <summary>
        /// Remove this instance from the process table (called during disposing)
        /// </summary>
        protected void RemoveFromProcessTable() => ProcessTable.Processing.TryRemove(GUID, out _);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => RemoveFromProcessTable();
    }
}

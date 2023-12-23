namespace wan24.Core
{
    // Properties
    public partial class AcidStream<T> where T : Stream
    {
        /// <summary>
        /// IO synchronization
        /// </summary>
        public SemaphoreSync SyncIO { get; } = new();

        /// <summary>
        /// Backup stream (will be disposed!)
        /// </summary>
        public Stream Backup { get; }

        /// <summary>
        /// Needs a commit?
        /// </summary>
        public bool NeedsCommit { get; protected set; }

        /// <summary>
        /// Automatic commit each writing operation?
        /// </summary>
        public bool AutoCommit { get; set; }

        /// <summary>
        /// Automatic rollback on error?
        /// </summary>
        public bool AutoRollback { get; set; } = true;

        /// <summary>
        /// Automatic flush after each write operation?
        /// </summary>
        public bool AutoFlush { get; set; }

        /// <summary>
        /// Automatic flush the backup stream after each write operation?
        /// </summary>
        public bool AutoFlushBackup { get; set; }

        /// <summary>
        /// Leave the base stream open when disposing (returns <see langword="false"/> always, setter will throw!)
        /// </summary>
        /// <exception cref="NotSupportedException">Setter isn't supported</exception>
        public override bool LeaveOpen { get => base.LeaveOpen; set => throw new NotSupportedException(); }

        /// <inheritdoc/>
        public override long Position
        {
            get => BaseStream.Position;
            set
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = SyncIO;
                BaseStream.Position = value;
            }
        }
    }
}

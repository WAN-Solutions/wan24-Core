namespace wan24.Core
{
    // ACID functionality
    public partial class AcidStream<T> where T : Stream
    {
        /// <inheritdoc/>
        public virtual void Commit()
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO;
            CommitInt();
        }

        /// <inheritdoc/>
        public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            await CommitIntAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public virtual void Rollback()
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO;
            RollbackInt();
        }

        /// <inheritdoc/>
        public virtual async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            await RollbackIntAsync(cancellationToken).DynamicContext();
        }
    }
}

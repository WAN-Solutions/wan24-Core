namespace wan24.Core
{
    // ACID functionality
    public partial class AcidStream<T> where T : Stream
    {
        /// <summary>
        /// Commit the changes since the last commit
        /// </summary>
        public virtual void Commit()
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO;
            CommitInt();
        }

        /// <summary>
        /// Commit the changes since the last commit
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            await CommitIntAsync(cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Perform a rollback
        /// </summary>
        public virtual void Rollback()
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO;
            RollbackInt();
        }

        /// <summary>
        /// Perform a rollback
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            await RollbackIntAsync(cancellationToken).DynamicContext();
        }
    }
}

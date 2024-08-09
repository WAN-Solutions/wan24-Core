namespace wan24.Core
{
    /// <summary>
    /// Interface for a transaction
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// Perform a rollback
        /// </summary>
        void Rollback();
        /// <summary>
        /// Perform a rollback
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RollbackAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Commit changes
        /// </summary>
        void Commit();
        /// <summary>
        /// Commit changes
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}

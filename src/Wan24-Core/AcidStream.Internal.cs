namespace wan24.Core
{
    // Internals
    public partial class AcidStream<T> where T : Stream
    {
        /// <summary>
        /// Serialization buffer
        /// </summary>
        protected readonly RentedArray<byte> SerializationBuffer;

        /// <summary>
        /// Commit the changes since the last commit
        /// </summary>
        protected virtual void CommitInt()
        {
            Logging.WriteTrace($"{this} commit changes");
            try
            {
                Backup.SetLength(0);
                InitializeBackupStream(BaseStream, Backup, SerializationBuffer, AutoFlushBackup);
            }
            catch (Exception ex)
            {
                if (AutoRollback)
                    try
                    {
                        RollbackInt();
                    }
                    catch (Exception ex2)
                    {
                        throw new AcidException("Rollback during failed commit operation failed, too", new AggregateException(ex, ex2));
                    }
                throw;
            }
            NeedsCommit = false;
        }

        /// <summary>
        /// Commit the changes since the last commit
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task CommitIntAsync(CancellationToken cancellationToken)
        {
            Logging.WriteTrace($"{this} commit changes");
            try
            {
                Backup.SetLength(0);
                await InitializeBackupStreamAsync(BaseStream, Backup, SerializationBuffer, AutoFlushBackup, cancellationToken).DynamicContext();
            }
            catch (Exception ex)
            {
                if (AutoRollback)
                    try
                    {
                        await RollbackIntAsync(cancellationToken).DynamicContext();
                    }
                    catch (Exception ex2)
                    {
                        throw new AcidException("Rollback during failed write IO operation failed, too", new AggregateException(ex, ex2));
                    }
                throw;
            }
            NeedsCommit = false;
        }

        /// <summary>
        /// Perform a rollback
        /// </summary>
        protected virtual void RollbackInt()
        {
            Logging.WriteTrace($"{this} rollback changes");
            PerformRollback(this, sync: false);
        }

        /// <summary>
        /// Perform a rollback
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task RollbackIntAsync(CancellationToken cancellationToken)
        {
            Logging.WriteTrace($"{this} rollback changes");
            await PerformRollbackAsync(this, sync: false, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (NeedsCommit && AutoRollback)
            {
                ErrorHandling.Handle(new InvalidOperationException($"{GetType()} wasn't committed - will be rolled back for disposing"));
                try
                {
                    RollbackInt();
                }
                catch (Exception ex)
                {
                    ErrorHandling.Handle(ex);
                }
            }
            Backup.Dispose();
            SerializationBuffer.Dispose();
            SyncIO.Dispose();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (NeedsCommit && AutoRollback)
            {
                ErrorHandling.Handle(new InvalidOperationException($"{GetType()} wasn't committed - will be rolled back for disposing"));
                try
                {
                    await RollbackIntAsync(CancellationToken.None).DynamicContext();
                }
                catch (Exception ex)
                {
                    ErrorHandling.Handle(ex);
                }
            }
            await Backup.DisposeAsync().DynamicContext();
            await SerializationBuffer.DisposeAsync().DynamicContext();
            await SyncIO.DisposeAsync().DynamicContext();
            await base.DisposeCore().DynamicContext();
        }
    }
}

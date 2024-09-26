using static wan24.Core.Logging;

namespace wan24.Core
{
    // Internals
    public partial class AcidStream<T> where T : Stream
    {
        /// <summary>
        /// Serialization buffer
        /// </summary>
        protected readonly RentedMemory<byte> SerializationBuffer;

        /// <summary>
        /// Write a write backup record
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Required a commit before?</returns>
        protected bool WriteWriteBackupRecord(in ReadOnlySpan<byte> buffer)
        {
            int len = (int)Math.Min(BaseStream.Length - BaseStream.Position, buffer.Length);
            if (len > 0)
            {
                if (Trace) Logging.WriteTrace($"{this} writing {len} byte overwritten data backup at offset {Backup.Position}");
                long pos = BaseStream.Position;
                Span<byte> bufferSpan = SerializationBuffer.Memory.Span;
                Backup.WriteByte((byte)IoTypes.Write);
                Backup.Write(DateTime.UtcNow.Ticks.GetBytes(bufferSpan));
                Backup.Write(pos.GetBytes(bufferSpan));
                Backup.Write(len.GetBytes(bufferSpan[..sizeof(int)]));
                BaseStream.CopyExactlyPartialTo(Backup, len);
                Backup.Write(len.GetBytes(bufferSpan[..sizeof(int)]));
                Backup.Write(pos.GetBytes(bufferSpan));
                Backup.WriteByte((byte)IoTypes.Write);
                if (Trace) Logging.WriteTrace($"{this} overwritten data backup done at offset {Backup.Position}");
                if (AutoFlushBackup) Backup.Flush();
                BaseStream.Position = pos;
            }
            bool commitRequired = NeedsCommit;
            NeedsCommit = true;
            return commitRequired;
        }

        /// <summary>
        /// Write a write backup record
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Required a commit before?</returns>
        protected async Task<bool> WriteWriteBackupRecordAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            int len = (int)Math.Min(BaseStream.Length - BaseStream.Position, buffer.Length);
            if (len > 0)
            {
                if (Trace) Logging.WriteTrace($"{this} writing {len} byte overwritten data backup at offset {Backup.Position}");
                long pos = BaseStream.Position;
                Memory<byte> bufferMem = SerializationBuffer.Memory;
                Backup.WriteByte((byte)IoTypes.Write);
                await Backup.WriteAsync(DateTime.UtcNow.Ticks.GetBytes(bufferMem), cancellationToken).DynamicContext();
                await Backup.WriteAsync(pos.GetBytes(bufferMem), cancellationToken).DynamicContext();
                await Backup.WriteAsync(len.GetBytes(bufferMem[..sizeof(int)]), cancellationToken).DynamicContext();
                await BaseStream.CopyExactlyPartialToAsync(Backup, len, cancellationToken: cancellationToken).DynamicContext();
                await Backup.WriteAsync(len.GetBytes(bufferMem[..sizeof(int)]), cancellationToken).DynamicContext();
                await Backup.WriteAsync(pos.GetBytes(bufferMem), cancellationToken).DynamicContext();
                Backup.WriteByte((byte)IoTypes.Write);
                if (Trace) Logging.WriteTrace($"{this} overwritten data backup done at offset {Backup.Position}");
                if (AutoFlushBackup) await Backup.FlushAsync(cancellationToken).DynamicContext();
                BaseStream.Position = pos;
            }
            bool commitRequired = NeedsCommit;
            NeedsCommit = true;
            return commitRequired;
        }

        /// <summary>
        /// Write a length backup record
        /// </summary>
        /// <param name="len">Current length in byte</param>
        /// <param name="value">New length in byte</param>
        /// <returns>Required a commit before?</returns>
        protected bool WriteLengthBackupRecord(in long len, in long value)
        {
            if (Trace) Logging.WriteTrace($"{this} applying new length at offset {Backup.Position}");
            Span<byte> bufferSpan = SerializationBuffer.Memory.Span;
            Backup.WriteByte((byte)IoTypes.Length);
            Backup.Write(DateTime.UtcNow.Ticks.GetBytes(bufferSpan));
            Backup.Write(len.GetBytes(bufferSpan));
            Backup.Write(value.GetBytes(bufferSpan));
            long dataLen = Math.Max(0, len - value);
            if (dataLen != 0)
            {
                if (Trace) Logging.WriteTrace($"{this} writing {len} byte cutted off data backup (setting new length to {value} byte)");
                long pos = BaseStream.Position;
                BaseStream.Position = value;
                BaseStream.CopyPartialTo(Backup, dataLen);
                BaseStream.Position = pos;
            }
            else
            {
                if (Trace) Logging.WriteTrace($"{this} setting new length to {value} byte");
            }
            Backup.Write(value.GetBytes(bufferSpan));
            Backup.Write(len.GetBytes(bufferSpan));
            Backup.WriteByte((byte)IoTypes.Length);
            if (Trace) Logging.WriteTrace($"{this} applying new length done at offset {Backup.Position}");
            if (AutoFlushBackup) Backup.Flush();
            bool commitRequired = NeedsCommit;
            NeedsCommit = true;
            return commitRequired;
        }

        /// <summary>
        /// Write a length backup record
        /// </summary>
        /// <param name="len">Current length in byte</param>
        /// <param name="value">New length in byte</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Required a commit before?</returns>
        protected async Task<bool> WriteLengthBackupRecordAsync(long len, long value, CancellationToken cancellationToken)
        {
            if (Trace) Logging.WriteTrace($"{this} applying new length at offset {Backup.Position}");
            Memory<byte> bufferMem = SerializationBuffer.Memory;
            Backup.WriteByte((byte)IoTypes.Length);
            await Backup.WriteAsync(DateTime.UtcNow.Ticks.GetBytes(bufferMem), cancellationToken).DynamicContext();
            await Backup.WriteAsync(len.GetBytes(bufferMem), cancellationToken).DynamicContext();
            await Backup.WriteAsync(value.GetBytes(bufferMem), cancellationToken).DynamicContext();
            long dataLen = Math.Max(0, len - value);
            if (dataLen != 0)
            {
                if (Trace) Logging.WriteTrace($"{this} writing {len} byte cutted off data backup (setting new length to {value} byte)");
                long pos = BaseStream.Position;
                BaseStream.Position = value;
                await BaseStream.CopyPartialToAsync(Backup, dataLen, cancellationToken: cancellationToken).DynamicContext();
                BaseStream.Position = pos;
            }
            else
            {
                if (Trace) Logging.WriteTrace($"{this} setting new length to {value} byte");
            }
            await Backup.WriteAsync(value.GetBytes(bufferMem), cancellationToken).DynamicContext();
            await Backup.WriteAsync(len.GetBytes(bufferMem), cancellationToken).DynamicContext();
            Backup.WriteByte((byte)IoTypes.Length);
            if (Trace) Logging.WriteTrace($"{this} applying new length done at offset {Backup.Position}");
            if (AutoFlushBackup) await Backup.FlushAsync(cancellationToken).DynamicContext();
            bool commitRequired = NeedsCommit;
            NeedsCommit = true;
            return commitRequired;
        }

        /// <summary>
        /// Commit the changes since the last commit
        /// </summary>
        protected virtual void CommitInt()
        {
            if (Trace) Logging.WriteTrace($"{this} commit changes");
            try
            {
                RaiseOnBeforeCommit();
                Backup.SetLength(0);
                InitializeBackupStream(BaseStream, Backup, SerializationBuffer, AutoFlushBackup);
                NeedsCommit = false;
                RaiseOnAfterCommit();
            }
            catch (Exception ex)
            {
                if (AutoRollback)
                {
                    if (Warning) Logging.WriteWarning($"{this} failed to commit - rolling back on exception: {ex.Message}");
                    try
                    {
                        RollbackInt();
                    }
                    catch (Exception ex2)
                    {
                        throw new AcidException("Rollback during failed commit operation failed, too", new AggregateException(ex, ex2));
                    }
                }
                else
                {
                    if (Warning) Logging.WriteWarning($"{this} failed to commit - no automatic rollback on exception: {ex.Message}");
                }
                throw;
            }
        }

        /// <summary>
        /// Commit the changes since the last commit
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task CommitIntAsync(CancellationToken cancellationToken)
        {
            if (Trace) Logging.WriteTrace($"{this} commit changes");
            try
            {
                RaiseOnBeforeCommit();
                Backup.SetLength(0);
                await InitializeBackupStreamAsync(BaseStream, Backup, SerializationBuffer, AutoFlushBackup, cancellationToken).DynamicContext();
                NeedsCommit = false;
                RaiseOnAfterCommit();
            }
            catch (Exception ex)
            {
                if (AutoRollback)
                {
                    if (Warning) Logging.WriteWarning($"{this} failed to commit - rolling back on exception: {ex.Message}");
                    try
                    {
                        await RollbackIntAsync(cancellationToken).DynamicContext();
                    }
                    catch (Exception ex2)
                    {
                        throw new AcidException("Rollback during failed write IO operation failed, too", new AggregateException(ex, ex2));
                    }
                }
                else
                {
                    if (Warning) Logging.WriteWarning($"{this} failed to commit - no automatic rollback on exception: {ex.Message}");
                }
                throw;
            }
        }

        /// <summary>
        /// Perform a rollback
        /// </summary>
        protected virtual void RollbackInt()
        {
            if (Trace) Logging.WriteTrace($"{this} rollback changes");
            RaiseOnBeforeRollback();
            PerformRollback(this, sync: false);
            RaiseOnAfterRollback();
        }

        /// <summary>
        /// Perform a rollback
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task RollbackIntAsync(CancellationToken cancellationToken)
        {
            if (Trace) Logging.WriteTrace($"{this} rollback changes");
            RaiseOnBeforeRollback();
            await PerformRollbackAsync(this, sync: false, cancellationToken).DynamicContext();
            RaiseOnAfterRollback();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (NeedsCommit)
                if (AutoRollback)
                {
                    ErrorHandling.Handle(new($"{GetType()} wasn't committed - will be rolled back for disposing", new InvalidOperationException(), tag: this));
                    try
                    {
                        RollbackInt();
                    }
                    catch (Exception ex)
                    {
                        ErrorHandling.Handle(new($"{GetType()} wasn't committed - rollback for disposing failed", ex, tag: this));
                    }
                }
                else
                {
                    ErrorHandling.Handle(new($"{GetType()} wasn't committed - won't roll back for disposing", new InvalidOperationException(), tag: this));
                }
            using SemaphoreSync sync = SyncIO;
            using SemaphoreSyncContext ssc = SyncIO;
            Backup.Dispose();
            SerializationBuffer.Dispose();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (NeedsCommit)
                if (AutoRollback)
                {
                    ErrorHandling.Handle(new($"{GetType()} wasn't committed - will be rolled back for disposing", new InvalidOperationException(), tag: this));
                    try
                    {
                        await RollbackIntAsync(CancellationToken.None).DynamicContext();
                    }
                    catch (Exception ex)
                    {
                        ErrorHandling.Handle(new($"{GetType()} wasn't committed - rollback for disposing failed", ex, tag: this));
                    }
                }
                else
                {
                    ErrorHandling.Handle(new($"{GetType()} wasn't committed - won't roll back for disposing", new InvalidOperationException(), tag: this));
                }
            using (SyncIO)
            {
                using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync().DynamicContext();
                await Backup.DisposeAsync().DynamicContext();
                SerializationBuffer.Dispose();
                await base.DisposeCore().DynamicContext();
            }
        }
    }
}

using static wan24.Core.Logging;

namespace wan24.Core
{
    /// <summary>
    /// ACID stream (IO is synchronized)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="stream">Target stream (will be disposed!)</param>
    /// <param name="backup">Backup stream (will be disposed!)</param>
    public class AcidStream(in Stream stream, in Stream? backup = null) : AcidStream<Stream>(stream, backup) { }

    /// <summary>
    /// ACID stream (IO is synchronized)
    /// </summary>
    /// <typeparam name="T">Target stream type</typeparam>
    public partial class AcidStream<T> : WrapperStream<T>, ITransaction where T : Stream
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">Target stream (will be disposed!)</param>
        /// <param name="backup">Backup stream (will be disposed!)</param>
        public AcidStream(in T stream, in Stream? backup = null) : base(stream)
        {
            if (!CanRead || !CanWrite || !CanSeek) throw new ArgumentException("Read-, writ- and seekable stream required", nameof(stream));
            if (backup is not null)
            {
                if (!backup.CanRead || !backup.CanWrite || !backup.CanSeek) throw new ArgumentException("Read-, writ- and seekable stream required", nameof(backup));
                if (backup.Length < sizeof(long)) throw new ArgumentException("Backup stream has not been initialized", nameof(backup));
            }
            SerializationBuffer = new(len: sizeof(long), clean: false);
            Backup = backup ?? new PooledTempStream();
            if (backup is null) InitializeBackupStream(BaseStream, Backup, SerializationBuffer);
            UseOriginalByteIO = false;
            UseOriginalBeginRead = true;
            UseOriginalBeginWrite = true;
            UseOriginalCopyTo = true;
            UseBaseStream = false;
        }

        /// <summary>
        /// Set a new position
        /// </summary>
        /// <param name="value">Offset</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task SetPositionAsync(long value, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            BaseStream.Position = value;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO;
            return BaseStream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO;
            return BaseStream.Read(buffer);
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            return await BaseStream.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            return await BaseStream.ReadAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            if (buffer.Length == 0) return;
            // Write rollback information
            using SemaphoreSyncContext ssc = SyncIO;
            bool commitRequired = WriteWriteBackupRecord(buffer);
            // Try to write the buffer
            try
            {
                BaseStream.Write(buffer);
                if (AutoFlush) BaseStream.Flush();
            }
            catch (Exception ex)
            {
                if (AutoRollback)
                    try
                    {
                        if (Warning) Logging.WriteWarning($"{this} rolling back after writing exception: {ex.Message}");
                        RollbackInt();
                    }
                    catch (Exception ex2)
                    {
                        throw new AcidException("Rollback during failed write IO operation failed, too", new AggregateException(ex, ex2));
                    }
                throw;
            }
            // Commit
            if (AutoCommit)
            {
                CommitInt();
            }
            else if (!commitRequired)
            {
                RaiseOnNeedCommit();
            }
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (buffer.Length == 0) return;
            // Write rollback information
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            bool commitRequired = await WriteWriteBackupRecordAsync(buffer, cancellationToken).DynamicContext();
            // Try to write the buffer
            try
            {
                await BaseStream.WriteAsync(buffer, cancellationToken).DynamicContext();
                if (AutoFlush) await BaseStream.FlushAsync(cancellationToken).DynamicContext();
            }
            catch (Exception ex)
            {
                if (AutoRollback)
                    try
                    {
                        if (Warning) Logging.WriteWarning($"{this} rolling back after writing exception: {ex.Message}");
                        await RollbackIntAsync(cancellationToken).DynamicContext();
                    }
                    catch (Exception ex2)
                    {
                        throw new AcidException("Rollback during failed write IO operation failed, too", new AggregateException(ex, ex2));
                    }
                throw;
            }
            // Commit
            if (AutoCommit)
            {
                await CommitIntAsync(cancellationToken).DynamicContext();
            }
            else if (!commitRequired)
            {
                RaiseOnNeedCommit();
            }
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            long len = BaseStream.Length;
            if (len == value) return;
            // Write rollback information
            using SemaphoreSyncContext ssc = SyncIO;
            bool commitRequired = WriteLengthBackupRecord(len, value);
            // Try to set the length
            try
            {
                BaseStream.SetLength(value);
                if (AutoFlush) BaseStream.Flush();
            }
            catch (Exception ex)
            {
                if (AutoRollback)
                    try
                    {
                        if (Warning) Logging.WriteWarning($"{this} rolling back after setting length exception: {ex.Message}");
                        RollbackInt();
                    }
                    catch (Exception ex2)
                    {
                        throw new AcidException("Rollback during failed new length IO operation failed, too", new AggregateException(ex, ex2));
                    }
                throw;
            }
            // Commit
            if (AutoCommit)
            {
                CommitInt();
            }
            else if (!commitRequired)
            {
                RaiseOnNeedCommit();
            }
        }

        /// <summary>
        /// Set a new stream length
        /// </summary>
        /// <param name="value">New length in byte</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task SetLengthAsync(long value, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            long len = BaseStream.Length;
            if (len == value) return;
            // Write rollback information
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            bool commitRequired = await WriteLengthBackupRecordAsync(len, value, cancellationToken).DynamicContext();
            // Try to set the length
            try
            {
                BaseStream.SetLength(value);
                if (AutoFlush) await BaseStream.FlushAsync(cancellationToken).DynamicContext();
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
                        throw new AcidException("Rollback during failed new length IO operation failed, too", new AggregateException(ex, ex2));
                    }
                throw;
            }
            // Commit
            if (AutoCommit)
            {
                await CommitIntAsync(cancellationToken).DynamicContext();
            }
            else if (!commitRequired)
            {
                RaiseOnNeedCommit();
            }
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO;
            return BaseStream.Seek(offset, origin);
        }

        /// <summary>
        /// Seek to a relative byte offset
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="origin">Origin</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New offset</returns>
        public virtual async Task<long> SeekAsync(long offset, SeekOrigin origin, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            return BaseStream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO;
            BaseStream.Flush();
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await SyncIO.SyncContextAsync(cancellationToken).DynamicContext();
            await BaseStream.FlushAsync(cancellationToken).DynamicContext();
        }
    }
}

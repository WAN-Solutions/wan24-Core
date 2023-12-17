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
    public sealed class AcidStream(in Stream stream, in Stream? backup = null) : AcidStream<Stream>(stream, backup)
    {
    }

    /// <summary>
    /// ACID stream (IO is synchronized)
    /// </summary>
    /// <typeparam name="T">Target stream type</typeparam>
    public partial class AcidStream<T> : WrapperStream<T> where T : Stream
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
            int len = (int)Math.Min(BaseStream.Length - BaseStream.Position, buffer.Length);
            if (len > 0)
            {
                Logging.WriteTrace($"{this} writing {len} byte overwritten data backup at offset {Backup.Position}");
                long pos = BaseStream.Position;
                Backup.WriteByte((byte)IoTypes.Write);
                Backup.Write(DateTime.UtcNow.Ticks.GetBytes(SerializationBuffer.Span));
                Backup.Write(pos.GetBytes(SerializationBuffer.Span));
                Backup.Write(len.GetBytes(SerializationBuffer.Span[..sizeof(int)]));
                BaseStream.CopyExactlyPartialTo(Backup, len);
                Backup.Write(len.GetBytes(SerializationBuffer.Span[..sizeof(int)]));
                Backup.Write(pos.GetBytes(SerializationBuffer.Span));
                Backup.WriteByte((byte)IoTypes.Write);
                Logging.WriteTrace($"{this} overwritten data backup done at offset {Backup.Position}");
                if (AutoFlushBackup) Backup.Flush();
                BaseStream.Position = pos;
            }
            NeedsCommit = true;
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
                        RollbackInt();
                    }
                    catch (Exception ex2)
                    {
                        throw new AcidException("Rollback during failed write IO operation failed, too", new AggregateException(ex, ex2));
                    }
                throw;
            }
            // Commit
            if (AutoCommit) CommitInt();
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
            int len = (int)Math.Min(BaseStream.Length - BaseStream.Position, buffer.Length);
            if (len > 0)
            {
                Logging.WriteTrace($"{this} writing {len} byte overwritten data backup at offset {Backup.Position}");
                long pos = BaseStream.Position;
                Backup.WriteByte((byte)IoTypes.Write);
                await Backup.WriteAsync(DateTime.UtcNow.Ticks.GetBytes(SerializationBuffer.Memory), cancellationToken).DynamicContext();
                await Backup.WriteAsync(pos.GetBytes(SerializationBuffer.Memory), cancellationToken).DynamicContext();
                await Backup.WriteAsync(len.GetBytes(SerializationBuffer.Memory[..sizeof(int)]), cancellationToken).DynamicContext();
                await BaseStream.CopyExactlyPartialToAsync(Backup, len, cancellationToken: cancellationToken).DynamicContext();
                await Backup.WriteAsync(len.GetBytes(SerializationBuffer.Memory[..sizeof(int)]), cancellationToken).DynamicContext();
                await Backup.WriteAsync(pos.GetBytes(SerializationBuffer.Memory), cancellationToken).DynamicContext();
                Backup.WriteByte((byte)IoTypes.Write);
                Logging.WriteTrace($"{this} overwritten data backup done at offset {Backup.Position}");
                if (AutoFlushBackup) await Backup.FlushAsync(cancellationToken).DynamicContext();
                BaseStream.Position = pos;
            }
            NeedsCommit = true;
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
                        await RollbackIntAsync(cancellationToken).DynamicContext();
                    }
                    catch (Exception ex2)
                    {
                        throw new AcidException("Rollback during failed write IO operation failed, too", new AggregateException(ex, ex2));
                    }
                throw;
            }
            // Commit
            if (AutoCommit) await CommitIntAsync(cancellationToken).DynamicContext();
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
            Logging.WriteTrace($"{this} applying new length at offset {Backup.Position}");
            Backup.WriteByte((byte)IoTypes.Length);
            Backup.Write(DateTime.UtcNow.Ticks.GetBytes(SerializationBuffer.Span));
            Backup.Write(len.GetBytes(SerializationBuffer.Span));
            Backup.Write(value.GetBytes(SerializationBuffer.Span));
            long dataLen = Math.Max(0, len - value);
            if (dataLen != 0)
            {
                Logging.WriteTrace($"{this} writing {len} byte cutted off data backup (setting new length to {value} byte)");
                long pos = BaseStream.Position;
                BaseStream.Position = value;
                BaseStream.CopyPartialTo(Backup, dataLen);
                BaseStream.Position = pos;
            }
            else
            {
                Logging.WriteTrace($"{this} setting new length to {value} byte");
            }
            Backup.Write(value.GetBytes(SerializationBuffer.Span));
            Backup.Write(len.GetBytes(SerializationBuffer.Span));
            Backup.WriteByte((byte)IoTypes.Length);
            Logging.WriteTrace($"{this} applying new length done at offset {Backup.Position}");
            if (AutoFlushBackup) Backup.Flush();
            NeedsCommit = true;
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
                        RollbackInt();
                    }
                    catch (Exception ex2)
                    {
                        throw new AcidException("Rollback during failed new length IO operation failed, too", new AggregateException(ex, ex2));
                    }
                throw;
            }
            // Commit
            if (AutoCommit) CommitInt();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = SyncIO;
            return BaseStream.Seek(offset, origin);
        }

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

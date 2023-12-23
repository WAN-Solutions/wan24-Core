using static wan24.Core.Logging;

namespace wan24.Core
{
    // Backup stream rollback functionality
    public partial class AcidStream<T> where T : Stream
    {
        /// <summary>
        /// Perform a full rollback
        /// </summary>
        /// <param name="stream">ACID stream</param>
        /// <param name="sync">Synchronized IO?</param>
        public static void PerformRollback(in AcidStream<T> stream, in bool sync = true)
        {
            using SemaphoreSyncContext? ssc = sync ? stream.SyncIO.SyncContext() : null;
            if (Debug) Logging.WriteDebug($"Performing rollback for ACID stream {stream}");
            stream.Backup.Position = 0;
            long len = ReadLengthFromBackup(stream.Backup, stream.SerializationBuffer),
                records = InvestigateBackup(stream.Backup, stream.SerializationBuffer);
            if (Trace) Logging.WriteTrace($"Original ACID target stream length in byte: {len}");
            if (Trace) Logging.WriteTrace($"Number of ACID records: {records}");
            try
            {
                for (; records > 0; records--)
                {
                    if (Trace) Logging.WriteTrace($"ACID stream rolling back record {records}");
                    if (ReadBackupRecordBackward(stream.Backup, stream.SerializationBuffer) is not BackupRecordBase record) throw new IOException($"Failed to read backup record #{records}");
                    PerformRollback(stream, record, sync: false);
                    stream.Backup.Position = record.Offset;
                }
                if (Trace) Logging.WriteTrace($"ACID stream rollback setting original target stream length of {len} byte");
                stream.BaseStream.SetLength(len);
                if (stream.AutoFlush) stream.BaseStream.Flush();
                stream.NeedsCommit = false;
                if (Trace) Logging.WriteTrace("Resetting ACID backup stream");
                stream.Backup.SetLength(0);
                InitializeBackupStream(stream.BaseStream, stream.Backup, stream.SerializationBuffer, stream.AutoFlushBackup);
            }
            catch (Exception ex)
            {
                throw new IOException($"Rollback failed at #{records}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Perform a full rollback
        /// </summary>
        /// <param name="stream">ACID stream</param>
        /// <param name="sync">Synchronized IO?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task PerformRollbackAsync(AcidStream<T> stream, bool sync = true, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext? ssc = sync ? await stream.SyncIO.SyncContextAsync(cancellationToken).DynamicContext() : null;
            if (Debug) Logging.WriteDebug($"Performing rollback for ACID stream {stream}");
            stream.Backup.Position = 0;
            long len = await ReadLengthFromBackupAsync(stream.Backup, stream.SerializationBuffer, cancellationToken).DynamicContext(),
                records = await InvestigateBackupAsync(stream.Backup, stream.SerializationBuffer, cancellationToken).DynamicContext();
            if (Trace) Logging.WriteTrace($"Original ACID target stream length in byte: {len}");
            if (Trace) Logging.WriteTrace($"Number of ACID records: {records}");
            try
            {
                for (; records > 0; records--)
                {
                    if (Trace) Logging.WriteTrace($"ACID stream rolling back record {records}");
                    if (await ReadBackupRecordBackwardAsync(stream.Backup, stream.SerializationBuffer, cancellationToken).DynamicContext() is not BackupRecordBase record)
                        throw new IOException($"Failed to read backup record #{records}");
                    await PerformRollbackAsync(stream, record, sync: false, cancellationToken).DynamicContext();
                    stream.Backup.Position = record.Offset;
                }
                if (Trace) Logging.WriteTrace($"ACID stream rollback setting original target stream length of {len} byte");
                stream.BaseStream.SetLength(len);
                if (stream.AutoFlush) await stream.BaseStream.FlushAsync(cancellationToken).DynamicContext();
                stream.NeedsCommit = false;
                if (Trace) Logging.WriteTrace("Resetting ACID backup stream");
                stream.Backup.SetLength(0);
                await InitializeBackupStreamAsync(stream.BaseStream, stream.Backup, stream.SerializationBuffer, stream.AutoFlushBackup, cancellationToken).DynamicContext();
            }
            catch (Exception ex)
            {
                throw new IOException($"Rollback failed at #{records}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Perform a rollback of a backup record
        /// </summary>
        /// <param name="stream">ACID stream</param>
        /// <param name="record">Backup record</param>
        /// <param name="sync">Synchronized IO?</param>
        public static void PerformRollback(in AcidStream<T> stream, in BackupRecordBase record, in bool sync = true)
        {
            using SemaphoreSyncContext? ssc = sync ? stream.SyncIO.SyncContext() : null;
            if (Debug) Logging.WriteDebug($"ACID stream {stream} rolling back record {record}");
            switch (record)
            {
                case BackupWriteRecord writeRecord:
                    // Restore overwritten data
                    if (Trace) Logging.WriteTrace($"ACID stream restoring {writeRecord.Length} byte overwritten data");
                    if (writeRecord.Position > stream.BaseStream.Length) throw new InvalidDataException($"Invalid backup data offset {writeRecord.Position}");
                    stream.Backup.Position = record.Offset + WRITE_META_LEN + TIME_META_LEN;
                    stream.BaseStream.Position = writeRecord.Position;
                    stream.Backup.CopyExactlyPartialTo(stream.BaseStream, writeRecord.Length);
                    break;
                case BackupLengthRecord lengthRecord:
                    // Restore the old stream length
                    if (Trace) Logging.WriteTrace($"ACID stream restoring {lengthRecord.OldLength} byte length from {lengthRecord.NewLength} byte length");
                    if (lengthRecord.DataLength != 0)
                    {
                        // Old stream length was larger
                        if (Trace) Logging.WriteTrace($"ACID stream restoring {lengthRecord.DataLength} byte cutted off data");
                        stream.Backup.Position = record.Offset + LENGTH_META_LEN + TIME_META_LEN;
                        stream.BaseStream.Position = lengthRecord.NewLength;
                        stream.Backup.CopyExactlyPartialTo(stream.BaseStream, lengthRecord.DataLength);
                    }
                    else
                    {
                        // Old stream length was smaller
                        if (Trace) Logging.WriteTrace($"ACID stream rollback cutting target stream to {lengthRecord.OldLength} byte");
                        stream.BaseStream.SetLength(lengthRecord.OldLength);
                    }
                    break;
                default:
                    throw new ArgumentException($"Unexpected record type {record.GetType()}", nameof(record));
            }
        }

        /// <summary>
        /// Perform a rollback of a backup record
        /// </summary>
        /// <param name="stream">ACID stream</param>
        /// <param name="record">Backup record</param>
        /// <param name="sync">Synchronized IO?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task PerformRollbackAsync(AcidStream<T> stream, BackupRecordBase record, bool sync = true, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext? ssc = sync ? await stream.SyncIO.SyncContextAsync(cancellationToken).DynamicContext() : null;
            if (Debug) Logging.WriteDebug($"ACID stream {stream} rolling back record {record}");
            switch (record)
            {
                case BackupWriteRecord writeRecord:
                    // Restore overwritten data
                    if (Trace) Logging.WriteTrace($"ACID stream restoring {writeRecord.Length} byte overwritten data");
                    if (writeRecord.Position > stream.BaseStream.Length) throw new InvalidDataException($"Invalid backup data offset {writeRecord.Position}");
                    stream.Backup.Position = record.Offset + WRITE_META_LEN + TIME_META_LEN;
                    stream.BaseStream.Position = writeRecord.Position;
                    await stream.Backup.CopyExactlyPartialToAsync(stream.BaseStream, writeRecord.Length, cancellationToken: cancellationToken).DynamicContext();
                    break;
                case BackupLengthRecord lengthRecord:
                    // Restore the old stream length
                    if (Trace) Logging.WriteTrace($"ACID stream restoring {lengthRecord.OldLength} byte length from {lengthRecord.NewLength} byte length");
                    if (lengthRecord.DataLength != 0)
                    {
                        // Old stream length was larger
                        if (Trace) Logging.WriteTrace($"ACID stream restoring {lengthRecord.DataLength} byte cutted off data");
                        stream.Backup.Position = record.Offset + LENGTH_META_LEN + TIME_META_LEN;
                        stream.BaseStream.Position = lengthRecord.NewLength;
                        await stream.Backup.CopyExactlyPartialToAsync(stream.BaseStream, lengthRecord.DataLength, cancellationToken: cancellationToken).DynamicContext();
                    }
                    else
                    {
                        // Old stream length was smaller
                        if (Trace) Logging.WriteTrace($"ACID stream rollback cutting target stream to {lengthRecord.OldLength} byte");
                        stream.BaseStream.SetLength(lengthRecord.OldLength);
                    }
                    break;
                default:
                    throw new ArgumentException($"Unexpected record type {record.GetType()}", nameof(record));
            }
        }
    }
}

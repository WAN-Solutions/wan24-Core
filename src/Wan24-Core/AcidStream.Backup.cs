using static wan24.Core.Logging; 

namespace wan24.Core
{
    // Backup stream basic functionality
    public partial class AcidStream<T> where T : Stream
    {
        /// <summary>
        /// Initialize the backup stream
        /// </summary>
        /// <param name="target">Target stream</param>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serialization buffer (8 byte required)</param>
        /// <param name="flush">Flush after initialization?</param>
        public static void InitializeBackupStream(in T target, in Stream backup, in RentedMemory<byte>? buffer = null, in bool flush = true)
        {
            if(Debug) Logging.WriteDebug($"ACID backup stream {backup} initialization from {target}");
            if (backup.Length != 0) throw new ArgumentException("Empty stream required", nameof(backup));
            ValidateSerializationBuffer(buffer);
            using RentedMemory<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedMemory<byte> data = buffer ?? serializerBuffer!.Value;
            backup.SetLength(0);
            backup.Write(target.Length.GetBytes(data.Memory.Span));
            if (flush) backup.Flush();
        }

        /// <summary>
        /// Initialize the backup stream
        /// </summary>
        /// <param name="target">Target stream</param>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serialization buffer (8 byte required)</param>
        /// <param name="flush">Flush after initialization?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task InitializeBackupStreamAsync(T target, Stream backup, RentedMemory<byte>? buffer = null, bool flush = true, CancellationToken cancellationToken = default)
        {
            if (Debug) Logging.WriteDebug($"ACID backup stream {backup} initialization from {target}");
            if (backup.Length != 0) throw new ArgumentException("Empty stream required", nameof(backup));
            ValidateSerializationBuffer(buffer);
            using RentedMemory<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedMemory<byte> data = buffer ?? serializerBuffer!.Value;
            backup.SetLength(0);
            await backup.WriteAsync(target.Length.GetBytes(data.Memory), cancellationToken).DynamicContext();
            if (flush) await backup.FlushAsync(cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Read the original target stream length from the backup
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serialization buffer (8 byte required)</param>
        /// <returns>Original target stream length in byte</returns>
        public static long ReadLengthFromBackup(in Stream backup, in RentedMemory<byte>? buffer = null)
        {
            if (Debug) Logging.WriteDebug("Reading original ACID target stream length from backup");
            if (backup.Position != 0) throw new ArgumentException("Invalid offset", nameof(backup));
            ValidateSerializationBuffer(buffer);
            using RentedMemory<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedMemory<byte> data = buffer ?? serializerBuffer!.Value;
            Span<byte> dataSpan = data.Memory.Span;
            backup.ReadExactly(dataSpan);
            long res = dataSpan.ToLong();
            if (Trace) Logging.WriteTrace($"ACID target stream original length was {res} byte");
            if (res < 0) throw new InvalidDataException($"Invalid length {res}");
            return res;
        }

        /// <summary>
        /// Read the original target stream length from the backup
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serialization buffer (8 byte required)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Original target stream length in byte</returns>
        public static async Task<long> ReadLengthFromBackupAsync(Stream backup, RentedMemory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            if (Debug) Logging.WriteDebug("Reading original ACID target stream length from backup");
            if (backup.Position != 0) throw new ArgumentException("Invalid offset", nameof(backup));
            ValidateSerializationBuffer(buffer);
            using RentedMemory<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedMemory<byte> data = buffer ?? serializerBuffer!.Value;
            await backup.ReadExactlyAsync(data.Memory, cancellationToken).DynamicContext();
            long res = data.Memory.Span.ToLong();
            if (Trace) Logging.WriteTrace($"ACID target stream original length was {res} byte");
            if (res < 0) throw new InvalidDataException($"Invalid length {res}");
            return res;
        }

        /// <summary>
        /// Investigate a backup stream
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serializer buffer (8 byte required)</param>
        /// <returns>Number of backup records</returns>
        public static long InvestigateBackup(in Stream backup, in RentedMemory<byte>? buffer = null)
        {
            if (Debug) Logging.WriteDebug("Investigating ACID backup stream");
            ValidateSerializationBuffer(buffer);
            backup.Position = 0;
            using RentedMemory<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedMemory<byte> data = buffer ?? serializerBuffer!.Value;
            long targetLength = ReadLengthFromBackup(backup, data);
            if (Trace) Logging.WriteTrace($"Using original ACID target stream length {targetLength} byte");
            DateTime lastTimestamp = DateTime.MinValue;
            long i = 0;
            for (; ; i++)
                try
                {
                    if (Trace) Logging.WriteTrace($"Reading ACID record #{i}");
                    if (ReadBackupRecordForward(backup, targetLength, data) is not BackupRecordBase record) break;
                    if (Trace) Logging.WriteTrace($"Red ACID record {record} with time {record.Time} (#{i})");
                    if (record.Time < lastTimestamp) throw new InvalidDataException($"Invalid backup record time {record.Time} ({lastTimestamp}) at #{i}");
                    lastTimestamp = record.Time;
                    switch (record)
                    {
                        case BackupWriteRecord:
                            break;
                        case BackupLengthRecord lengthRecord:
                            if (lengthRecord.OldLength < targetLength) throw new InvalidDataException($"Invalid old length {lengthRecord.OldLength} at #{i}");
                            targetLength = lengthRecord.NewLength;
                            if (Trace) Logging.WriteTrace($"Using new original ACID target stream length {targetLength} byte");
                            break;
                        default:
                            throw new InvalidProgramException($"Unexpected record type {record.GetType()}");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Backup record investigation failed at #{i}: {ex.Message}", ex);
                }
            if (Trace) Logging.WriteTrace($"Done investigating ACID backup stream, found {i} records");
            return i;
        }

        /// <summary>
        /// Investigate a backup stream
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serializer buffer (8 byte required)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of backup records</returns>
        public static async Task<long> InvestigateBackupAsync(Stream backup, RentedMemory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            if (Debug) Logging.WriteDebug("Investigating ACID backup stream");
            ValidateSerializationBuffer(buffer);
            backup.Position = 0;
            using RentedMemory<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedMemory<byte> data = buffer ?? serializerBuffer!.Value;
            long targetLength = await ReadLengthFromBackupAsync(backup, data, cancellationToken).DynamicContext();
            if (Trace) Logging.WriteTrace($"Using original ACID target stream length {targetLength} byte");
            DateTime lastTimestamp = DateTime.MinValue;
            long i = 0;
            for (; ; i++)
                try
                {
                    if (Trace) Logging.WriteTrace($"Reading ACID record #{i}");
                    if (await ReadBackupRecordForwardAsync(backup, targetLength, data, cancellationToken).DynamicContext() is not BackupRecordBase record) break;
                    if (Trace) Logging.WriteTrace($"Red ACID record {record} with time {record.Time} (#{i})");
                    if (record.Time < lastTimestamp) throw new InvalidDataException($"Invalid backup record time {record.Time} ({lastTimestamp}) at #{i}");
                    lastTimestamp = record.Time;
                    switch (record)
                    {
                        case BackupWriteRecord:
                            break;
                        case BackupLengthRecord lengthRecord:
                            if (lengthRecord.OldLength < targetLength) throw new InvalidDataException($"Invalid old length {lengthRecord.OldLength} at #{i}");
                            targetLength = lengthRecord.NewLength;
                            if (Trace) Logging.WriteTrace($"Using new original ACID target stream length {targetLength} byte");
                            break;
                        default:
                            throw new InvalidProgramException($"Unexpected record type {record.GetType()}");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Backup record investigation failed at #{i}: {ex.Message}", ex);
                }
            if (Trace) Logging.WriteTrace($"Done investigating ACID backup stream, found {i} records");
            return i;
        }
    }
}

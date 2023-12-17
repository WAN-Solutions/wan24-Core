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
        public static void InitializeBackupStream(in T target, in Stream backup, in RentedArray<byte>? buffer = null, in bool flush = true)
        {
            Logging.WriteDebug($"ACID backup stream {backup} initialization from {target}");
            if (backup.Length != 0) throw new ArgumentException("Empty stream required", nameof(backup));
            ValidateSerializationBuffer(buffer);
            using RentedArray<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedArray<byte> data = buffer ?? serializerBuffer!;
            backup.SetLength(0);
            backup.Write(target.Length.GetBytes(data.Span));
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
        public static async Task InitializeBackupStreamAsync(T target, Stream backup, RentedArray<byte>? buffer = null, bool flush = true, CancellationToken cancellationToken = default)
        {
            Logging.WriteDebug($"ACID backup stream {backup} initialization from {target}");
            if (backup.Length != 0) throw new ArgumentException("Empty stream required", nameof(backup));
            ValidateSerializationBuffer(buffer);
            using RentedArray<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedArray<byte> data = buffer ?? serializerBuffer!;
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
        public static long ReadLengthFromBackup(in Stream backup, in RentedArray<byte>? buffer = null)
        {
            Logging.WriteDebug("Reading original ACID target stream length from backup");
            if (backup.Position != 0) throw new ArgumentException("Invalid offset", nameof(backup));
            ValidateSerializationBuffer(buffer);
            using RentedArray<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedArray<byte> data = buffer ?? serializerBuffer!;
            backup.ReadExactly(data.Span);
            long res = data.Span.ToLong();
            Logging.WriteTrace($"ACID target stream original length was {res} byte");
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
        public static async Task<long> ReadLengthFromBackupAsync(Stream backup, RentedArray<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            Logging.WriteDebug("Reading original ACID target stream length from backup");
            if (backup.Position != 0) throw new ArgumentException("Invalid offset", nameof(backup));
            ValidateSerializationBuffer(buffer);
            using RentedArray<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedArray<byte> data = buffer ?? serializerBuffer!;
            await backup.ReadExactlyAsync(data.Memory, cancellationToken).DynamicContext();
            long res = data.Span.ToLong();
            Logging.WriteTrace($"ACID target stream original length was {res} byte");
            if (res < 0) throw new InvalidDataException($"Invalid length {res}");
            return res;
        }

        /// <summary>
        /// Investigate a backup stream
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serializer buffer (8 byte required)</param>
        /// <returns>Number of backup records</returns>
        public static long InvestigateBackup(in Stream backup, in RentedArray<byte>? buffer = null)
        {
            Logging.WriteDebug("Investigating ACID backup stream");
            ValidateSerializationBuffer(buffer);
            backup.Position = 0;
            using RentedArray<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedArray<byte> data = buffer ?? serializerBuffer!;
            long targetLength = ReadLengthFromBackup(backup, data);
            Logging.WriteTrace($"Using original ACID target stream length {targetLength} byte");
            DateTime lastTimestamp = DateTime.MinValue;
            long i = 0;
            for (; ; i++)
                try
                {
                    Logging.WriteTrace($"Reading ACID record #{i}");
                    if (ReadBackupRecordForward(backup, targetLength, data) is not BackupRecordBase record) break;
                    Logging.WriteTrace($"Red ACID record {record} with time {record.Time} (#{i})");
                    if (record.Time < lastTimestamp) throw new InvalidDataException($"Invalid backup record time {record.Time} ({lastTimestamp}) at #{i}");
                    lastTimestamp = record.Time;
                    switch (record)
                    {
                        case BackupWriteRecord:
                            break;
                        case BackupLengthRecord lengthRecord:
                            if (lengthRecord.OldLength < targetLength) throw new InvalidDataException($"Invalid old length {lengthRecord.OldLength} at #{i}");
                            targetLength = lengthRecord.NewLength;
                            Logging.WriteTrace($"Using new original ACID target stream length {targetLength} byte");
                            break;
                        default:
                            throw new InvalidProgramException($"Unexpected record type {record.GetType()}");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Backup record investigation failed at #{i}: {ex.Message}", ex);
                }
            Logging.WriteTrace($"Done investigating ACID backup stream, found {i} records");
            return i;
        }

        /// <summary>
        /// Investigate a backup stream
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serializer buffer (8 byte required)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of backup records</returns>
        public static async Task<long> InvestigateBackupAsync(Stream backup, RentedArray<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            Logging.WriteDebug("Investigating ACID backup stream");
            ValidateSerializationBuffer(buffer);
            backup.Position = 0;
            using RentedArray<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedArray<byte> data = buffer ?? serializerBuffer!;
            long targetLength = await ReadLengthFromBackupAsync(backup, data, cancellationToken).DynamicContext();
            Logging.WriteTrace($"Using original ACID target stream length {targetLength} byte");
            DateTime lastTimestamp = DateTime.MinValue;
            long i = 0;
            for (; ; i++)
                try
                {
                    Logging.WriteTrace($"Reading ACID record #{i}");
                    if (await ReadBackupRecordForwardAsync(backup, targetLength, data, cancellationToken).DynamicContext() is not BackupRecordBase record) break;
                    Logging.WriteTrace($"Red ACID record {record} with time {record.Time} (#{i})");
                    if (record.Time < lastTimestamp) throw new InvalidDataException($"Invalid backup record time {record.Time} ({lastTimestamp}) at #{i}");
                    lastTimestamp = record.Time;
                    switch (record)
                    {
                        case BackupWriteRecord:
                            break;
                        case BackupLengthRecord lengthRecord:
                            if (lengthRecord.OldLength < targetLength) throw new InvalidDataException($"Invalid old length {lengthRecord.OldLength} at #{i}");
                            targetLength = lengthRecord.NewLength;
                            Logging.WriteTrace($"Using new original ACID target stream length {targetLength} byte");
                            break;
                        default:
                            throw new InvalidProgramException($"Unexpected record type {record.GetType()}");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Backup record investigation failed at #{i}: {ex.Message}", ex);
                }
            Logging.WriteTrace($"Done investigating ACID backup stream, found {i} records");
            return i;
        }

        /// <summary>
        /// Read the next backup record (offset will be at the beginning of the next record after the red record)
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="targetLength">Original target stream length from the current backup offset</param>
        /// <param name="buffer">Serialization buffer (8 byte required)</param>
        /// <returns>Record or <see langword="null"/> on EOF</returns>
        public static BackupRecordBase? ReadBackupRecordForward(in Stream backup, in long targetLength, in RentedArray<byte>? buffer = null)
        {
            Logging.WriteDebug($"Reading next ACID record from offset {backup.Position} (original target stream length {targetLength} byte)");
            if (backup.Position < sizeof(long)) throw new ArgumentException("Invalid offset", nameof(backup));
            ArgumentOutOfRangeException.ThrowIfNegative(targetLength);
            ValidateSerializationBuffer(buffer);
            long offset = backup.Position;
            if (ReadRecordType(backup, allowEof: true) is not IoTypes type) return null;
            Logging.WriteTrace($"Going to read ACID record {type}");
            using RentedArray<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedArray<byte> data = buffer ?? serializerBuffer!;
            switch (type)
            {
                case IoTypes.Write:
                    {
                        DateTime timestamp = ReadTimestamp(backup, data);
                        Logging.WriteTrace($"Red ACID write record time {timestamp}");
                        long pos = ReadPositiveLong(backup, data),
                            newOffset;
                        Logging.WriteTrace($"Red ACID write record offset {pos} byte");
                        if (pos >= targetLength) throw new InvalidDataException($"Invalid target offset {pos}");
                        int len = ReadPositiveInt(backup, data);
                        Logging.WriteTrace($"Red ACID write record data length {len} byte");
                        if (len < 1 || pos + len > targetLength) throw new InvalidDataException($"Invalid backup data length {len}");
                        newOffset = len == 0
                            ? offset + TIME_META_LEN + (WRITE_META_LEN << 1) - sizeof(int)
                            : offset + TIME_META_LEN + (WRITE_META_LEN << 1) + len;
                        Logging.WriteTrace($"ACID write record end at offset {newOffset} byte");
                        if (newOffset > backup.Length) throw new InvalidDataException($"Not enough backup data for reading {len} byte");
                        backup.Position = newOffset;
                        return new BackupWriteRecord(offset, timestamp, pos, len);
                    }
                case IoTypes.Length:
                    {
                        DateTime timestamp = ReadTimestamp(backup, data);
                        Logging.WriteTrace($"Red ACID length record time {timestamp}");
                        long oldLen = ReadPositiveLong(backup, data);
                        Logging.WriteTrace($"Red ACID length record old length {oldLen} byte");
                        if (oldLen < targetLength) throw new InvalidDataException($"Invalid old length {oldLen}");
                        long newLen = ReadPositiveLong(backup, data),
                            dataLen = Math.Max(0, oldLen - newLen),
                            newOffset;
                        Logging.WriteTrace($"Red ACID length record new length {newLen} byte ({dataLen} stored backup data)");
                        newOffset = dataLen == 0
                            ? offset + TIME_META_LEN + (LENGTH_META_LEN << 1)
                            : offset + TIME_META_LEN + (LENGTH_META_LEN << 1) + dataLen;
                        Logging.WriteTrace($"ACID length record end at offset {newOffset} byte");
                        if (newOffset > backup.Length) throw new IOException($"Not enough backup data for reading {dataLen} byte (offset {offset} -> {newOffset}), length {backup.Length}");
                        backup.Position = newOffset;
                        return new BackupLengthRecord(offset, timestamp, oldLen, newLen, dataLen);
                    }
                default:
                    throw new InvalidProgramException($"Invalid record type {type}");
            }
        }

        /// <summary>
        /// Read the next backup record (offset will be at the beginning of the next record after the red record)
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="targetLength">Original target stream length from the current backup offset</param>
        /// <param name="buffer">Serialization buffer (8 byte required)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Record or <see langword="null"/> on EOF</returns>
        public static async Task<BackupRecordBase?> ReadBackupRecordForwardAsync(Stream backup, long targetLength, RentedArray<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            Logging.WriteDebug($"Reading next ACID record from offset {backup.Position} (original target stream length {targetLength} byte)");
            if (backup.Position < sizeof(long)) throw new ArgumentException("Invalid offset", nameof(backup));
            ArgumentOutOfRangeException.ThrowIfNegative(targetLength);
            ValidateSerializationBuffer(buffer);
            long offset = backup.Position;
            if (ReadRecordType(backup, allowEof: true) is not IoTypes type) return null;
            Logging.WriteTrace($"Going to read ACID record {type}");
            using RentedArray<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedArray<byte> data = buffer ?? serializerBuffer!;
            switch (type)
            {
                case IoTypes.Write:
                    {
                        DateTime timestamp = await ReadTimestampAsync(backup, data, cancellationToken).DynamicContext();
                        Logging.WriteTrace($"Red ACID write record time {timestamp}");
                        long pos = await ReadPositiveLongAsync(backup, data, cancellationToken).DynamicContext(),
                            newOffset;
                        Logging.WriteTrace($"Red ACID write record offset {pos} byte");
                        if (pos >= targetLength) throw new InvalidDataException($"Invalid target offset {pos}");
                        int len = await ReadPositiveIntAsync(backup, data, cancellationToken).DynamicContext();
                        Logging.WriteTrace($"Red ACID write record data length {len} byte");
                        if (len < 1 || pos + len > targetLength) throw new InvalidDataException($"Invalid backup data length {len}");
                        newOffset = len == 0
                            ? offset + TIME_META_LEN + (WRITE_META_LEN << 1) - sizeof(int)
                            : offset + TIME_META_LEN + (WRITE_META_LEN << 1) + len;
                        Logging.WriteTrace($"ACID write record end at offset {newOffset} byte");
                        if (newOffset > backup.Length) throw new InvalidDataException($"Not enough backup data for reading {len} byte");
                        backup.Position = newOffset;
                        return new BackupWriteRecord(offset, timestamp, pos, len);
                    }
                case IoTypes.Length:
                    {
                        DateTime timestamp = await ReadTimestampAsync(backup, data, cancellationToken).DynamicContext();
                        Logging.WriteTrace($"Red ACID length record time {timestamp}");
                        long oldLen = await ReadPositiveLongAsync(backup, data, cancellationToken).DynamicContext();
                        Logging.WriteTrace($"Red ACID length record old length {oldLen} byte");
                        if (oldLen < targetLength) throw new InvalidDataException($"Invalid old length {oldLen}");
                        long newLen = await ReadPositiveLongAsync(backup, data, cancellationToken).DynamicContext(),
                            dataLen = Math.Max(0, oldLen - newLen),
                            newOffset;
                        Logging.WriteTrace($"Red ACID length record new length {newLen} byte ({dataLen} stored backup data)");
                        newOffset = dataLen == 0
                            ? offset + TIME_META_LEN + (LENGTH_META_LEN << 1) - sizeof(long)
                            : offset + TIME_META_LEN + (LENGTH_META_LEN << 1) + dataLen;
                        Logging.WriteTrace($"ACID length record end at offset {newOffset} byte");
                        if (newOffset > backup.Length) throw new IOException($"Not enough backup data for reading {dataLen} byte (offset {offset} -> {newOffset}, length {backup.Length})");
                        backup.Position = newOffset;
                        return new BackupLengthRecord(offset, timestamp, oldLen, newLen, dataLen);
                    }
                default:
                    throw new InvalidProgramException($"Invalid record type {type}");
            }
        }

        /// <summary>
        /// Read the previous backup record (offset will be at the end of the previous record before the red record)
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serialization buffer (8 byte required)</param>
        /// <returns>Record or <see langword="null"/> on EOF</returns>
        public static BackupRecordBase? ReadBackupRecordBackward(in Stream backup, in RentedArray<byte>? buffer = null)
        {
            ValidateSerializationBuffer(buffer);
            long offset = backup.Position;
            Logging.WriteDebug($"Reading previous ACID record from offset {offset}");
            if (offset < sizeof(long)) throw new ArgumentException("Invalid offset", nameof(backup));
            if (offset == sizeof(long)) return null;
            if (offset < TIME_META_LEN + (WRITE_META_LEN << 1) - sizeof(int)) throw new InvalidDataException("Not enough data from the current offset");
            offset--;
            backup.Position = offset;
            if (ReadRecordType(backup, allowEof: false) is not IoTypes type) throw new IOException("Failed to read the record type");
            Logging.WriteTrace($"Going to read ACID record {type}");
            using RentedArray<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedArray<byte> data = buffer ?? serializerBuffer!;
            switch (type)
            {
                case IoTypes.Write:
                    {
                        offset -= sizeof(long);
                        backup.Position = offset;
                        long pos = ReadPositiveLong(backup, data);
                        Logging.WriteTrace($"Red ACID write record offset {pos}");
                        offset -= sizeof(int);
                        backup.Position = offset;
                        int len = ReadPositiveInt(backup, data);
                        Logging.WriteTrace($"Red ACID write record stored data length {len} byte");
                        long dataLen = len == 0 ? WRITE_META_LEN + TIME_META_LEN - sizeof(int) : WRITE_META_LEN + TIME_META_LEN + len;
                        if (len < 1 || offset < sizeof(long) + dataLen) throw new InvalidDataException($"Invalid backup data length {len}");
                        offset -= dataLen;
                        backup.Position = offset + sizeof(byte);
                        DateTime timestamp = ReadTimestamp(backup, data);
                        Logging.WriteTrace($"Red ACID write record time {timestamp}");
                        backup.Position = offset;
                        return new BackupWriteRecord(offset, timestamp, pos, len);
                    }
                case IoTypes.Length:
                    {
                        offset -= sizeof(long);
                        backup.Position = offset;
                        long oldLen = ReadPositiveLong(backup, data);
                        Logging.WriteTrace($"Red ACID length record old length {oldLen} byte");
                        offset -= sizeof(long);
                        backup.Position = offset;
                        long newLen = ReadPositiveLong(backup, data),
                            len = Math.Max(0, oldLen - newLen),
                            dataLen = len == 0 ? LENGTH_META_LEN + TIME_META_LEN : LENGTH_META_LEN + TIME_META_LEN + len;
                        Logging.WriteTrace($"Red ACID length record new length {newLen} byte ({len} byte stored backup data)");
                        if (offset < sizeof(long) + dataLen) throw new InvalidDataException($"Invalid backup data length {dataLen}");
                        offset -= dataLen;
                        backup.Position = offset + sizeof(byte);
                        DateTime timestamp = ReadTimestamp(backup, data);
                        Logging.WriteTrace($"Red ACID length record time {timestamp}");
                        backup.Position = offset;
                        return new BackupLengthRecord(offset, timestamp, oldLen, newLen, len);
                    }
                default:
                    throw new InvalidProgramException($"Invalid record type {type}");
            }
        }

        /// <summary>
        /// Read the previous backup record (offset will be at the end of the previous record before the red record)
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serialization buffer (8 byte required)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Record or <see langword="null"/> on EOF</returns>
        public static async Task<BackupRecordBase?> ReadBackupRecordBackwardAsync(Stream backup, RentedArray<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            ValidateSerializationBuffer(buffer);
            long offset = backup.Position;
            Logging.WriteDebug($"Reading previous ACID record from offset {offset}");
            if (offset < sizeof(long)) throw new ArgumentException("Invalid offset", nameof(backup));
            if (offset == sizeof(long)) return null;
            if (offset < TIME_META_LEN + (WRITE_META_LEN << 1) - sizeof(int)) throw new InvalidDataException("Not enough data from the current offset");
            offset--;
            backup.Position = offset;
            if (ReadRecordType(backup, allowEof: false) is not IoTypes type) throw new IOException("Failed to read the record type");
            Logging.WriteTrace($"Going to read ACID record {type}");
            using RentedArray<byte>? serializerBuffer = buffer is null ? new(len: sizeof(long), clean: false) : null;
            RentedArray<byte> data = buffer ?? serializerBuffer!;
            switch (type)
            {
                case IoTypes.Write:
                    {
                        offset -= sizeof(long);
                        backup.Position = offset;
                        long pos = await ReadPositiveLongAsync(backup, data, cancellationToken).DynamicContext();
                        Logging.WriteTrace($"Red ACID write record offset {pos}");
                        offset -= sizeof(int);
                        backup.Position = offset;
                        int len = ReadPositiveInt(backup, data);
                        Logging.WriteTrace($"Red ACID write record stored data length {len} byte");
                        long dataLen = len == 0 ? WRITE_META_LEN + TIME_META_LEN - sizeof(int) : WRITE_META_LEN + TIME_META_LEN + len;
                        if (len < 1 || offset < sizeof(long) + dataLen) throw new InvalidDataException($"Invalid backup data length {len}");
                        offset -= dataLen;
                        backup.Position = offset + sizeof(byte);
                        DateTime timestamp = await ReadTimestampAsync(backup, data, cancellationToken).DynamicContext();
                        Logging.WriteTrace($"Red ACID write record time {timestamp}");
                        backup.Position = offset;
                        return new BackupWriteRecord(offset, timestamp, pos, len);
                    }
                case IoTypes.Length:
                    {
                        offset -= sizeof(long);
                        backup.Position = offset;
                        long oldLen = await ReadPositiveLongAsync(backup, data, cancellationToken).DynamicContext();
                        Logging.WriteTrace($"Red ACID length record old length {oldLen} byte");
                        offset -= sizeof(long);
                        backup.Position = offset;
                        long newLen = await ReadPositiveLongAsync(backup, data, cancellationToken).DynamicContext(),
                            len = Math.Max(0, oldLen - newLen),
                            dataLen = len == 0 ? LENGTH_META_LEN + TIME_META_LEN : LENGTH_META_LEN + TIME_META_LEN + len;
                        Logging.WriteTrace($"Red ACID length record new length {newLen} byte ({len} byte stored backup data)");
                        if (offset < sizeof(long) + dataLen) throw new InvalidDataException($"Invalid backup data length {dataLen}");
                        offset -= dataLen;
                        backup.Position = offset + sizeof(byte);
                        DateTime timestamp = await ReadTimestampAsync(backup, data, cancellationToken).DynamicContext();
                        Logging.WriteTrace($"Red ACID length record time {timestamp}");
                        backup.Position = offset;
                        return new BackupLengthRecord(offset, timestamp, oldLen, newLen, len);
                    }
                default:
                    throw new InvalidProgramException($"Invalid record type {type}");
            }
        }
    }
}

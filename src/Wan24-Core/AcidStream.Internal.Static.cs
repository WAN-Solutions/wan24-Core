using static wan24.Core.Logging;

namespace wan24.Core
{
    // Internal static methods
    public partial class AcidStream<T> where T : Stream
    {
        /// <summary>
        /// Write record meta data length in byte
        /// </summary>
        protected const int WRITE_META_LEN = sizeof(byte) /* Record type indicator */ + sizeof(long) /* Offset */ + sizeof(int) /* Backup data length */;
        /// <summary>
        /// New length record meta data length in byte
        /// </summary>
        protected const int LENGTH_META_LEN = sizeof(byte) /* Record type indicator */ + sizeof(long) /* Old length */ + sizeof(long) /* New length */;
        /// <summary>
        /// Length of the time meta data in byte
        /// </summary>
        protected const int TIME_META_LEN = sizeof(long);

        /// <summary>
        /// Validate the given serialization buffer
        /// </summary>
        /// <param name="buffer">Buffer (8 byte required)</param>
        protected static void ValidateSerializationBuffer(RentedMemory<byte>? buffer)
        {
            if (buffer.HasValue && buffer.Value.Memory.Length != sizeof(long)) throw new ArgumentException($"{sizeof(long)} byte buffer required", nameof(buffer));
        }

        /// <summary>
        /// Read the record type
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="allowEof">Allow EOF?</param>
        /// <returns>Record type</returns>
        protected static IoTypes? ReadRecordType(in Stream backup, in bool allowEof)
        {
            if (Trace) Logging.WriteTrace($"Reading ACID record type from offset {backup.Position}");
            int ioType = backup.ReadByte();
            if (ioType == -1)
            {
                if (!allowEof) throw new IOException("Unexpected EOF when reading record type");
                return null;
            }
            IoTypes type = (IoTypes)ioType;
            if (Trace) Logging.WriteTrace($"Red ACID record type {type} (#{ioType})");
            return type switch
            {
                IoTypes.Write or IoTypes.Length => (AcidStream<T>.IoTypes?)type,
                _ => throw new InvalidDataException($"Invalid record type #{ioType} ({type}) at offset {backup.Position - 1}/{backup.Length}")
            };
        }

        /// <summary>
        /// Read the timestamp
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serialization buffer (8 byte required)</param>
        /// <returns>Timestamp</returns>
        protected static DateTime ReadTimestamp(in Stream backup, in RentedMemory<byte> buffer)
        {
            if (Trace) Logging.WriteTrace("Going to read ACID record time");
            DateTime res = new(ReadPositiveLong(backup, buffer), DateTimeKind.Utc);
            if (res > DateTime.UtcNow) throw new InvalidDataException($"Timestamp {res} in the future");
            return res;
        }

        /// <summary>
        /// Read the timestamp
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serialization buffer (8 byte required)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Timestamp</returns>
        protected static async Task<DateTime> ReadTimestampAsync(Stream backup, RentedMemory<byte> buffer, CancellationToken cancellationToken)
        {
            if (Trace) Logging.WriteTrace("Going to read ACID record time");
            DateTime res = new(await ReadPositiveLongAsync(backup, buffer, cancellationToken).DynamicContext(), DateTimeKind.Utc);
            if (res > DateTime.UtcNow) throw new InvalidDataException($"Timestamp {res} in the future");
            return res;
        }

        /// <summary>
        /// Read a positive 64 bit integer
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serialization buffer (8 byte required)</param>
        /// <returns>Timestamp</returns>
        protected static long ReadPositiveLong(in Stream backup, in RentedMemory<byte> buffer)
        {
            if (Trace) Logging.WriteTrace($"Going to read positive 64 bit integer from ACID record from offset {backup.Position}");
            Span<byte> bufferSpan = buffer.Memory.Span;
            backup.ReadExactly(bufferSpan);
            long res = bufferSpan.ToLong();
            if (Trace) Logging.WriteTrace($"Red positive 64 bit integer {res} from ACID record");
            if (res < 0) throw new InvalidDataException($"Invalid negative 64 bit integer {res}");
            return res;
        }

        /// <summary>
        /// Read a positive 64 bit integer
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serialization buffer (8 byte required)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Timestamp</returns>
        protected static async Task<long> ReadPositiveLongAsync(Stream backup, RentedMemory<byte> buffer, CancellationToken cancellationToken)
        {
            if (Trace) Logging.WriteTrace($"Going to read positive 64 bit integer from ACID record from offset {backup.Position}");
            await backup.ReadExactlyAsync(buffer.Memory, cancellationToken).DynamicContext();
            long res = buffer.Memory.Span.ToLong();
            if (Trace) Logging.WriteTrace($"Red positive 64 bit integer {res} from ACID record");
            if (res < 0) throw new InvalidDataException($"Invalid negative 64 bit integer {res}");
            return res;
        }

        /// <summary>
        /// Read a positive 32 bit integer
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serialization buffer (8 byte required)</param>
        /// <returns>Timestamp</returns>
        protected static int ReadPositiveInt(in Stream backup, in RentedMemory<byte> buffer)
        {
            if (Trace) Logging.WriteTrace($"Going to read positive 32 bit integer from ACID record from offset {backup.Position}");
            Span<byte> bufferSpan = buffer.Memory.Span;
            backup.ReadExactly(bufferSpan[..sizeof(int)]);
            int res = bufferSpan.ToInt();
            if (Trace) Logging.WriteTrace($"Red positive 32 bit integer {res} from ACID record");
            if (res < 0) throw new InvalidDataException($"Invalid negative 32 bit integer {res}");
            return res;
        }

        /// <summary>
        /// Read a positive 32 bit integer
        /// </summary>
        /// <param name="backup">Backup stream</param>
        /// <param name="buffer">Serialization buffer (8 byte required)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Timestamp</returns>
        protected static async Task<int> ReadPositiveIntAsync(Stream backup, RentedMemory<byte> buffer, CancellationToken cancellationToken)
        {
            if (Trace) Logging.WriteTrace($"Going to read positive 32 bit integer from ACID record from offset {backup.Position}");
            await backup.ReadExactlyAsync(buffer.Memory[..sizeof(int)], cancellationToken).DynamicContext();
            int res = buffer.Memory.Span.ToInt();
            if (Trace) Logging.WriteTrace($"Red positive 32 bit integer {res} from ACID record");
            if (res < 0) throw new InvalidDataException($"Invalid negative 32 bit integer {res}");
            return res;
        }
    }
}

namespace wan24.Core
{
    // Backup records
    public partial class AcidStream<T> where T : Stream
    {
        /// <summary>
        /// IO types
        /// </summary>
        public enum IoTypes : byte
        {
            /// <summary>
            /// Write operation
            /// </summary>
            Write = 0,
            /// <summary>
            /// New length operation
            /// </summary>
            Length = 1
        }

        /// <summary>
        /// Base class for a backup record
        /// </summary>
        public abstract class BackupRecordBase
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="offset">Backup stream byte offset</param>
            /// <param name="type">Record type</param>
            /// <param name="time">Timestamp</param>
            protected BackupRecordBase(long offset, IoTypes type, DateTime time)
            {
                Offset = offset;
                Type = type;
                Time = time;
            }

            /// <summary>
            /// Backup stream byte offset
            /// </summary>
            public long Offset { get; }

            /// <summary>
            /// Record type
            /// </summary>
            public IoTypes Type { get; }

            /// <summary>
            /// Timestamp
            /// </summary>
            public DateTime Time { get; }
        }

        /// <summary>
        /// Write backup record
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="offset">Backup stream byte offset</param>
        /// <param name="timestamp">Timestamp</param>
        /// <param name="pos">Target stream position byte offset</param>
        /// <param name="len">Backup data length in byte</param>
        public sealed class BackupWriteRecord(long offset, DateTime timestamp, long pos, int len) : BackupRecordBase(offset, IoTypes.Write, timestamp)
        {
            /// <summary>
            /// Target stream position byte offset
            /// </summary>
            public long Position { get; } = pos;

            /// <summary>
            /// Backup data length in byte
            /// </summary>
            public int Length { get; } = len;
        }

        /// <summary>
        /// Length backup record
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="timestamp">Timestamp</param>
        /// <param name="oldLen">Old target stream length</param>
        /// <param name="newLen">New target stream length</param>
        /// <param name="dataLen">Backup data length in byte</param>
        /// <param name="offset">Backup stream byte offset</param>
        public sealed class BackupLengthRecord(long offset, DateTime timestamp, long oldLen, long newLen, long dataLen) : BackupRecordBase(offset, IoTypes.Length, timestamp)
        {
            /// <summary>
            /// Old target stream length in byte
            /// </summary>
            public long OldLength { get; } = oldLen;

            /// <summary>
            /// New target stream length in byte
            /// </summary>
            public long NewLength { get; } = newLen;

            /// <summary>
            /// Backup data length in byte
            /// </summary>
            public long DataLength { get; } = dataLen;
        }
    }
}

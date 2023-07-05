using System.Buffers.Binary;

namespace wan24.Core
{
    // Conversion
    public partial class Bitmap
    {
        /// <summary>
        /// Get the bitmap as byte array
        /// </summary>
        /// <param name="buffer">Target buffer</param>
        /// <returns>Bitmap</returns>
        /// <exception cref="OutOfMemoryException">Bitmap is larger than <see cref="int.MaxValue"/></exception>
        public byte[] ToArray(byte[]? buffer = null)
        {
            lock (SyncObject)
            {
                if (_Map.LongLength == 0) return _Map;
                if (_Map.LongLength > int.MaxValue) throw new OutOfMemoryException();
                long len = GetByteCount(BitCount);
                byte[] res = buffer ?? new byte[len];
                _Map.AsSpan(0, (int)Math.Min(len, res.LongLength)).CopyTo(res.AsSpan());
                return res;
            }
        }

        /// <summary>
        /// Get the bitmap as byte array
        /// </summary>
        /// <param name="buffer">Target buffer</param>
        /// <returns>Bitmap</returns>
        /// <exception cref="OutOfMemoryException">Bitmap is larger than <see cref="int.MaxValue"/></exception>
        public Span<byte> ToSpan(Span<byte> buffer)
        {
            lock (SyncObject)
            {
                if (_Map.LongLength == 0) return _Map;
                if (_Map.LongLength > int.MaxValue) throw new OutOfMemoryException();
                _Map.AsSpan(0, (int)Math.Min(GetByteCount(BitCount), buffer.Length)).CopyTo(buffer);
                return buffer;
            }
        }

        /// <summary>
        /// Get the bitmap as byte
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public byte ToByte(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(byte), (mem) => mem.Span[0]);

        /// <summary>
        /// Get the bitmap as signed byte
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public sbyte ToSByte(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(byte), (mem) => (sbyte)mem.Span[0]);

        /// <summary>
        /// Get the bitmap as unsigned short
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public ushort ToUShort(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(ushort), (mem) => BinaryPrimitives.ReadUInt16LittleEndian(mem.Span));

        /// <summary>
        /// Get the bitmap as short
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public short ToShort(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(short), (mem) => BinaryPrimitives.ReadInt16LittleEndian(mem.Span));

        /// <summary>
        /// Get the bitmap as unsigned int
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public uint ToUInt(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(uint), (mem) => BinaryPrimitives.ReadUInt32LittleEndian(mem.Span));

        /// <summary>
        /// Get the bitmap as int
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public int ToInt(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(int), (mem) => BinaryPrimitives.ReadInt32LittleEndian(mem.Span));

        /// <summary>
        /// Get the bitmap as unsigned long
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public ulong ToULong(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(ulong), (mem) => BinaryPrimitives.ReadUInt64LittleEndian(mem.Span));

        /// <summary>
        /// Get the bitmap as long
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public long ToLong(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(long), (mem) => BinaryPrimitives.ReadInt64LittleEndian(mem.Span));

        /// <summary>
        /// Ensure executing a function having a size matching span of the bitmap
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="offset">Byte offset</param>
        /// <param name="count">Byte count</param>
        /// <param name="function">Function</param>
        /// <returns>Return value</returns>
        protected T ExecuteWithEnsuredSpan<T>(int offset, int count, Func<ReadOnlyMemory<byte>, T> function)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
            lock (SyncObject)
            {
                long byteCount = GetByteCount(BitCount);
                if (offset < byteCount)
                {
                    if (offset + count < byteCount)
                    {
                        return function(_Map.AsMemory(offset, count));
                    }
                    else
                    {
                        using RentedArrayStruct<byte> buffer = new(count);
                        _Map.AsSpan(offset, (int)(byteCount - offset)).CopyTo(buffer.Span);
                        return function(buffer.Memory);
                    }
                }
                else
                {
                    using RentedArrayStruct<byte> buffer = new(count);
                    return function(buffer.Memory);
                }
            }
        }
    }
}

using System.Numerics;
using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class StreamExtensions_Tests : TestBase
    {
        private const int VERSION = 1;

        [TestMethod]
        public void GetRemainingBytes_Tests()
        {
            using MemoryPoolStream ms = new();
            ms.Write(RandomNumberGenerator.GetBytes(10));
            ms.Position = 3;
            Assert.AreEqual(7L, ms.GetRemainingBytes());
        }

        [TestMethod]
        public void CopyPartialTo_Tests()
        {
            using MemoryPoolStream ms1 = new();
            ms1.Write(RandomNumberGenerator.GetBytes(100));
            ms1.Position = 25;
            using MemoryPoolStream ms2 = new();
            Assert.AreEqual(0L, ms1.CopyPartialTo(ms2, count: 50));
            Assert.AreEqual(75L, ms1.Position);
            Assert.AreEqual(50L, ms2.Position);
            Assert.AreEqual(25L, ms1.CopyPartialTo(ms2, count: 50));
            Assert.AreEqual(100L, ms1.Position);
            Assert.AreEqual(75L, ms2.Position);
        }

        [TestMethod]
        public async Task CopyPartialToAsync_Tests()
        {
            using MemoryPoolStream ms1 = new();
            await ms1.WriteAsync(RandomNumberGenerator.GetBytes(100));
            ms1.Position = 25;
            using MemoryPoolStream ms2 = new();
            Assert.AreEqual(0L, await ms1.CopyPartialToAsync(ms2, count: 50));
            Assert.AreEqual(75L, ms1.Position);
            Assert.AreEqual(50L, ms2.Position);
            Assert.AreEqual(25L, await ms1.CopyPartialToAsync(ms2, count: 50));
            Assert.AreEqual(100L, ms1.Position);
            Assert.AreEqual(75L, ms2.Position);
        }

        [TestMethod]
        public void GenericSeek_Tests()
        {
            byte[] data = RandomNumberGenerator.GetBytes(100);
            using MemoryPoolStream ms = new();
            ms.Write(data);
            ms.GenericSeek(50, SeekOrigin.Begin);
            Assert.AreEqual(50L, ms.Position);
            Assert.AreEqual(data[50], (byte)ms.ReadByte());
            ms.GenericSeek(25, SeekOrigin.Current);
            Assert.AreEqual(76L, ms.Position);
            Assert.AreEqual(data[76], (byte)ms.ReadByte());
            ms.GenericSeek(-75, SeekOrigin.End);
            Assert.AreEqual(25L, ms.Position);
            Assert.AreEqual(data[25], (byte)ms.ReadByte());
        }

        [TestMethod]
        public void GenericCopyTo_Tests()
        {
            using MemoryPoolStream ms1 = new();
            ms1.Write(RandomNumberGenerator.GetBytes(100));
            ms1.Position = 25;
            using MemoryPoolStream ms2 = new();
            ms1.GenericCopyTo(ms2);
            Assert.AreEqual(100L, ms1.Position);
            Assert.AreEqual(75L, ms2.Position);
        }

        [TestMethod]
        public async Task GenericCopyToAsync_Tests()
        {
            using MemoryPoolStream ms1 = new();
            await ms1.WriteAsync(RandomNumberGenerator.GetBytes(100));
            ms1.Position = 25;
            using MemoryPoolStream ms2 = new();
            await ms1.GenericCopyToAsync(ms2);
            Assert.AreEqual(100L, ms1.Position);
            Assert.AreEqual(75L, ms2.Position);
        }

        [TestMethod]
        public void BasicSerialization_Tests()
        {
            //TODO Write more tests
            using MemoryStream ms = new();
            object? obj;
            int index = 0;
            foreach (Tuple<object?, Action<Stream>, Func<Stream, object?>, int?> data in new Tuple<object?, Action<Stream>, Func<Stream, object?>, int?>[]
            {
                // Bits
                new((sbyte)123,stream=>stream.Write((sbyte)123),stream=>stream.ReadOneSByte(VERSION),1),
                new((byte)123,stream=>stream.Write((byte)123),stream=>stream.ReadOneByte(VERSION),1),
                new((short)123,stream=>stream.Write((short)123),stream=>stream.ReadShort(VERSION),2),
                new((ushort)123,stream=>stream.Write((ushort)123),stream=>stream.ReadUShort(VERSION),2),
                new(123,stream=>stream.Write(123),stream=>stream.ReadInteger(VERSION),4),
                new((uint)123,stream=>stream.Write((uint)123),stream=>stream.ReadUInteger(VERSION),4),
                new((long)123,stream=>stream.Write((long)123),stream=>stream.ReadLong(VERSION),8),
                new((ulong)123,stream=>stream.Write((ulong)123),stream=>stream.ReadULong(VERSION),8),
                new((Int128)123,stream=>stream.Write((Int128)123),stream=>stream.ReadInt128(VERSION),16),
                new((UInt128)123,stream=>stream.Write((UInt128)123),stream=>stream.ReadUInt128(VERSION),16),
                new((Half)123,stream=>stream.Write((Half)123),stream=>stream.ReadHalf(VERSION),2),
                new((float)123,stream=>stream.Write((float)123),stream=>stream.ReadFloat(VERSION),4),
                new((double)123,stream=>stream.Write((double)123),stream=>stream.ReadDouble(VERSION),8),
                new((decimal)123,stream=>stream.Write((decimal)123),stream=>stream.ReadDecimal(VERSION),16),
                new((BigInteger)123,stream=>stream.Write((BigInteger)123),stream=>stream.ReadBigInteger(VERSION, new byte[2]),2),
                // Numeric
                new((sbyte)123,stream=>stream.WriteNumeric((sbyte)123),stream=>stream.ReadNumeric<sbyte>(VERSION),1),
                new((byte)123,stream=>stream.WriteNumeric((byte)123),stream=>stream.ReadNumeric<byte>(VERSION),1),
                new((short)123,stream=>stream.WriteNumeric((short)123),stream=>stream.ReadNumeric<short>(VERSION),1),
                new((ushort)123,stream=>stream.WriteNumeric((ushort)123),stream=>stream.ReadNumeric<ushort>(VERSION),1),
                new(123,stream=>stream.WriteNumeric(123),stream=>stream.ReadNumeric<int>(VERSION),1),
                new((uint)123,stream=>stream.WriteNumeric((uint)123),stream=>stream.ReadNumeric<uint>(VERSION),1),
                new((long)123,stream=>stream.WriteNumeric((long)123),stream=>stream.ReadNumeric<long>(VERSION),1),
                new((ulong)123,stream=>stream.WriteNumeric((ulong)123),stream=>stream.ReadNumeric<ulong>(VERSION),1),
                new((Int128)123,stream=>stream.WriteNumeric((Int128)123),stream=>stream.ReadNumeric<Int128>(VERSION),1),
                new((UInt128)123,stream=>stream.WriteNumeric((UInt128)123),stream=>stream.ReadNumeric<UInt128>(VERSION),1),
                new((Half)123,stream=>stream.WriteNumeric((Half)123),stream=>stream.ReadNumeric<Half>(VERSION),1),
                new((float)123,stream=>stream.WriteNumeric((float)123),stream=>stream.ReadNumeric<float>(VERSION),1),
                new((double)123,stream=>stream.WriteNumeric((double)123),stream=>stream.ReadNumeric<double>(VERSION),1),
                new((decimal)123,stream=>stream.WriteNumeric((decimal)123),stream=>stream.ReadNumeric<decimal>(VERSION),1),
                new((BigInteger)123,stream=>stream.WriteNumeric((BigInteger)123),stream=>stream.ReadNumeric<BigInteger>(VERSION, new byte[1]),1),
                new(null,stream=>stream.WriteNumericNullable((BigInteger?)null),stream=>stream.ReadNumericNullable<BigInteger>(VERSION, new byte[1]),1),
                // String
                new("test",stream=>stream.Write("test"),stream=>{
                    using RentedMemoryRef<char> buffer = new(len: 4, clean: false);
                    Span<char> bufferSpan=buffer.Span;
                    int len = stream.ReadString(VERSION, bufferSpan);
                    return new string(bufferSpan[..len]);
                },5),
                new("test",stream=>stream.Write16("test"),stream=>{
                    using RentedMemoryRef<char> buffer = new(len: 8, clean: false);
                    Span<char> bufferSpan=buffer.Span;
                    int len = stream.ReadString16(VERSION, bufferSpan);
                    return new string(bufferSpan[..len]);
                },9),
                new("test",stream=>stream.Write32("test"),stream=>{
                    using RentedMemoryRef<char> buffer = new(len: 16, clean: false);
                    Span<char> bufferSpan=buffer.Span;
                    int len = stream.ReadString32(VERSION, bufferSpan);
                    return new string(bufferSpan[..len]);
                },17),
                new(-1,stream=>stream.Write((string?)null),stream=>stream.ReadStringNullable(VERSION,new char[1]),1),
                new(-1,stream=>stream.Write16(null),stream=>stream.ReadString16Nullable(VERSION,new char[1]),1),
                new(-1,stream=>stream.Write32(null),stream=>stream.ReadString32Nullable(VERSION,new char[1]),1),
                // Bool
                new(true,stream=>stream.Write(true),stream=>stream.ReadBoolean(VERSION),1),
                new(false,stream=>stream.Write(false),stream=>stream.ReadBoolean(VERSION),1),
                new(null,stream=>stream.Write((bool?)null),stream=>stream.ReadBooleanNullable(VERSION),1),
                // Type
                new(typeof(string),stream=>stream.Write(typeof(string)),stream=>stream.ReadType(VERSION),null),
                new(null,stream=>stream.WriteNullable((Type?)null),stream=>stream.ReadTypeNullable(VERSION),1),
                // JSON
                new("test",stream=>stream.WriteJson("test"),stream=>stream.ReadJson<string>(VERSION, new byte[16]),null),
                new(null,stream=>stream.WriteJsonNullable<string>(null),stream=>stream.ReadJsonNullable<string>(VERSION, new byte[1]),1),
                // Enum
                new(OptInOut.OptOut,stream=>stream.Write(OptInOut.OptOut),stream=>stream.ReadEnum<OptInOut>(VERSION),1),
                new(null,stream=>stream.WriteNullable((OptInOut?)null),stream=>stream.ReadEnumNullable<OptInOut>(VERSION),1),
            })
            {
                try
                {
                    data.Item2(ms);
                    Assert.AreEqual(ms.Length, ms.Position, $"{data.Item1?.GetType().ToString() ?? "NULL"} at #{index}");
                    if (data.Item4.HasValue)
                        Assert.AreEqual(data.Item4.Value, ms.Length, $"{data.Item1?.GetType().ToString() ?? "NULL"} at #{index}");
                    ms.Position = 0;
                    obj = data.Item3(ms);
                    Assert.AreEqual(data.Item1, obj, $"{data.Item1?.GetType().ToString() ?? "NULL"} at #{index}");
                    Assert.AreEqual(ms.Length, ms.Position, $"{data.Item1?.GetType().ToString() ?? "NULL"} at #{index}");
                    ms.SetLength(0);
                    index++;
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"{data.Item1} at index #{index} exception: {ex}");
                    throw;
                }
            }
        }
    }
}

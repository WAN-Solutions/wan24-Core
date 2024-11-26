using System.Numerics;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class NumericTypes_Tests
    {
        private static readonly MethodInfoExt GetNumericTypeMethod;

        static NumericTypes_Tests()
        {
            GetNumericTypeMethod = typeof(NumericTypesExtensions)
                .GetMethodsCached()
                .Where(m => m.Name == nameof(NumericTypesExtensions.GetNumericType) && m.IsGenericMethod)
                .First();
        }

        [TestMethod]
        public void DefaultValue_Tests()
        {
            object defaultValue;
            foreach (Tuple<NumericTypes, object> data in new Tuple<NumericTypes, object>[]
            {
                new(NumericTypes.SByte, (sbyte)0),
                new(NumericTypes.Byte, (byte)0),
                new(NumericTypes.Short, (short)0),
                new(NumericTypes.UShort, (ushort)0),
                new(NumericTypes.Int, 0),
                new(NumericTypes.UInt, (uint)0),
                new(NumericTypes.Long, (long)0),
                new(NumericTypes.ULong, (ulong)0),
                new(NumericTypes.Int128, (Int128)0),
                new(NumericTypes.UInt128, (UInt128)0),
                new(NumericTypes.Half, (Half)0),
                new(NumericTypes.Float, (float)0),
                new(NumericTypes.Double, (double)0),
                new(NumericTypes.Decimal, (decimal)0),
                new(NumericTypes.BigInteger, (BigInteger)0)
            })
            {
                try
                {
                    defaultValue = data.Item1.GetDefault();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\"{ex.Message}\" when  getting the default for {data.Item1}");
                    throw;
                }
                Assert.AreEqual(data.Item2.GetType(), defaultValue.GetType(), data.Item1.ToString());
                Assert.IsTrue(defaultValue.Equals(data.Item2), data.Item1.ToString());
            }
            Assert.ThrowsException<ArgumentException>(() => NumericTypes.None.GetDefault());
        }

        [TestMethod]
        public void ClrType_Tests()
        {
            foreach (Tuple<NumericTypes, Type> data in new Tuple<NumericTypes, Type>[]
            {
                new(NumericTypes.SByte, typeof(sbyte)),
                new(NumericTypes.Byte, typeof(byte)),
                new(NumericTypes.Short, typeof(short)),
                new(NumericTypes.UShort, typeof(ushort)),
                new(NumericTypes.Int, typeof(int)),
                new(NumericTypes.UInt, typeof(uint)),
                new(NumericTypes.Long, typeof(long)),
                new(NumericTypes.ULong, typeof(ulong)),
                new(NumericTypes.Int128, typeof(Int128)),
                new(NumericTypes.UInt128, typeof(UInt128)),
                new(NumericTypes.Half, typeof(Half)),
                new(NumericTypes.Float, typeof(float)),
                new(NumericTypes.Double, typeof(double)),
                new(NumericTypes.Decimal, typeof(decimal)),
                new(NumericTypes.BigInteger, typeof(BigInteger))
            })
                try
                {
                    Assert.AreEqual(data.Item2, data.Item1.GetClrType(), data.Item1.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\"{ex.Message}\" when getting the CLR type of {data.Item1}");
                    throw;
                }
            Assert.AreEqual(typeof(void), NumericTypes.None.GetClrType());
        }

        [TestMethod]
        public void CastNumericValue_Tests()
        {
            object castedValue;
            foreach (Tuple<object, Type, NumericTypes> data in new Tuple<object, Type, NumericTypes>[]
            {
                new((sbyte)123, typeof(short), NumericTypes.Short),
                new((BigInteger)123, typeof(byte), NumericTypes.Byte),
                new(0, typeof(int), NumericTypes.Int)
            })
            {
                try
                {
                    castedValue = data.Item1.CastNumericValue(data.Item3);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\"{ex.Message}\" when casting the numeric value of {data.Item1} ({data.Item1.GetType()}) to {data.Item3}");
                    throw;
                }
                Assert.AreEqual(data.Item2, castedValue.GetType(), data.Item1.GetType().ToString());
            }
            Assert.ThrowsException<ArgumentException>(() => ((object)0).CastNumericValue(NumericTypes.None));

            Assert.AreEqual(typeof(short), ((sbyte)123).CastNumericValue(NumericTypes.Short).GetType());
            Assert.AreEqual(typeof(byte), ((BigInteger)123).CastNumericValue(NumericTypes.Byte).GetType());
            Assert.AreEqual(typeof(int), 0.CastNumericValue(NumericTypes.Int).GetType());
            Assert.ThrowsException<ArgumentException>(() => 0.CastNumericValue(NumericTypes.None));
        }

        [TestMethod]
        public void GetNumericType_Tests()
        {
            int index = 0;
            foreach (Tuple<object, NumericTypes> data in new Tuple<object, NumericTypes>[]
            {
                new((sbyte)0, NumericTypes.Zero),
                new((sbyte)1, NumericTypes.One),
                new((sbyte)-1, NumericTypes.MinusOne),
                new((sbyte)123, NumericTypes.DoublePi | NumericTypes.Number67To194),
                new(sbyte.MinValue, NumericTypes.SByteMin),
                new((sbyte)-2,NumericTypes.SByte),
                new((short)0, NumericTypes.Zero),
                new((short)1, NumericTypes.One),
                new((short)-1, NumericTypes.MinusOne),
                new((short)123, NumericTypes.DoublePi | NumericTypes.Number67To194),
                new(short.MinValue, NumericTypes.ShortMin),
                new(short.MaxValue, NumericTypes.ShortMax),
                new((short)-2, NumericTypes.SByte),
                new((short)200, NumericTypes.Byte),
                new(short.MinValue + 1, NumericTypes.Short),
                new(0, NumericTypes.Zero),
                new(1, NumericTypes.One),
                new(-1, NumericTypes.MinusOne),
                new(123, NumericTypes.DoublePi | NumericTypes.Number67To194),
                new(int.MinValue, NumericTypes.IntMin),
                new(int.MaxValue, NumericTypes.IntMax),
                new(-2, NumericTypes.SByte),
                new(200, NumericTypes.Byte),
                new(short.MinValue + 1, NumericTypes.Short),
                new(ushort.MaxValue - 1, NumericTypes.UShort),
                new(int.MinValue + 1, NumericTypes.Int),
                new((long)0, NumericTypes.Zero),
                new((long)1, NumericTypes.One),
                new((long)-1, NumericTypes.MinusOne),
                new((long)123, NumericTypes.DoublePi | NumericTypes.Number67To194),
                new(long.MinValue, NumericTypes.LongMin),
                new(long.MaxValue, NumericTypes.LongMax),
                new((long)-2, NumericTypes.SByte),
                new((long)200, NumericTypes.Byte),
                new((long)short.MinValue + 1, NumericTypes.Short),
                new((long)ushort.MaxValue - 1, NumericTypes.UShort),
                new((long)int.MinValue + 1, NumericTypes.Int),
                new((long)uint.MaxValue - 1, NumericTypes.UInt),
                new(long.MinValue + 1, NumericTypes.Long),
                new(Half.Zero, NumericTypes.Zero),
                new(Half.One, NumericTypes.One),
                new(Half.NegativeOne, NumericTypes.MinusOne),
                new((Half)123, NumericTypes.DoublePi | NumericTypes.Number67To194),
                new(Half.MinValue, NumericTypes.HalfMin),
                new(Half.MaxValue, NumericTypes.HalfMax),
                new(Half.E, NumericTypes.HalfE),
                new(Half.Epsilon, NumericTypes.HalfEpsilon),
                new(Half.NaN, NumericTypes.HalfNaN),
                new(Half.NegativeInfinity, NumericTypes.HalfNegativeInfinity),
                new(Half.Pi, NumericTypes.HalfPi),
                new(Half.PositiveInfinity, NumericTypes.HalfPositiveInfinity),
                new(Half.Tau, NumericTypes.HalfTau),
                new((Half)(-2), NumericTypes.SByte),
                new((Half)200, NumericTypes.Byte),
                new((Half)16000, NumericTypes.Short),
                new((Half)123.123f, NumericTypes.Half),
                new(0f, NumericTypes.Zero),
                new(1f, NumericTypes.One),
                new(-1f, NumericTypes.MinusOne),
                new(123f, NumericTypes.DoublePi | NumericTypes.Number67To194),
                new(float.MinValue, NumericTypes.FloatMin),
                new(float.MaxValue, NumericTypes.FloatMax),
                new(float.E, NumericTypes.FloatE),
                new(float.Epsilon, NumericTypes.FloatEpsilon),
                new(float.NaN, NumericTypes.FloatNaN),
                new(float.NegativeInfinity, NumericTypes.FloatNegativeInfinity),
                new(float.Pi, NumericTypes.FloatPi),
                new(float.PositiveInfinity, NumericTypes.FloatPositiveInfinity),
                new(float.Tau, NumericTypes.FloatTau),
                new(-2f, NumericTypes.SByte),
                new(200f, NumericTypes.Byte),
                new((float)short.MinValue + 1, NumericTypes.Short),
                new((float)ushort.MaxValue - 1, NumericTypes.UShort),
                new(1.123f, NumericTypes.Half),
                new((float)Half.MaxValue + 123.123f, NumericTypes.Float),
                new((float)Half.MinValue - 123.123f, NumericTypes.Float),
                new(0d, NumericTypes.Zero),
                new(1d, NumericTypes.One),
                new(-1d, NumericTypes.MinusOne),
                new(123d, NumericTypes.DoublePi | NumericTypes.Number67To194),
                new(double.MinValue, NumericTypes.DoubleMin),
                new(double.MaxValue, NumericTypes.DoubleMax),
                new(double.E, NumericTypes.DoubleE),
                new(double.Epsilon, NumericTypes.DoubleEpsilon),
                new(double.NaN, NumericTypes.DoubleNaN),
                new(double.NegativeInfinity, NumericTypes.DoubleNegativeInfinity),
                new(double.Pi, NumericTypes.DoublePi),
                new(double.PositiveInfinity, NumericTypes.DoublePositiveInfinity),
                new(double.Tau, NumericTypes.DoubleTau),
                new(-2d, NumericTypes.SByte),
                new(200d, NumericTypes.Byte),
                new((double)short.MinValue + 1, NumericTypes.Short),
                new((double)ushort.MaxValue - 1, NumericTypes.UShort),
                new((double)int.MinValue + 1, NumericTypes.Int),
                new((double)uint.MaxValue - 1, NumericTypes.UInt),
                new(0d-NumericTypesExtensions.MaxNonIntFloat, NumericTypes.Float),//FIXME Is double!?
                new(NumericTypesExtensions.MaxNonIntFloat, NumericTypes.Float),
                new(0d-NumericTypesExtensions.MaxNonIntDouble, NumericTypes.Double),
                new(NumericTypesExtensions.MaxNonIntDouble, NumericTypes.Double),
                new(0m, NumericTypes.Zero),
                new(1m, NumericTypes.One),
                new(-1m, NumericTypes.MinusOne),
                new(123m, NumericTypes.DoublePi | NumericTypes.Number67To194),
                new(decimal.MinValue, NumericTypes.DecimalMin),
                new(decimal.MaxValue, NumericTypes.DecimalMax),
                new(-2m, NumericTypes.SByte),
                new(200m, NumericTypes.Byte),
                new(256m, NumericTypes.Short),
                new(short.MinValue + 1m, NumericTypes.Short),
                new(ushort.MaxValue - 1m, NumericTypes.UShort),
                new(int.MinValue + 1m, NumericTypes.Int),
                new(uint.MaxValue - 1m, NumericTypes.UInt),
                new(long.MinValue + 1m, NumericTypes.Long),
                new(ulong.MaxValue - 1m, NumericTypes.ULong),
                new(decimal.MinValue + 1, NumericTypes.Decimal),
                new(decimal.MaxValue - 1, NumericTypes.Decimal),
                new(NumericTypesExtensions.MinNonIntDecimal, NumericTypes.Decimal),
                new(NumericTypesExtensions.MaxNonIntDecimal, NumericTypes.Decimal),
                new((byte)0, NumericTypes.Zero),
                new((byte)1, NumericTypes.One),
                new((byte)123, NumericTypes.DoublePi | NumericTypes.Number67To194),
                new(byte.MaxValue, NumericTypes.ByteMax),
                new((byte)254, NumericTypes.Byte),
                new((ushort)0, NumericTypes.Zero),
                new((ushort)1, NumericTypes.One),
                new((ushort)123, NumericTypes.DoublePi | NumericTypes.Number67To194),
                new(ushort.MaxValue, NumericTypes.UShortMax),
                new((ushort)254, NumericTypes.Byte),
                new((uint)0, NumericTypes.Zero),
                new((uint)1, NumericTypes.One),
                new((uint)123, NumericTypes.DoublePi | NumericTypes.Number67To194),
                new(uint.MaxValue, NumericTypes.UIntMax),
                new((uint)254, NumericTypes.Byte),
                new((uint)ushort.MaxValue - 1, NumericTypes.UShort),
                new((ulong)0, NumericTypes.Zero),
                new((ulong)1, NumericTypes.One),
                new((ulong)123, NumericTypes.DoublePi | NumericTypes.Number67To194),
                new(ulong.MaxValue, NumericTypes.ULongMax),
                new((ulong)254, NumericTypes.Byte),
                new((ulong)ushort.MaxValue - 1, NumericTypes.UShort),
                new((ulong)uint.MaxValue - 1, NumericTypes.UInt),
                new(BigInteger.Zero, NumericTypes.Zero),
                new(BigInteger.One, NumericTypes.One),
                new(BigInteger.MinusOne, NumericTypes.MinusOne),
                new((BigInteger)123, NumericTypes.DoublePi | NumericTypes.Number67To194),
                new((BigInteger)(-2), NumericTypes.SByte),
                new((BigInteger)200, NumericTypes.Byte),
                new((BigInteger)short.MinValue + 1, NumericTypes.Short),
                new((BigInteger)ushort.MaxValue - 1, NumericTypes.UShort),
                new((BigInteger)int.MinValue + 1, NumericTypes.Int),
                new((BigInteger)uint.MaxValue - 1, NumericTypes.UInt),
                new((BigInteger)long.MinValue + 1, NumericTypes.Long),
                new((BigInteger)ulong.MaxValue - 1, NumericTypes.ULong),
                new((BigInteger)ulong.MaxValue + 1, NumericTypes.BigInteger),
                new((BigInteger)long.MinValue - 1, NumericTypes.BigInteger),
                new(string.Empty, NumericTypes.None)
            })
                try
                {
                    Assert.AreEqual(data.Item2, data.Item1.GetNumericType(), $"Object {data.Item1} ({data.Item1.GetType()})");
                    if (data.Item1.GetType().IsNumeric())
                        Assert.AreEqual(data.Item2, (NumericTypes)GetNumericTypeMethod.MakeGenericMethod(data.Item1.GetType()).Invoker!(null, [data.Item1])!, $"Generic {data.Item1} ({data.Item1.GetType()})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\"{ex.Message}\" when getting the numeric type of {data.Item1} ({data.Item1.GetType()})");
                    throw;
                }
                finally
                {
                    index++;
                }
        }

        [TestMethod]
        public void IsUnsigned_Tests()
        {
            foreach (Tuple<NumericTypes, bool> data in new Tuple<NumericTypes, bool>[]
            {
                new(NumericTypes.SByte, false),
                new(NumericTypes.Byte, true),
                new(NumericTypes.Short, false),
                new(NumericTypes.UShort, true),
                new(NumericTypes.Int, false),
                new(NumericTypes.UInt, true),
                new(NumericTypes.Long, false),
                new(NumericTypes.ULong, true),
                new(NumericTypes.Int128, false),
                new(NumericTypes.UInt128, true),
                new(NumericTypes.Half, false),
                new(NumericTypes.Float, false),
                new(NumericTypes.Double, false),
                new(NumericTypes.Decimal, false),
                new(NumericTypes.BigInteger, false)
            })
                Assert.AreEqual(data.Item2, data.Item1.IsUnsigned(), data.Item1.ToString());
        }

        [TestMethod]
        public void IsFloatingPoint_Tests()
        {
            foreach (Tuple<NumericTypes, bool> data in new Tuple<NumericTypes, bool>[]
            {
                new(NumericTypes.SByte, false),
                new(NumericTypes.Byte, false),
                new(NumericTypes.Short, false),
                new(NumericTypes.UShort, false),
                new(NumericTypes.Int, false),
                new(NumericTypes.UInt, false),
                new(NumericTypes.Long, false),
                new(NumericTypes.ULong, false),
                new(NumericTypes.Int128, false),
                new(NumericTypes.UInt128, false),
                new(NumericTypes.Half, true),
                new(NumericTypes.Float, true),
                new(NumericTypes.Double, true),
                new(NumericTypes.Decimal, false),
                new(NumericTypes.BigInteger, false)
            })
                Assert.AreEqual(data.Item2, data.Item1.IsFloatingPoint(), data.Item1.ToString());
        }

        [TestMethod]
        public void IsDecimal_Tests()
        {
            foreach (Tuple<NumericTypes, bool> data in new Tuple<NumericTypes, bool>[]
            {
                new(NumericTypes.SByte, false),
                new(NumericTypes.Byte, false),
                new(NumericTypes.Short, false),
                new(NumericTypes.UShort, false),
                new(NumericTypes.Int, false),
                new(NumericTypes.UInt, false),
                new(NumericTypes.Long, false),
                new(NumericTypes.ULong, false),
                new(NumericTypes.Int128, false),
                new(NumericTypes.UInt128, false),
                new(NumericTypes.Half, false),
                new(NumericTypes.Float, false),
                new(NumericTypes.Double, false),
                new(NumericTypes.Decimal, true),
                new(NumericTypes.BigInteger, false)
            })
                Assert.AreEqual(data.Item2, data.Item1.IsDecimal(), data.Item1.ToString());
        }

        [TestMethod]
        public void IsBigInteger_Tests()
        {
            foreach (Tuple<NumericTypes, bool> data in new Tuple<NumericTypes, bool>[]
            {
                new(NumericTypes.SByte, false),
                new(NumericTypes.Byte, false),
                new(NumericTypes.Short, false),
                new(NumericTypes.UShort, false),
                new(NumericTypes.Int, false),
                new(NumericTypes.UInt, false),
                new(NumericTypes.Long, false),
                new(NumericTypes.ULong, false),
                new(NumericTypes.Int128, false),
                new(NumericTypes.UInt128, false),
                new(NumericTypes.Half, false),
                new(NumericTypes.Float, false),
                new(NumericTypes.Double, false),
                new(NumericTypes.Decimal, false),
                new(NumericTypes.BigInteger, true)
            })
                Assert.AreEqual(data.Item2, data.Item1.IsBigInteger(), data.Item1.ToString());
        }

        [TestMethod]
        public void GetNumericType_Clr_Tests()
        {
            foreach (Tuple<object, NumericTypes> data in new Tuple<object, NumericTypes>[]
            {
                new((sbyte)123, NumericTypes.SByte),
                new((byte)123, NumericTypes.Byte),
                new((short)123, NumericTypes.Short),
                new((ushort)123, NumericTypes.UShort),
                new(123, NumericTypes.Int),
                new((uint)123, NumericTypes.UInt),
                new((long)123, NumericTypes.Long),
                new((ulong)123, NumericTypes.ULong),
                new((Int128)123, NumericTypes.Int128),
                new((UInt128)123, NumericTypes.UInt128),
                new((Half)123, NumericTypes.Half),
                new((float)123, NumericTypes.Float),
                new((double)123, NumericTypes.Double),
                new((decimal)123, NumericTypes.Decimal),
                new((BigInteger)123, NumericTypes.BigInteger),
                new(string.Empty, NumericTypes.None)
            })
                Assert.AreEqual(data.Item2, data.Item1.GetType().GetNumericType(), data.Item2.ToString());
        }

        [TestMethod]
        public void CastType_Tests()
        {
            object castedValue;
            foreach (Tuple<object, NumericTypes, object> data in new Tuple<object, NumericTypes, object>[]
            {
                new((sbyte)123, NumericTypes.BigInteger, (BigInteger)123),
                new((byte)123, NumericTypes.BigInteger, (BigInteger)123),
                new((short)123, NumericTypes.BigInteger, (BigInteger)123),
                new((ushort)123, NumericTypes.BigInteger, (BigInteger)123),
                new(123, NumericTypes.BigInteger, (BigInteger)123),
                new((uint)123, NumericTypes.BigInteger, (BigInteger)123),
                new((long)123, NumericTypes.BigInteger, (BigInteger)123),
                new((ulong)123, NumericTypes.BigInteger, (BigInteger)123),
                new((Int128)123, NumericTypes.BigInteger, (BigInteger)123),
                new((UInt128)123, NumericTypes.BigInteger, (BigInteger)123),
                new((Int128)123, NumericTypes.BigInteger, (BigInteger)123),
                new((UInt128)123, NumericTypes.BigInteger, (BigInteger)123),
                new((Half)123, NumericTypes.BigInteger, (BigInteger)123),
                new((float)123, NumericTypes.BigInteger, (BigInteger)123),
                new((double)123, NumericTypes.BigInteger, (BigInteger)123),
                new((decimal)123, NumericTypes.BigInteger, (BigInteger)123),
                new((BigInteger)123, NumericTypes.SByte, (sbyte)123)
            })
            {
                try
                {
                    castedValue = data.Item1.CastType(data.Item2);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\"{ex.Message}\" when casting the type of {data.Item1.GetType()} to {data.Item2}");
                    throw;
                }
                Assert.AreEqual(data.Item2, castedValue.GetType().GetNumericType(), data.Item2.ToString());
                Assert.AreEqual(data.Item3.GetType(), castedValue.GetType(), data.Item2.ToString());
                Assert.AreEqual(data.Item3, castedValue, data.Item2.ToString());
            }
        }

        [TestMethod]
        public void GetDataStructureLength_Tests()
        {
            foreach (Tuple<NumericTypes, int> data in new Tuple<NumericTypes, int>[]
            {
                new(NumericTypes.None, 0),
                new(NumericTypes.One, 0),
                new(NumericTypes.SByte, 1),
                new(NumericTypes.Byte, 1),
                new(NumericTypes.Short, 2),
                new(NumericTypes.UShort, 2),
                new(NumericTypes.Int, 4),
                new(NumericTypes.UInt, 4),
                new(NumericTypes.Long, 8),
                new(NumericTypes.ULong, 8),
                new(NumericTypes.Int128, 16),
                new(NumericTypes.UInt128, 16),
                new(NumericTypes.Half, 2),
                new(NumericTypes.Float, 4),
                new(NumericTypes.Double, 8),
                new(NumericTypes.Decimal, 16)
            })
                Assert.AreEqual(data.Item2, data.Item1.GetDataStructureLength(), data.Item1.ToString());
            Assert.ThrowsException<ArgumentException>(() => NumericTypes.BigInteger.GetDataStructureLength());
        }

        [TestMethod]
        public void GetTypeGroup_Tests()
        {
            foreach (Tuple<NumericTypes, NumericTypes> data in new Tuple<NumericTypes, NumericTypes>[]
            {
                new(NumericTypes.Int, NumericTypes.Zero),
                new(NumericTypes.Int, NumericTypes.One),
                new(NumericTypes.Int, NumericTypes.MinusOne),
                new(NumericTypes.SByte, NumericTypes.SByte),
                new(NumericTypes.SByte, NumericTypes.SByteMin),
                new(NumericTypes.Byte, NumericTypes.Byte),
                new(NumericTypes.Byte, NumericTypes.ByteMax),
                new(NumericTypes.Short, NumericTypes.Short),
                new(NumericTypes.Short, NumericTypes.ShortMin),
                new(NumericTypes.Short, NumericTypes.ShortMax),
                new(NumericTypes.UShort, NumericTypes.UShort),
                new(NumericTypes.UShort, NumericTypes.UShortMax),
                new(NumericTypes.Int, NumericTypes.Int),
                new(NumericTypes.Int, NumericTypes.IntMin),
                new(NumericTypes.Int, NumericTypes.IntMax),
                new(NumericTypes.UInt, NumericTypes.UInt),
                new(NumericTypes.UInt, NumericTypes.UIntMax),
                new(NumericTypes.Long, NumericTypes.Long),
                new(NumericTypes.Long, NumericTypes.LongMin),
                new(NumericTypes.Long, NumericTypes.LongMax),
                new(NumericTypes.ULong, NumericTypes.ULong),
                new(NumericTypes.ULong, NumericTypes.ULongMax),
                new(NumericTypes.Int128, NumericTypes.Int128),
                new(NumericTypes.Int128, NumericTypes.Int128Min),
                new(NumericTypes.Int128, NumericTypes.Int128Max),
                new(NumericTypes.UInt128, NumericTypes.UInt128),
                new(NumericTypes.UInt128, NumericTypes.UInt128Max),
                new(NumericTypes.Half, NumericTypes.Half),
                new(NumericTypes.Half, NumericTypes.HalfMin),
                new(NumericTypes.Half, NumericTypes.HalfMax),
                new(NumericTypes.Half, NumericTypes.HalfE),
                new(NumericTypes.Half, NumericTypes.HalfEpsilon),
                new(NumericTypes.Half, NumericTypes.HalfNaN),
                new(NumericTypes.Half, NumericTypes.HalfNegativeInfinity),
                new(NumericTypes.Half, NumericTypes.HalfPi),
                new(NumericTypes.Half, NumericTypes.HalfPositiveInfinity),
                new(NumericTypes.Half, NumericTypes.HalfTau),
                new(NumericTypes.Float, NumericTypes.Float),
                new(NumericTypes.Float, NumericTypes.FloatMin),
                new(NumericTypes.Float, NumericTypes.FloatMax),
                new(NumericTypes.Float, NumericTypes.FloatE),
                new(NumericTypes.Float, NumericTypes.FloatEpsilon),
                new(NumericTypes.Float, NumericTypes.FloatNaN),
                new(NumericTypes.Float, NumericTypes.FloatNegativeInfinity),
                new(NumericTypes.Float, NumericTypes.FloatPi),
                new(NumericTypes.Float, NumericTypes.FloatPositiveInfinity),
                new(NumericTypes.Float, NumericTypes.FloatTau),
                new(NumericTypes.Double, NumericTypes.Double),
                new(NumericTypes.Double, NumericTypes.DoubleMin),
                new(NumericTypes.Double, NumericTypes.DoubleMax),
                new(NumericTypes.Double, NumericTypes.DoubleE),
                new(NumericTypes.Double, NumericTypes.DoubleEpsilon),
                new(NumericTypes.Double, NumericTypes.DoubleNaN),
                new(NumericTypes.Double, NumericTypes.DoubleNegativeInfinity),
                new(NumericTypes.Double, NumericTypes.DoublePi),
                new(NumericTypes.Double, NumericTypes.DoublePositiveInfinity),
                new(NumericTypes.Double, NumericTypes.DoubleTau),
                new(NumericTypes.Decimal, NumericTypes.Decimal),
                new(NumericTypes.Decimal, NumericTypes.DecimalMin),
                new(NumericTypes.Decimal, NumericTypes.DecimalMax),
                new(NumericTypes.BigInteger, NumericTypes.BigInteger),
                new(NumericTypes.Int, NumericTypes.Number2)
            })
                Assert.AreEqual(data.Item1, data.Item2.GetTypeGroup(), data.Item2.ToString());
        }

        [TestMethod]
        public void HasValue_Tests()
        {
            Assert.IsFalse(NumericTypes.None.HasValue());
            Assert.IsTrue(NumericTypes.Zero.HasValue());
            Assert.IsFalse(NumericTypes.SByte.HasValue());
            Assert.IsTrue((NumericTypes.SByte | NumericTypes.Number67To194).HasValue());
            Assert.IsTrue(NumericTypes.Number67To194.HasValue());
            for (int i = NumericTypesExtensions.MIN_VALUE; i <= NumericTypesExtensions.MAX_VALUE; i++)
                Assert.IsTrue(i.GetValueNumericType().HasValue());
        }

        [TestMethod]
        public void GetValue_Tests()
        {
            foreach (Tuple<NumericTypes, object> data in new Tuple<NumericTypes, object>[]
            {
                new(NumericTypes.Zero, 0),
                new(NumericTypes.One, 1),
                new(NumericTypes.MinusOne, -1),
                new(NumericTypes.Number2, 2),
                new(NumericTypes.Number67To194, 67),
                new(NumericTypes.Number67To194 | NumericTypes.Zero, 68)
            })
                Assert.AreEqual(data.Item2, data.Item1.GetValue(), data.Item1.ToString());
            Assert.ThrowsException<ArgumentException>(() => NumericTypes.None.GetValue());
            for (int i = NumericTypesExtensions.MIN_VALUE; i <= NumericTypesExtensions.MAX_VALUE; i++)
                Assert.AreEqual(i, i.GetValueNumericType().GetValue(), i.ToString());
        }

        [TestMethod]
        public void IsValueNumericType_Tests()
        {
            Assert.IsTrue(0.IsValueNumericType());
            Assert.IsTrue(1.IsValueNumericType());
            Assert.IsTrue(NumericTypesExtensions.MIN_VALUE.IsValueNumericType());
            Assert.IsFalse((NumericTypesExtensions.MIN_VALUE - 1).IsValueNumericType());
            Assert.IsTrue(NumericTypesExtensions.MAX_VALUE.IsValueNumericType());
            Assert.IsFalse((NumericTypesExtensions.MAX_VALUE + 1).IsValueNumericType());
        }

        [TestMethod]
        public void GetValueNumericType_Tests()
        {
            Assert.AreEqual(NumericTypes.Zero, 0.GetValueNumericType());
            Assert.AreEqual(NumericTypes.One, 1.GetValueNumericType());
            Assert.AreEqual(NumericTypes.MinusOne, (-1).GetValueNumericType());
            Assert.AreEqual(NumericTypes.Number2, 2.GetValueNumericType());
            Assert.AreEqual(NumericTypes.Number67To194, (128 - (int)NumericTypes.Number2 + 2).GetValueNumericType());
            Assert.AreEqual(NumericTypes.Number66 | NumericTypes.Number67To194, NumericTypesExtensions.MAX_VALUE.GetValueNumericType());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => 200.GetValueNumericType());
        }
    }
}

using System.Numerics;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class NumericTypes_Tests
    {
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
            //TODO Overwork tests
            foreach (Tuple<object, NumericTypes> data in new Tuple<object, NumericTypes>[]
            {
                new((sbyte)0, NumericTypes.Zero),
                new((sbyte)1, NumericTypes.One),
                new((sbyte)-1, NumericTypes.MinusOne),
                new((sbyte)123, NumericTypes.DoublePi | NumericTypes.Number71To199),
                new(sbyte.MinValue, NumericTypes.SByteMin),
                new((sbyte)-2,NumericTypes.SByte),
                new((short)0, NumericTypes.Zero),
                new((short)1, NumericTypes.One),
                new((short)-1, NumericTypes.MinusOne),
                new((short)123, NumericTypes.DoublePi | NumericTypes.Number71To199),
                new(short.MinValue, NumericTypes.ShortMin),
                new(short.MaxValue, NumericTypes.ShortMax),
                new((short)-2, NumericTypes.Short),
                new(0, NumericTypes.Zero),
                new(1, NumericTypes.One),
                new(-1, NumericTypes.MinusOne),
                new(123, NumericTypes.DoublePi | NumericTypes.Number71To199),
                new(int.MinValue, NumericTypes.IntMin),
                new(int.MaxValue, NumericTypes.IntMax),
                new(-2, NumericTypes.Int),
                new((long)0, NumericTypes.Zero),
                new((long)1, NumericTypes.One),
                new((long)-1, NumericTypes.MinusOne),
                new((long)123, NumericTypes.DoublePi | NumericTypes.Number71To199),
                new(long.MinValue, NumericTypes.LongMin),
                new(long.MaxValue, NumericTypes.LongMax),
                new((long)-2, NumericTypes.Long),
                new(Half.Zero, NumericTypes.Zero),
                new(Half.One, NumericTypes.One),
                new(Half.NegativeOne, NumericTypes.MinusOne),
                new((Half)123, NumericTypes.DoublePi | NumericTypes.Number71To199),
                new(Half.MinValue, NumericTypes.HalfMin),
                new(Half.MaxValue, NumericTypes.HalfMax),
                new(Half.E, NumericTypes.HalfE),
                new(Half.Epsilon, NumericTypes.HalfEpsilon),
                new(Half.NaN, NumericTypes.HalfNaN),
                new(Half.NegativeInfinity, NumericTypes.HalfNegativeInfinity),
                new(Half.Pi, NumericTypes.HalfPi),
                new(Half.PositiveInfinity, NumericTypes.HalfPositiveInfinity),
                new(Half.Tau, NumericTypes.HalfTau),
                new((Half)(-2), NumericTypes.Half),
                new(0f, NumericTypes.Zero),
                new(1f, NumericTypes.One),
                new(-1f, NumericTypes.MinusOne),
                new(123f, NumericTypes.DoublePi | NumericTypes.Number71To199),
                new(float.MinValue, NumericTypes.FloatMin),
                new(float.MaxValue, NumericTypes.FloatMax),
                new(float.E, NumericTypes.FloatE),
                new(float.Epsilon, NumericTypes.FloatEpsilon),
                new(float.NaN, NumericTypes.FloatNaN),
                new(float.NegativeInfinity, NumericTypes.FloatNegativeInfinity),
                new(float.Pi, NumericTypes.FloatPi),
                new(float.PositiveInfinity, NumericTypes.FloatPositiveInfinity),
                new(float.Tau, NumericTypes.FloatTau),
                new(-2f, NumericTypes.Float),
                new(0d, NumericTypes.Zero),
                new(1d, NumericTypes.One),
                new(-1d, NumericTypes.MinusOne),
                new(123d, NumericTypes.DoublePi | NumericTypes.Number71To199),
                new(double.MinValue, NumericTypes.DoubleMin),
                new(double.MaxValue, NumericTypes.DoubleMax),
                new(double.E, NumericTypes.DoubleE),
                new(double.Epsilon, NumericTypes.DoubleEpsilon),
                new(double.NaN, NumericTypes.DoubleNaN),
                new(double.NegativeInfinity, NumericTypes.DoubleNegativeInfinity),
                new(double.Pi, NumericTypes.DoublePi),
                new(double.PositiveInfinity, NumericTypes.DoublePositiveInfinity),
                new(double.Tau, NumericTypes.DoubleTau),
                new(-2d, NumericTypes.Double),
                new(0m, NumericTypes.Zero),
                new(1m, NumericTypes.One),
                new(-1m, NumericTypes.MinusOne),
                new(123m, NumericTypes.DoublePi | NumericTypes.Number71To199),
                new(decimal.MinValue, NumericTypes.DecimalMin),
                new(decimal.MaxValue, NumericTypes.DecimalMax),
                new(-2m, NumericTypes.Decimal),
                new((byte)0, NumericTypes.Zero),
                new((byte)1, NumericTypes.One),
                new((byte)123, NumericTypes.DoublePi | NumericTypes.Number71To199),
                new(byte.MaxValue, NumericTypes.ByteMax),
                new((byte)254, NumericTypes.Byte),
                new((ushort)0, NumericTypes.Zero),
                new((ushort)1, NumericTypes.One),
                new((ushort)123, NumericTypes.DoublePi | NumericTypes.Number71To199),
                new(ushort.MaxValue, NumericTypes.UShortMax),
                new((ushort)254, NumericTypes.UShort),
                new((uint)0, NumericTypes.Zero),
                new((uint)1, NumericTypes.One),
                new((uint)123, NumericTypes.DoublePi | NumericTypes.Number71To199),
                new(uint.MaxValue, NumericTypes.UIntMax),
                new((uint)254, NumericTypes.UInt),
                new((ulong)0, NumericTypes.Zero),
                new((ulong)1, NumericTypes.One),
                new((ulong)123, NumericTypes.DoublePi | NumericTypes.Number71To199),
                new(ulong.MaxValue, NumericTypes.ULongMax),
                new((ulong)254, NumericTypes.ULong),
                new(BigInteger.Zero, NumericTypes.Zero),
                new(BigInteger.One, NumericTypes.One),
                new(BigInteger.MinusOne, NumericTypes.MinusOne),
                new((BigInteger)123, NumericTypes.DoublePi | NumericTypes.Number71To199),
                new((BigInteger)254, NumericTypes.BigInteger),
                new(string.Empty, NumericTypes.None)
            })
                try
                {
                    Assert.AreEqual(data.Item2, data.Item1.GetNumericType());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\"{ex.Message}\" when getting the numeric type of {data.Item1} ({data.Item1.GetType()})");
                    throw;
                }

            Assert.AreEqual(NumericTypes.Zero, ((sbyte)0).GetNumericType());
            Assert.AreEqual(NumericTypes.One, ((sbyte)1).GetNumericType());
            Assert.AreEqual(NumericTypes.MinusOne, ((sbyte)-1).GetNumericType());
            Assert.AreEqual(NumericTypes.DoublePi | NumericTypes.Number71To199, ((sbyte)123).GetNumericType());
            Assert.AreEqual(NumericTypes.SByteMin, sbyte.MinValue.GetNumericType());
            Assert.AreEqual(NumericTypes.SByte, ((sbyte)-2).GetNumericType());
            Assert.AreEqual(NumericTypes.Zero, ((short)0).GetNumericType());
            Assert.AreEqual(NumericTypes.One, ((short)1).GetNumericType());
            Assert.AreEqual(NumericTypes.MinusOne, ((short)-1).GetNumericType());
            Assert.AreEqual(NumericTypes.DoublePi | NumericTypes.Number71To199, ((short)123).GetNumericType());
            Assert.AreEqual(NumericTypes.ShortMin, short.MinValue.GetNumericType());
            Assert.AreEqual(NumericTypes.ShortMax, short.MaxValue.GetNumericType());
            Assert.AreEqual(NumericTypes.Short, ((short)-2).GetNumericType());
            Assert.AreEqual(NumericTypes.Zero, 0.GetNumericType());
            Assert.AreEqual(NumericTypes.One, 1.GetNumericType());
            Assert.AreEqual(NumericTypes.MinusOne, (-1).GetNumericType());
            Assert.AreEqual(NumericTypes.DoublePi | NumericTypes.Number71To199, 123.GetNumericType());
            Assert.AreEqual(NumericTypes.IntMin, int.MinValue.GetNumericType());
            Assert.AreEqual(NumericTypes.IntMax, int.MaxValue.GetNumericType());
            Assert.AreEqual(NumericTypes.Int, (-2).GetNumericType());
            Assert.AreEqual(NumericTypes.Zero, ((long)0).GetNumericType());
            Assert.AreEqual(NumericTypes.One, ((long)1).GetNumericType());
            Assert.AreEqual(NumericTypes.MinusOne, ((long)-1).GetNumericType());
            Assert.AreEqual(NumericTypes.DoublePi | NumericTypes.Number71To199, ((long)123).GetNumericType());
            Assert.AreEqual(NumericTypes.LongMin, long.MinValue.GetNumericType());
            Assert.AreEqual(NumericTypes.LongMax, long.MaxValue.GetNumericType());
            Assert.AreEqual(NumericTypes.Long, ((long)-2).GetNumericType());
            Assert.AreEqual(NumericTypes.Zero, Half.Zero.GetNumericType());
            Assert.AreEqual(NumericTypes.One, Half.One.GetNumericType());
            Assert.AreEqual(NumericTypes.MinusOne, Half.NegativeOne.GetNumericType());
            Assert.AreEqual(NumericTypes.DoublePi | NumericTypes.Number71To199, ((Half)123).GetNumericType());
            Assert.AreEqual(NumericTypes.HalfMin, Half.MinValue.GetNumericType());
            Assert.AreEqual(NumericTypes.HalfMax, Half.MaxValue.GetNumericType());
            Assert.AreEqual(NumericTypes.HalfE, Half.E.GetNumericType());
            Assert.AreEqual(NumericTypes.HalfEpsilon, Half.Epsilon.GetNumericType());
            Assert.AreEqual(NumericTypes.HalfNaN, Half.NaN.GetNumericType());
            Assert.AreEqual(NumericTypes.HalfNegativeInfinity, Half.NegativeInfinity.GetNumericType());
            Assert.AreEqual(NumericTypes.HalfPi, Half.Pi.GetNumericType());
            Assert.AreEqual(NumericTypes.HalfPositiveInfinity, Half.PositiveInfinity.GetNumericType());
            Assert.AreEqual(NumericTypes.HalfTau, Half.Tau.GetNumericType());
            Assert.AreEqual(NumericTypes.Half, ((Half)(-2)).GetNumericType());
            Assert.AreEqual(NumericTypes.Zero, 0f.GetNumericType());
            Assert.AreEqual(NumericTypes.One, 1f.GetNumericType());
            Assert.AreEqual(NumericTypes.MinusOne, (-1f).GetNumericType());
            Assert.AreEqual(NumericTypes.DoublePi | NumericTypes.Number71To199, 123f.GetNumericType());
            Assert.AreEqual(NumericTypes.FloatMin, float.MinValue.GetNumericType());
            Assert.AreEqual(NumericTypes.FloatMax, float.MaxValue.GetNumericType());
            Assert.AreEqual(NumericTypes.FloatE, float.E.GetNumericType());
            Assert.AreEqual(NumericTypes.FloatEpsilon, float.Epsilon.GetNumericType());
            Assert.AreEqual(NumericTypes.FloatNaN, float.NaN.GetNumericType());
            Assert.AreEqual(NumericTypes.FloatNegativeInfinity, float.NegativeInfinity.GetNumericType());
            Assert.AreEqual(NumericTypes.FloatPi, float.Pi.GetNumericType());
            Assert.AreEqual(NumericTypes.FloatPositiveInfinity, float.PositiveInfinity.GetNumericType());
            Assert.AreEqual(NumericTypes.FloatTau, float.Tau.GetNumericType());
            Assert.AreEqual(NumericTypes.Float, (-2f).GetNumericType());
            Assert.AreEqual(NumericTypes.Zero, 0d.GetNumericType());
            Assert.AreEqual(NumericTypes.One, 1d.GetNumericType());
            Assert.AreEqual(NumericTypes.MinusOne, (-1d).GetNumericType());
            Assert.AreEqual(NumericTypes.DoublePi | NumericTypes.Number71To199, 123d.GetNumericType());
            Assert.AreEqual(NumericTypes.DoubleMin, double.MinValue.GetNumericType());
            Assert.AreEqual(NumericTypes.DoubleMax, double.MaxValue.GetNumericType());
            Assert.AreEqual(NumericTypes.DoubleE, double.E.GetNumericType());
            Assert.AreEqual(NumericTypes.DoubleEpsilon, double.Epsilon.GetNumericType());
            Assert.AreEqual(NumericTypes.DoubleNaN, double.NaN.GetNumericType());
            Assert.AreEqual(NumericTypes.DoubleNegativeInfinity, double.NegativeInfinity.GetNumericType());
            Assert.AreEqual(NumericTypes.DoublePi, double.Pi.GetNumericType());
            Assert.AreEqual(NumericTypes.DoublePositiveInfinity, double.PositiveInfinity.GetNumericType());
            Assert.AreEqual(NumericTypes.DoubleTau, double.Tau.GetNumericType());
            Assert.AreEqual(NumericTypes.Double, (-2d).GetNumericType());
            Assert.AreEqual(NumericTypes.Zero, ((byte)0).GetNumericType());
            Assert.AreEqual(NumericTypes.One, ((byte)1).GetNumericType());
            Assert.AreEqual(NumericTypes.DoublePi | NumericTypes.Number71To199, ((byte)123).GetNumericType());
            Assert.AreEqual(NumericTypes.ByteMax, byte.MaxValue.GetNumericType());
            Assert.AreEqual(NumericTypes.Byte, ((byte)254).GetNumericType());
            Assert.AreEqual(NumericTypes.Zero, ((ushort)0).GetNumericType());
            Assert.AreEqual(NumericTypes.One, ((ushort)1).GetNumericType());
            Assert.AreEqual(NumericTypes.DoublePi | NumericTypes.Number71To199, ((ushort)123).GetNumericType());
            Assert.AreEqual(NumericTypes.UShortMax, ushort.MaxValue.GetNumericType());
            Assert.AreEqual(NumericTypes.UShort, ((ushort)254).GetNumericType());
            Assert.AreEqual(NumericTypes.Zero, ((uint)0).GetNumericType());
            Assert.AreEqual(NumericTypes.One, ((uint)1).GetNumericType());
            Assert.AreEqual(NumericTypes.DoublePi | NumericTypes.Number71To199, ((uint)123).GetNumericType());
            Assert.AreEqual(NumericTypes.UIntMax, uint.MaxValue.GetNumericType());
            Assert.AreEqual(NumericTypes.UInt, ((uint)254).GetNumericType());
            Assert.AreEqual(NumericTypes.Zero, ((ulong)0).GetNumericType());
            Assert.AreEqual(NumericTypes.One, ((ulong)1).GetNumericType());
            Assert.AreEqual(NumericTypes.DoublePi | NumericTypes.Number71To199, ((ulong)123).GetNumericType());
            Assert.AreEqual(NumericTypes.ULongMax, ulong.MaxValue.GetNumericType());
            Assert.AreEqual(NumericTypes.ULong, ((ulong)254).GetNumericType());
            Assert.AreEqual(NumericTypes.Zero, BigInteger.Zero.GetNumericType());
            Assert.AreEqual(NumericTypes.One, BigInteger.One.GetNumericType());
            Assert.AreEqual(NumericTypes.MinusOne, BigInteger.MinusOne.GetNumericType());
            Assert.AreEqual(NumericTypes.DoublePi | NumericTypes.Number71To199, ((BigInteger)123).GetNumericType());
            Assert.AreEqual(NumericTypes.BigInteger, ((BigInteger)254).GetNumericType());
            Assert.AreEqual(NumericTypes.None, string.Empty.GetNumericType());
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
                new(NumericTypes.Half, NumericTypes.Half),
                new(NumericTypes.Half, NumericTypes.HalfMin),
                new(NumericTypes.Half, NumericTypes.HalfMax),
                new(NumericTypes.Half, NumericTypes.HalfE),
                new(NumericTypes.Half, NumericTypes.HalfEpsilon),
                new(NumericTypes.Half, NumericTypes.HalfNaN),
                new(NumericTypes.Half, NumericTypes.HalfNegativeInfinity),
                new(NumericTypes.Half, NumericTypes.HalfNegativeZero),
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
            Assert.IsTrue((NumericTypes.SByte | NumericTypes.Number71To199).HasValue());
            Assert.IsTrue(NumericTypes.Number71To199.HasValue());
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
                new(NumericTypes.Number71To199, 71),
                new(NumericTypes.Number71To199|NumericTypes.Zero, 72)
            })
                Assert.AreEqual(data.Item2, data.Item1.GetValue(), data.Item1.ToString());
            Assert.ThrowsException<ArgumentException>(() => NumericTypes.None.GetValue());
        }

        [TestMethod]
        public void IsValueNumericType_Tests()
        {
            Assert.IsTrue(0.IsValueNumericType());
            Assert.IsTrue(1.IsValueNumericType());
            Assert.IsTrue((-1).IsValueNumericType());
            Assert.IsFalse((-2).IsValueNumericType());
            Assert.IsTrue(199.IsValueNumericType());
            Assert.IsFalse(200.IsValueNumericType());
        }

        [TestMethod]
        public void GetValueNumericType_Tests()
        {
            Assert.AreEqual(NumericTypes.Zero, 0.GetValueNumericType());
            Assert.AreEqual(NumericTypes.One, 1.GetValueNumericType());
            Assert.AreEqual(NumericTypes.MinusOne, (-1).GetValueNumericType());
            Assert.AreEqual(NumericTypes.Number2, 2.GetValueNumericType());
            Assert.AreEqual(NumericTypes.Number71To199, 199.GetValueNumericType());
            Assert.AreEqual(NumericTypes.Number70 | NumericTypes.Number71To199, 198.GetValueNumericType());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => 200.GetValueNumericType());
        }
    }
}

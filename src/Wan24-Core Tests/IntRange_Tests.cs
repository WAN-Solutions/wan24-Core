using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class IntRange_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            {
                Assert.AreEqual(1, IntRange.Zero.Count);
                Assert.AreEqual(0, IntRange.Zero.From);
                Assert.AreEqual(0, IntRange.Zero.To);
                Assert.AreEqual(0, IntRange.Zero[0]);
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => IntRange.Zero[1]);
                Assert.AreEqual(1, IntRange.Zero.EnumerableRange.Count());
                Assert.AreEqual(0, IntRange.Zero.EnumerableRange.First());
                Range range = IntRange.Zero.AsRange;
                Assert.AreEqual(0, range.Start.Value);
                Assert.AreEqual(1, range.End.Value);
                int[] arr = IntRange.Zero.ToArray();
                Assert.AreEqual(1, arr.Length);
                Assert.AreEqual(0, arr[0]);
                Assert.AreEqual(1, IntRange.Zero.ToArray(arr));
                Assert.AreEqual(0, arr[0]);
                arr = IntRange.Zero.AsEnumerable().ToArray();
                Assert.AreEqual(1, arr.Length);
                Assert.AreEqual(0, arr[0]);
                byte[] data = IntRange.Zero.GetBytes();
                IntRange.Zero.GetBytes(data);
                IntRange zero = new(data);
                Assert.AreEqual(IntRange.Zero, zero);
                string str = zero.ToString();
                zero = IntRange.Parse(str);
                Assert.AreEqual(IntRange.Zero, zero);
                Assert.IsTrue(IntRange.TryParse(str, out zero));
                Assert.AreEqual(IntRange.Zero, zero);
                zero = new(0, 0);
                Assert.AreEqual(IntRange.Zero, zero);
                zero = new();
                Assert.AreEqual(IntRange.Zero, zero);
                Assert.AreNotEqual(IntRange.Zero, IntRange.MaxValue);
            }
            {
                Assert.AreEqual(1L + uint.MaxValue, IntRange.MaxValue.Count);
                Assert.AreEqual(int.MinValue, IntRange.MaxValue.From);
                Assert.AreEqual(int.MaxValue, IntRange.MaxValue.To);
                Assert.AreEqual(int.MinValue, IntRange.MaxValue[0]);
                Assert.AreEqual(0, IntRange.MaxValue[1L + int.MaxValue]);
                Assert.AreEqual(int.MaxValue, IntRange.MaxValue[uint.MaxValue]);
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => IntRange.MaxValue[1L + uint.MaxValue]);
                int[] arr = IntRange.MaxValue.ToArray((long)int.MaxValue + 1, 1);
                Assert.AreEqual(1, arr.Length);
                Assert.AreEqual(0, arr[0]);
                Assert.AreEqual(1, IntRange.MaxValue.ToArray(arr, (long)int.MaxValue + 1));
                Assert.AreEqual(0, arr[0]);
                arr = IntRange.MaxValue.AsEnumerable().Take(1).ToArray();
                Assert.AreEqual(1, arr.Length);
                Assert.AreEqual(int.MinValue, arr[0]);
                byte[] data = IntRange.MaxValue.GetBytes();
                IntRange.MaxValue.GetBytes(data);
                IntRange max = new(data);
                Assert.AreEqual(IntRange.MaxValue, max);
                string str = max.ToString();
                max = IntRange.Parse(str);
                Assert.AreEqual(IntRange.MaxValue, max);
                Assert.IsTrue(IntRange.TryParse(str, out max));
                Assert.AreEqual(IntRange.MaxValue, max);
                max = new(int.MinValue, int.MaxValue);
                Assert.AreEqual(IntRange.MaxValue, max);
                Assert.AreNotEqual(IntRange.MaxValue, IntRange.Zero);
            }
        }

        [TestMethod]
        public void Count_Tests()
        {
            Assert.AreEqual(3, new IntRange(-3, -1).Count);
            Assert.AreEqual(3, new IntRange(-1, 1).Count);
            Assert.AreEqual(3, new IntRange(0, 2).Count);
            Assert.AreEqual(3, new IntRange(1, 3).Count);
        }

        [TestMethod]
        public void Enumerable_Tests()
        {
            int[] arr = [.. new IntRange(-3, -1)];
            Assert.AreEqual(3, arr.Length);
            Assert.AreEqual(-3, arr[0]);
            Assert.AreEqual(-2, arr[1]);
            Assert.AreEqual(-1, arr[2]);

            arr = [.. new IntRange(-1, 1)];
            Assert.AreEqual(3, arr.Length);
            Assert.AreEqual(-1, arr[0]);
            Assert.AreEqual(0, arr[1]);
            Assert.AreEqual(1, arr[2]);

            arr = [.. new IntRange(0, 2)];
            Assert.AreEqual(3, arr.Length);
            Assert.AreEqual(0, arr[0]);
            Assert.AreEqual(1, arr[1]);
            Assert.AreEqual(2, arr[2]);

            arr = [.. new IntRange(1, 3)];
            Assert.AreEqual(3, arr.Length);
            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(2, arr[1]);
            Assert.AreEqual(3, arr[2]);
        }

        [TestMethod]
        public void DoesFit_Tests()
        {
            IntRange a = new(1, 3),
                b = new(0, 2),
                c = new(1, 4),
                d = new(4, 4),
                e = new(1, 2),
                f = new(2, 3);
            Assert.IsTrue(a.DoesFit(a));
            Assert.IsFalse(a.DoesFit(b));
            Assert.IsFalse(a.DoesFit(c));
            Assert.IsFalse(a.DoesFit(d));
            Assert.IsTrue(a.DoesFit(e));
            Assert.IsTrue(a.DoesFit(f));
        }

        [TestMethod]
        public void DoesIntersect_Tests()
        {
            IntRange a = new(1, 3),
                b = new(0, 2),
                c = new(1, 4),
                d = new(4, 4),
                e = new(1, 2),
                f = new(2, 3);
            Assert.IsTrue(a.DoesIntersect(a));
            Assert.IsTrue(a.DoesIntersect(b));
            Assert.IsTrue(a.DoesIntersect(c));
            Assert.IsFalse(a.DoesIntersect(d));
            Assert.IsTrue(a.DoesIntersect(e));
            Assert.IsTrue(a.DoesIntersect(f));
        }

        [TestMethod]
        public void ToArray_Tests()
        {
            IntRange range = new(1, 3);
            int[] arr = range.ToArray();
            Assert.AreEqual(3, arr.Length);
            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(2, arr[1]);
            Assert.AreEqual(3, arr[2]);

            arr = range.ToArray(start: 1);
            Assert.AreEqual(2, arr.Length);
            Assert.AreEqual(2, arr[0]);
            Assert.AreEqual(3, arr[1]);

            arr = range.ToArray(count: 2);
            Assert.AreEqual(2, arr.Length);
            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(2, arr[1]);

            arr = range.ToArray(step: 2);
            Assert.AreEqual(2, arr.Length);
            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(3, arr[1]);

            arr = new int[3];
            Assert.AreEqual(3, range.ToArray(arr));
            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(2, arr[1]);
            Assert.AreEqual(3, arr[2]);

            Assert.AreEqual(2, range.ToArray(arr, start: 1));
            Assert.AreEqual(2, arr[0]);
            Assert.AreEqual(3, arr[1]);

            Assert.AreEqual(2, range.ToArray(arr, step: 2));
            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(3, arr[1]);
        }

        [TestMethod]
        public void AsEnumerable_Tests()
        {
            IntRange range = new(1, 3);
            int[] arr = range.AsEnumerable().ToArray();
            Assert.AreEqual(3, arr.Length);
            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(2, arr[1]);
            Assert.AreEqual(3, arr[2]);

            arr = range.AsEnumerable(start: 1).ToArray();
            Assert.AreEqual(2, arr.Length);
            Assert.AreEqual(2, arr[0]);
            Assert.AreEqual(3, arr[1]);

            arr = range.AsEnumerable(count: 2).ToArray();
            Assert.AreEqual(2, arr.Length);
            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(2, arr[1]);

            arr = range.AsEnumerable(step: 2).ToArray();
            Assert.AreEqual(2, arr.Length);
            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(3, arr[1]);
        }

        [TestMethod]
        public void Operators_Tests()
        {
            Assert.IsTrue(IntRange.Zero < IntRange.MaxValue);
            Assert.IsTrue(IntRange.MaxValue > IntRange.Zero);
            Assert.IsTrue(IntRange.Zero <= IntRange.MaxValue);
            Assert.IsTrue(IntRange.Zero <= IntRange.Zero);
            Assert.IsTrue(IntRange.MaxValue >= IntRange.Zero);
            Assert.IsTrue(IntRange.MaxValue >= IntRange.MaxValue);
            IntRange range = IntRange.Zero >> 1;
            Assert.AreEqual(1, range.From);
            Assert.AreEqual(1, range.To);
            range = IntRange.Zero << 1;
            Assert.AreEqual(-1, range.From);
            Assert.AreEqual(-1, range.To);
        }

        [TestMethod]
        public void Parse_Tests()
        {
            string str = IntRange.Zero.ToString();
            Assert.AreEqual(IntRange.Zero, IntRange.Parse(str));
            Assert.ThrowsException<FormatException>(() => IntRange.Parse(string.Empty));
            Assert.IsTrue(IntRange.TryParse(str, out _));
            Assert.IsFalse(IntRange.TryParse(string.Empty, out _));
        }
    }
}

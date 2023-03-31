using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ArrayExtensions_Tests
    {
        [TestMethod]
        public void SpanValidation_Tests()
        {
            bool[] test = new bool[20];
            Assert.IsTrue(test.AsSpan().IsValid(0, test.Length));
            Assert.IsTrue(test.AsSpan().IsValid(10, test.Length - 10));
            Assert.IsFalse(test.AsSpan().IsValid(0, test.Length + 1));
            Assert.IsFalse(test.AsSpan().IsValid(0, -1));
            Assert.IsFalse(test.AsSpan().IsValid(-1, test.Length));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => test.AsSpan().EnsureValid(0, test.Length + 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => test.AsSpan().EnsureValid(0, -1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => test.AsSpan().EnsureValid(-1, test.Length));
        }

        [TestMethod]
        public void MemoryValidation_Tests()
        {
            bool[] test = new bool[20];
            Assert.IsTrue(test.AsMemory().IsValid(0, test.Length));
            Assert.IsTrue(test.AsMemory().IsValid(10, test.Length - 10));
            Assert.IsFalse(test.AsMemory().IsValid(0, test.Length + 1));
            Assert.IsFalse(test.AsMemory().IsValid(0, -1));
            Assert.IsFalse(test.AsMemory().IsValid(-1, test.Length));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => test.AsMemory().EnsureValid(0, test.Length + 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => test.AsMemory().EnsureValid(0, -1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => test.AsMemory().EnsureValid(-1, test.Length));
        }
    }
}

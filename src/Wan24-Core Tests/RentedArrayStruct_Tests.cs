using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class RentedArrayStuct_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            using RentedArrayStruct<bool> arr = new(3);
            // Length
            Assert.AreEqual(3, arr.Length);
            Assert.AreEqual(3, (int)arr);
            // Contents
            Assert.IsFalse(arr[0]);
            Assert.IsFalse(arr[1]);
            Assert.IsFalse(arr[2]);
            arr[1] = true;
            // Span
            Span<bool> span = (Span<bool>)arr;
            Assert.IsFalse(span[0]);
            Assert.IsTrue(span[1]);
            Assert.IsFalse(span[2]);
            // Memory
            span = ((Memory<bool>)arr).Span;
            Assert.IsFalse(span[0]);
            Assert.IsTrue(span[1]);
            Assert.IsFalse(span[2]);
            // Copy
            bool[] copy = arr.GetCopy();
            arr.Dispose();
            Assert.IsFalse(copy[0]);
            Assert.IsTrue(copy[1]);
            Assert.IsFalse(copy[2]);
            // Disposed
            Assert.ThrowsException<ObjectDisposedException>(() => arr.GetCopy());
        }
    }
}

using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class SecureByteArray_Tests
    {
        [TestMethod]
        public void Instance_Tests()
        {
            byte[] arr = new byte[] { 0, 1, 2 };
            using (SecureByteArray secureArr = new(arr)) { }
            Assert.AreEqual(0, arr[0]);
            Assert.AreEqual(0, arr[1]);
            Assert.AreEqual(0, arr[2]);
        }

        [TestMethod]
        public void Detach_Tests()
        {
            byte[] arr = new byte[] { 0, 1, 2 };
            using (SecureByteArray secureArr = new(arr)) secureArr.DetachAndDispose();
            Assert.AreEqual(0, arr[0]);
            Assert.AreEqual(1, arr[1]);
            Assert.AreEqual(2, arr[2]);
        }

        [TestMethod]
        public void ExplicitCast_Tests()
        {
            byte[] arr = new byte[] { 0, 1, 2 };
            using (SecureByteArray secureArr = (SecureByteArray)arr) { }
            Assert.AreEqual(0, arr[0]);
            Assert.AreEqual(0, arr[1]);
            Assert.AreEqual(0, arr[2]);
        }

        [TestMethod]
        public void ImplicitCast_Tests()
        {
            byte[] arr = new byte[] { 0, 1, 2 };
            using SecureByteArray secureArr = (SecureByteArray)arr;
            Assert.AreEqual(arr, (byte[])secureArr);
            Assert.IsTrue(arr.SequenceEqual((byte[])secureArr));
            Span<byte> span = (Span<byte>)secureArr;
            Assert.AreEqual(0, span[0]);
            Assert.AreEqual(1, span[1]);
            Assert.AreEqual(2, span[2]);
            span = ((Memory<byte>)secureArr).Span;
            Assert.AreEqual(0, span[0]);
            Assert.AreEqual(1, span[1]);
            Assert.AreEqual(2, span[2]);
            Assert.AreNotEqual(IntPtr.Zero, (IntPtr)secureArr);
            Assert.AreEqual(3, (int)secureArr);
            Assert.AreEqual(3L, (long)secureArr);
        }
    }
}

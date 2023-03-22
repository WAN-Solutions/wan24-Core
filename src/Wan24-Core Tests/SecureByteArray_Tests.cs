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
    }
}

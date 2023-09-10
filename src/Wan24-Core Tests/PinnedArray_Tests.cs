#if !NO_UNSAFE
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class PinnedArray_Tests : TestBase
    {
        [TestMethod]
        public unsafe void General_Tests()
        {
            byte[] data = new byte[] { 1 };
            using PinnedArray<byte> pin = new(data);
            Assert.AreEqual(data[0], pin[0]);
        }
    }
}
#endif

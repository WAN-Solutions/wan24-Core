using System.Text;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class StringExtensions_Tests
    {
        [TestMethod]
        public void UTF8_Tests()
        {
            Assert.AreEqual("test", "test".GetBytes().ToUtf8String());
        }
    }
}

using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class LazyValue_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            LazyValue<bool> value = new(() => true);
            Assert.IsTrue(value.Value);
        }
    }
}

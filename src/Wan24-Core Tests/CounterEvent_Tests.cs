using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class CounterEvent_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            using CounterEvent counter = new()
            {
                MinCounter = 0,
                MaxCounter = 10
            };
            //TODO
        }
    }
}

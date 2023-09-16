using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class Delay_Tests : TestBase
    {
        [TestMethod, Timeout(3000)]
        public async Task General_Tests()
        {
            DelayService service = DelayService.Instance;
            await using (service)
            {
                await service.StartAsync();
                DateTime start = DateTime.Now;
                await new Delay(TimeSpan.FromMilliseconds(100)).Task;
                Assert.IsTrue(DateTime.Now - start >= TimeSpan.FromMilliseconds(80));
            }
        }
    }
}

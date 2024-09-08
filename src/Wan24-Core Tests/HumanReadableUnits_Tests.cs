using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class HumanReadableUnits_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            Assert.AreEqual("123 B", HumanReadableUnits.FormatBytes(123));

            Assert.AreEqual("1,00 KB", HumanReadableUnits.FormatBytes(1024).Replace(',','.'));

            Assert.AreEqual("Now", HumanReadableUnits.FormatTimeSpan(TimeSpan.Zero));

            Assert.AreEqual("1 minutes ago", HumanReadableUnits.FormatTimeSpan(TimeSpan.FromMinutes(1)));
        }
    }
}

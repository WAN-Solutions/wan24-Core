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

            Assert.AreEqual("1.00 KiB", HumanReadableUnits.FormatBytes(1024).Replace(',','.'));

            Assert.AreEqual("1M 111K 111B", HumanReadableUnits.FormatBytesFull(1111111, unitFactor: HumanReadableUnits.POWERS_OF_TEN));

            Assert.AreEqual("Now", HumanReadableUnits.FormatTimeSpan(TimeSpan.Zero));

            Assert.AreEqual("1 minutes ago", HumanReadableUnits.FormatTimeSpan(TimeSpan.FromMinutes(1)));
        }
    }
}

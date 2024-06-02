using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class UnixTime_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            UnixTime now = new(DateTime.Now);
            Assert.AreEqual(now, UnixTime.Now);
            now = new(DateTime.UtcNow);
            Assert.AreEqual(now, UnixTime.Now);
            Assert.IsTrue(UnixTime.Now.IsNow);
            Assert.IsFalse(UnixTime.Now.IsPast);
            Assert.IsFalse(UnixTime.Now.IsFuture);
            Assert.IsTrue(UnixTime.MinValue.IsPast);
            Assert.IsFalse(UnixTime.MinValue.IsFuture);
            Assert.IsFalse(UnixTime.MinValue.IsNow);
            Assert.IsTrue(UnixTime.MaxValue.IsFuture);
            Assert.IsFalse(UnixTime.MaxValue.IsPast);
            Assert.IsFalse(UnixTime.MaxValue.IsNow);
            Assert.AreEqual(1, UnixTime.MinValue.AddSeconds(1).EpochSeconds);
            Assert.AreEqual(UnixTime.ONE_MINUTE, UnixTime.MinValue.AddMinutes(1).EpochSeconds);
            Assert.AreEqual(UnixTime.ONE_HOUR, UnixTime.MinValue.AddHours(1).EpochSeconds);
            Assert.AreEqual(UnixTime.ONE_DAY, UnixTime.MinValue.AddDays(1).EpochSeconds);
            Assert.AreEqual(UnixTime.ONE_DAY * 31, UnixTime.MinValue.AddMonths(1).EpochSeconds);
            Assert.AreEqual(UnixTime.ONE_DAY * 365, UnixTime.MinValue.AddYears(1).EpochSeconds);
            byte[] data = UnixTime.MaxValue.GetBytes();
            UnixTime time = new(data);
            Assert.AreEqual(UnixTime.MaxValue, time);
            string str = UnixTime.MaxValue.ToString();
            time = UnixTime.Parse(str);
            Assert.AreEqual(UnixTime.MaxValue, time);

        }
    }
}

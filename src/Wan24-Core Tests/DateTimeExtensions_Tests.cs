using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class DateTimeExtensions_Tests
    {
        [TestMethod]
        public void InRange_Tests()
        {
            TimeSpan offset = TimeSpan.FromMinutes(3);
            DateTime dt = DateTime.Now,
                begin = dt - offset,
                end = dt + offset;
            Assert.IsTrue(dt.IsInRange(begin, end));
            Assert.IsFalse(begin.IsInRange(begin, end, beginIncluding: false));
            Assert.IsTrue(end.IsInRange(begin, end, endIncluding: true));
            Assert.IsTrue(dt.IsInRange(offset));
            Assert.IsFalse(end.IsInRange(offset, begin - TimeSpan.FromMilliseconds(1)));
            Assert.IsTrue(end.IsInRange(TimeSpan.FromMinutes(5), dt));
        }

        [TestMethod]
        public void ApplyOffset_Tests()
        {
            DateTime dt = DateTime.Now,
                before = dt - TimeSpan.FromMinutes(3),
                after = dt + TimeSpan.FromMinutes(3);
            TimeSpan offset = TimeSpan.FromMinutes(5);
            Assert.IsTrue(before.ApplyOffset(offset, dt) > dt);
            Assert.IsTrue(after.ApplyOffset(offset, dt) < dt);
        }
    }
}

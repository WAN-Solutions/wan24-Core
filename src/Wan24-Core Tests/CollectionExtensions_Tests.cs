using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class CollectionExtensions_Tests
    {
        [TestMethod]
        public void AddRange_Tests()
        {
            ICollection<bool> list = new List<bool>();
            list.AddRange(true, false);
            Assert.IsTrue(list.Count == 2);
            Assert.IsTrue(list.First());
            Assert.IsFalse(list.Last());
            list = new List<bool>();
            list.AddRange(new bool[] { true, false }.AsEnumerable());
            Assert.IsTrue(list.Count == 2);
            Assert.IsTrue(list.First());
            Assert.IsFalse(list.Last());
        }
    }
}

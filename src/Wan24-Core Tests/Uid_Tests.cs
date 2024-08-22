using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class Uid_Tests : TestBase
    {
        [TestMethod]
        public void General_Uid_Tests()
        {
            Uid uid = new(),
                uid2 = Uid.Parse(uid.ToString());
            Assert.IsTrue(uid.GetBytes().SequenceEqual(uid2.GetBytes()));
        }

        [TestMethod]
        public void General_UidExt_Tests()
        {
            UidExt uid = new(),
                uid2 = UidExt.Parse(uid.ToString());
            Assert.IsTrue(uid.GetBytes().SequenceEqual(uid2.GetBytes()));
        }
    }
}

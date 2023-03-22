using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class TypeHelper_Tests
    {
        [TestMethod]
        public void GetType_Tests()
        {
            Type type = typeof(TypeHelper_Tests);
            int eventRaised = 0;
            TypeHelper.OnLoadType += (e) => eventRaised++;
            Assert.IsNull(TypeHelper.GetType(type.ToString()));
            Assert.AreEqual(1, eventRaised);
            TypeHelper.AddTypes(type);
            Assert.AreEqual(type, TypeHelper.GetType(type.ToString()));
            Assert.AreEqual(typeof(BytesExtensions), TypeHelper.GetType(typeof(BytesExtensions).ToString()));
            Assert.AreEqual(1, eventRaised);
        }
    }
}

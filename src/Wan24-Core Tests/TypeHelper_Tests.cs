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
            TypeHelper.Instance.OnLoadType += (e) => eventRaised++;
            Assert.IsNull(TypeHelper.Instance.GetType(type.ToString()));
            Assert.AreEqual(1, eventRaised);
            TypeHelper.Instance.AddTypes(type);
            Assert.AreEqual(type, TypeHelper.Instance.GetType(type.ToString()));
            Assert.AreEqual(typeof(BytesExtensions), TypeHelper.Instance.GetType(typeof(BytesExtensions).ToString()));
            Assert.AreEqual(1, eventRaised);
        }
    }
}

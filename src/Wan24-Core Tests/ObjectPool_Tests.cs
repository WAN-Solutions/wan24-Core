using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ObjectPool_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            ObjectPool<TestObject> pool = new(1, () => new());
            TestObject obj = pool.Rent();
            obj.Count++;
            pool.Return(obj);
            obj = pool.Rent();
            Assert.AreEqual(1, obj.Count);
            obj.Count++;
            TestObject obj2 = pool.Rent();
            Assert.AreEqual(0, obj2.Count);
            pool.Return(obj);
            obj2.Count++;
            pool.Return(obj2);
            obj = pool.Rent();
            Assert.AreEqual(2, obj.Count);
            obj = pool.Rent();
            Assert.AreEqual(0, obj.Count);
        }

        public sealed class TestObject
        {
            public int Count = 0;
        }
    }
}

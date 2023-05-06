using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class GenericHelper_Tests
    {
        [TestMethod]
        public void AreEqual_Tests()
        {
            TestObject<string> a = new() { Value = "a" },
                b = new() { Value = "b" },
                c = new() { Value = null };
            Assert.IsTrue(GenericHelper.AreEqual(a, a));
            Assert.IsFalse(GenericHelper.AreEqual(a, b));
            Assert.IsFalse(GenericHelper.AreEqual(a.Value, b.Value));
            Assert.IsTrue(GenericHelper.AreEqual(a.Value, a.Value));
            Assert.IsFalse(GenericHelper.AreEqual(a.Value, c.Value));
            Assert.IsFalse(GenericHelper.AreEqual(c.Value, a.Value));
            Assert.IsTrue(GenericHelper.AreEqual(c.Value, c.Value));
        }

        [TestMethod]
        public void IsNull_Tests()
        {
            TestObject<string> a = new() { Value = "a" },
                b = new() { Value = null };
            Assert.IsFalse(GenericHelper.IsNull(a));
            Assert.IsFalse(GenericHelper.IsNull(a.Value));
            Assert.IsTrue(GenericHelper.IsNull(b.Value));
        }

        [TestMethod]
        public void IsDefault_Tests()
        {
            Assert.IsTrue(GenericHelper.IsDefault<string>(null));
            Assert.IsTrue(GenericHelper.IsDefault(0));
            Assert.IsTrue(GenericHelper.IsDefault(false));
            Assert.IsFalse(GenericHelper.IsDefault(true));
        }

        [TestMethod]
        public void IsNullOrDefault_Tests()
        {
            Assert.IsTrue(GenericHelper.IsNullOrDefault<string>(null));
            Assert.IsFalse(GenericHelper.IsNullOrDefault<string>(string.Empty));
            Assert.IsTrue(GenericHelper.IsNullOrDefault(0));
            Assert.IsFalse(GenericHelper.IsNullOrDefault(1));
            int? value = null;
            Assert.IsTrue(GenericHelper.IsNullOrDefault(value));
        }

        public class TestObject<T>
        {
            public T? Value = default;
        }
    }
}

using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ValueObjectBase_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            TestObject a = new("test1", "abc"),
                b = new("test1", "def"),
                c = new("test2", "ghi");
            Assert.IsTrue((TestObject?)null == (TestObject?)null);
            Assert.IsTrue(a == a);
            Assert.IsTrue(a == b);
            Assert.IsTrue(a != c);
            Assert.IsTrue(b != c);
            Assert.IsFalse((TestObject?)null != (TestObject?)null);
            Assert.IsFalse(a != a);
            Assert.IsFalse(a != b);
            Assert.IsFalse(a == c);
            Assert.IsFalse(b == c);
            Assert.AreEqual(a, a);
            Assert.AreEqual(a, b);
            Assert.AreNotEqual(a, c);
            Assert.AreNotEqual(b, c);
        }

        private sealed class TestObject : AutoValueObjectBase<TestObject>
        {
            public TestObject(string value, string excludedValue) : base()
            {
                Value = value;
                ExcludedValue = excludedValue;
            }

            public string Value { get; }

            [ExcludeValue]
            public string ExcludedValue { get; }
        }
    }
}

using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ValueObjectBase_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            TestObject a = new("test1"),
                b = new("test1"),
                c = new("test2");
            Assert.IsTrue(a == a);
            Assert.IsTrue(a == b);
            Assert.IsTrue(a != c);
            Assert.IsTrue(b != c);
            Assert.IsFalse(a != a);
            Assert.IsFalse(a != b);
            Assert.IsFalse(a == c);
            Assert.IsFalse(b == c);
            Assert.AreEqual(a, a);
            Assert.AreEqual(a, b);
            Assert.AreNotEqual(a, c);
            Assert.AreNotEqual(b, c);
        }

        private sealed class TestObject : ValueObjectBase<TestObject>
        {
            public TestObject(string value) : base() => Value = value;

            public string Value { get; }

            protected override IEnumerable<object?> EqualsObjects()
            {
                yield return Value;
            }
        }
    }
}

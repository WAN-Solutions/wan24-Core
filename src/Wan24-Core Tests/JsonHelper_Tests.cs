using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class JsonHelper_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            // Normal
            string json = JsonHelper.Encode(new TestObject()
            {
                Property = true
            });
            Assert.IsFalse(string.IsNullOrWhiteSpace(json));
            Assert.IsFalse(json.Contains('\n'));
            TestObject? obj = JsonHelper.Decode<TestObject>(json);
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.Property);
            // Prettified
            json = JsonHelper.Encode(new TestObject()
            {
                Property = true
            }, prettify: true);
            Assert.IsTrue(json.Contains('\n'));
            obj = JsonHelper.Decode<TestObject>(json);
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.Property);
        }

        public sealed class TestObject
        {
            public bool Property { get; set; }
        }
    }
}

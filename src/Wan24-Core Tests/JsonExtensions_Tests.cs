using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class JsonExtensions_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            string json = new JsonHelper_Tests.TestObject().ToJson();
            Assert.IsFalse(json.Contains('\n'));
            json = new JsonHelper_Tests.TestObject().ToJson(prettify: true);
            Assert.IsTrue(json.Contains('\n'));
            JsonHelper_Tests.TestObject? test = (JsonHelper_Tests.TestObject?)typeof(JsonHelper_Tests.TestObject).FromJson(json);
            Assert.IsNotNull(test);
            test = json.DecodeJson<JsonHelper_Tests.TestObject>();
            Assert.IsNotNull(test);
        }
    }
}

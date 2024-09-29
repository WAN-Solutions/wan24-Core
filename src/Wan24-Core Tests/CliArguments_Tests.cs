using System.Collections.Immutable;
using System.Collections.ObjectModel;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class CliArguments_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            // Sanatizing
            Assert.AreEqual("test", CliArguments.SanitizeValue("test"));
            Assert.AreEqual("'test test'", CliArguments.SanitizeValue("test test"));
            Assert.AreEqual("'test test\\\\\\\\\\\\u0022\\\\u0027'", CliArguments.SanitizeValue("test test\\\"'"));
            // Parsing
            CliArguments ca = CliArguments.Parse("-flag1 '-flag2' \"-flag3\" --key1 value1 'value2' \"value3\" --key2 'value 1\\\\\\\\'\\\"' \"value 2\\\\\\\\'\\\"\" --key3 -value1 --key3 -value2");
            Assert.AreEqual(6, ca.Count);
            // Flags
            Assert.IsTrue(ca["flag1"]);
            Assert.IsTrue(ca["flag2"]);
            Assert.IsTrue(ca["flag3"]);
            Assert.IsFalse(ca["flag4"]);
            Assert.AreEqual(0, ca.ValueCount("flag1"));
            Assert.AreEqual(-1, ca.ValueCount("flag4"));
            Assert.IsTrue(ca.IsBoolean("flag1"));
            Assert.IsFalse(ca.IsBoolean("flag4"));
            Assert.IsFalse(ca.IsBoolean("key1"));
            Assert.IsFalse(ca.HasValues("flag1"));
            // Key/values
            Assert.IsTrue(ca["key1", true]);
            Assert.IsTrue(ca["key2", true]);
            Assert.IsTrue(ca["key3", true]);
            Assert.AreEqual(3, ca.ValueCount("key1"));
            Assert.AreEqual(2, ca.ValueCount("key2"));
            Assert.AreEqual(2, ca.ValueCount("key3"));
            // key1
            Assert.IsTrue(ca.HasValues("key1"));
            Assert.AreEqual("value1", ca.Single("key1"));
            ImmutableArray<string> values = ca.All("key1");
            Assert.AreEqual(3, values.Length);
            Assert.AreEqual("value2", values[1]);
            Assert.AreEqual("value3", values[2]);
            // key2
            values = ca.All("key2");
            Assert.AreEqual(2, values.Length);
            Assert.AreEqual("value 1\\'\"", values[0]);
            Assert.AreEqual("value 2\\'\"", values[1]);
            // key3
            values = ca.All("key3");
            Assert.AreEqual(2, values.Length);
            Assert.AreEqual("-value1", values[0]);
            Assert.AreEqual("-value2", values[1]);
            // Arguments printing
            Assert.AreEqual(@"-flag1 -flag2 -flag3 --key1 value1 value2 value3 --key2 'value 1\\\\\\u0027\\u0022' 'value 2\\\\\\u0027\\u0022' --key3 -value1 --key3 -value2", ca.ToString());
        }

        [TestMethod]
        public void DashFlag_Tests()
        {
            CliArguments ca = CliArguments.Parse("-- a");
            Assert.AreEqual(1, ca.Count);
            Assert.IsTrue(ca[string.Empty]);
            Assert.AreEqual("a", ca.Single(string.Empty));
            Assert.AreEqual("-- a", ca.ToString());
            ca = CliArguments.Parse("-- -");
            Assert.AreEqual(1, ca.Count);
            Assert.IsTrue(ca["-"]);
            Assert.IsTrue(ca.IsBoolean("-"));
            Assert.AreEqual("--", ca.ToString());
            ca = CliArguments.Parse("--");
            Assert.AreEqual(1, ca.Count);
            Assert.IsTrue(ca["-"]);
            Assert.IsTrue(ca.IsBoolean("-"));
            Assert.AreEqual("--", ca.ToString());
        }

        [TestMethod]
        public void KeyLess_Tests()
        {
            CliArguments ca = CliArguments.Parse("value1 -flag value2 --key value3");
            Assert.AreEqual(2, ca.KeyLessArguments.Length);
            Assert.AreEqual("value1", ca.KeyLessArguments[0]);
            Assert.AreEqual("value2", ca.KeyLessArguments[1]);
            Assert.IsTrue(ca["flag"]);
            Assert.IsTrue(ca["key", true]);
        }
    }
}

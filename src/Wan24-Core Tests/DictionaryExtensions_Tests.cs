using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class DictionaryExtensions_Tests
    {
        [TestMethod]
        public void Merge_Dictionary_Tests()
        {
            // General
            Dictionary<string, string> a = new()
                {
                {"a", "a" },
                {"c", "c" }
                },
                b = new()
                {
                {"a", "a2" },
                {"b", "b" }
                },
                c = a.Merge(b);
            Assert.AreEqual(3, c.Count);
            Assert.IsTrue(c.ContainsKey("a"));
            Assert.IsTrue(c.ContainsKey("b"));
            Assert.IsTrue(c.ContainsKey("c"));
            Assert.AreEqual("a2", c["a"]);
            Assert.AreEqual("b", c["b"]);
            Assert.AreEqual("c", c["c"]);

            // Prefix
            a = new()
                {
                {"a", "a" },
                {"c", "c" }
                };
            b = new()
                {
                {"a", "a2" },
                {"b", "b" }
                };
            c = a.Merge(b, prefix: "_");
            Assert.AreEqual(4, c.Count);
            Assert.IsTrue(c.ContainsKey("a"));
            Assert.IsTrue(c.ContainsKey("_a"));
            Assert.IsTrue(c.ContainsKey("_b"));
            Assert.IsTrue(c.ContainsKey("c"));
            Assert.AreEqual("a", c["a"]);
            Assert.AreEqual("a2", c["_a"]);
            Assert.AreEqual("b", c["_b"]);
            Assert.AreEqual("c", c["c"]);

            // Existing only
            a = new()
                {
                {"a", "a" },
                {"c", "c" }
                };
            b = new()
                {
                {"a", "a2" },
                {"b", "b" }
                };
            c = a.Merge(b, existingOnly: true);
            Assert.AreEqual(2, c.Count);
            Assert.IsTrue(c.ContainsKey("a"));
            Assert.IsTrue(c.ContainsKey("c"));
            Assert.AreEqual("a2", c["a"]);
            Assert.AreEqual("c", c["c"]);

            // Don't overwrite
            a = new()
                {
                {"a", "a" },
                {"c", "c" }
                };
            b = new()
                {
                {"a", "a2" },
                {"b", "b" }
                };
            c = a.Merge(b, overwrite: false);
            Assert.AreEqual(3, c.Count);
            Assert.IsTrue(c.ContainsKey("a"));
            Assert.IsTrue(c.ContainsKey("b"));
            Assert.IsTrue(c.ContainsKey("c"));
            Assert.AreEqual("a", c["a"]);
            Assert.AreEqual("b", c["b"]);
            Assert.AreEqual("c", c["c"]);
        }

        [TestMethod]
        public void Merge_Enumerable_Tests()
        {
            // General
            List<string> enumerable = ["a", "b"];
            Dictionary<string, string> a = new()
                {
                    {"a", "a" }
                },
                b = a.Merge(enumerable);
            Assert.AreEqual(3, b.Count);
            Assert.IsTrue(b.ContainsKey("a"));
            Assert.IsTrue(b.ContainsKey("0"));
            Assert.IsTrue(b.ContainsKey("1"));
            Assert.AreEqual("a", b["a"]);
            Assert.AreEqual("a", b["0"]);
            Assert.AreEqual("b", b["1"]);

            // Prefix
            a = new()
                {
                    {"a", "a" }
                };
            b = a.Merge(enumerable, prefix: "_");
            Assert.AreEqual(3, b.Count);
            Assert.IsTrue(b.ContainsKey("a"));
            Assert.IsTrue(b.ContainsKey("_0"));
            Assert.IsTrue(b.ContainsKey("_1"));
            Assert.AreEqual("a", b["a"]);
            Assert.AreEqual("a", b["_0"]);
            Assert.AreEqual("b", b["_1"]);

            // Existing only
            a = new()
                {
                    {"a", "a" }
                };
            b = a.Merge(enumerable, existingOnly: true);
            Assert.AreEqual(1, b.Count);
            Assert.IsTrue(b.ContainsKey("a"));
            Assert.AreEqual("a", b["a"]);

            // Don't overwrite
            a = new()
                {
                    {"a", "a" },
                    {"0","c" }
                };
            b = a.Merge(enumerable, overwrite: false);
            Assert.AreEqual(3, b.Count);
            Assert.IsTrue(b.ContainsKey("a"));
            Assert.IsTrue(b.ContainsKey("0"));
            Assert.IsTrue(b.ContainsKey("1"));
            Assert.AreEqual("a", b["a"]);
            Assert.AreEqual("c", b["0"]);
            Assert.AreEqual("b", b["1"]);
        }

        [TestMethod]
        public void Diff_Tests()
        {
            // General
            Dictionary<string, string?> a = new()
                {
                {"a", "a" },
                {"c","c" }
                },
                b = new()
                {
                {"a", "x" },
                {"b", "b" }
                },
                c = a.Diff(b);
            Assert.AreEqual(3, c.Count);
            Assert.IsTrue(c.ContainsKey("a"));
            Assert.IsTrue(c.ContainsKey("b"));
            Assert.IsTrue(c.ContainsKey("c"));
            Assert.AreEqual("x", c["a"]);
            Assert.AreEqual(null, c["b"]);
            Assert.AreEqual(null, c["c"]);

            // Existing A keys only
            c = a.Diff(b, existingOriginalKeysOnly: true);
            Assert.AreEqual(2, c.Count);
            Assert.IsTrue(c.ContainsKey("a"));
            Assert.IsTrue(c.ContainsKey("c"));
            Assert.AreEqual("x", c["a"]);
            Assert.AreEqual(null, c["c"]);

            // Existing B keys only
            c = a.Diff(b, existingOtherKeysOnly: true);
            Assert.AreEqual(2, c.Count);
            Assert.IsTrue(c.ContainsKey("a"));
            Assert.IsTrue(c.ContainsKey("b"));
            Assert.AreEqual("x", c["a"]);
            Assert.AreEqual(null, c["b"]);
        }

        [TestMethod]
        public void AsQueryString_Tests()
        {
            Dictionary<string, string> a = new()
            {
                {"a","a" },
                {"b","c" }
            };
            string query = a.AsQueryString();
            Assert.AreEqual("a=a&b=c", query);
        }
    }
}

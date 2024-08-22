namespace Wan24_Core_Tests
{
    [TestClass]
    public class OrderedDictionary_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            wan24.Core.OrderedDictionary<string, int> dict = new()
            {
                { "0", 0 },
                { "1", 1 },
                { "2", 2 }
            };
            dict.Remove("1");
            dict["3"] = 3;
            Assert.ThrowsException<IndexOutOfRangeException>(() => dict[4]);
            Assert.ThrowsException<KeyNotFoundException>(() => dict[string.Empty]);
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(0, dict[0]);
            Assert.AreEqual(2, dict[1]);
            Assert.AreEqual(3, dict[2]);
            Assert.AreEqual(0, dict["0"]);
            Assert.AreEqual(2, dict["2"]);
            Assert.AreEqual(3, dict["3"]);
            Assert.IsTrue(dict.Contains("0"));
            Assert.IsFalse(dict.Contains("1"));
            Assert.IsTrue(dict.Contains("2"));
            Assert.IsTrue(dict.Contains("3"));
            Assert.IsTrue(dict.ContainsKey("0"));
            Assert.IsFalse(dict.ContainsKey("1"));
            Assert.IsTrue(dict.ContainsKey("2"));
            Assert.IsTrue(dict.ContainsKey("3"));
            Assert.IsTrue(dict.ContainsValue(0));
            Assert.IsFalse(dict.ContainsValue(1));
            Assert.IsTrue(dict.ContainsValue(2));
            Assert.IsTrue(dict.ContainsValue(3));
            Assert.AreEqual(-1, dict.IndexOfKey("1"));
            Assert.AreEqual(-1, dict.IndexOfValue(1));
            Assert.AreEqual(2, dict.IndexOfKey("3"));
            Assert.AreEqual(2, dict.IndexOfValue(3));
            dict.Insert(1, "1", 1);
            Assert.AreEqual(4, dict.Count);
            Assert.IsTrue(dict.ContainsKey("1"));
            Assert.IsTrue(dict.ContainsValue(1));
            Assert.AreEqual(1, dict.IndexOfKey("1"));
            Assert.AreEqual(0, dict[0]);
            Assert.AreEqual(1, dict[1]);
            Assert.AreEqual(2, dict[2]);
            Assert.AreEqual(3, dict[3]);
            Assert.AreEqual(0, dict["0"]);
            Assert.AreEqual(1, dict["1"]);
            Assert.AreEqual(2, dict["2"]);
            Assert.AreEqual(3, dict["3"]);
            Assert.IsTrue(dict.Contains("0"));
            Assert.IsTrue(dict.Contains("1"));
            Assert.IsTrue(dict.Contains("2"));
            Assert.IsTrue(dict.Contains("3"));
            Assert.IsTrue(dict.ContainsKey("0"));
            Assert.IsTrue(dict.ContainsKey("1"));
            Assert.IsTrue(dict.ContainsKey("2"));
            Assert.IsTrue(dict.ContainsKey("3"));
            Assert.IsTrue(dict.ContainsValue(0));
            Assert.IsTrue(dict.ContainsValue(1));
            Assert.IsTrue(dict.ContainsValue(2));
            Assert.IsTrue(dict.ContainsValue(3));
            Assert.AreEqual(1, dict.IndexOfKey("1"));
            Assert.AreEqual(1, dict.IndexOfValue(1));
            Assert.AreEqual(3, dict.IndexOfKey("3"));
            Assert.AreEqual(3, dict.IndexOfValue(3));
            Assert.IsTrue(dict.Keys.SequenceEqual(Enumerable.Range(0, 4).Select(n => n.ToString())));
            Assert.IsTrue(dict.Values.SequenceEqual(Enumerable.Range(0, 4)));
            dict.ReplaceAt(1, "a", -1);
            Assert.IsTrue(dict.ContainsKey("a"));
            Assert.IsTrue(dict.ContainsValue(-1));
            Assert.AreEqual(1, dict.IndexOfKey("a"));
            Assert.AreEqual(1, dict.IndexOfValue(-1));
            dict.Clear();
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.IsReadOnly);
            dict["a"] = 1;
            dict = dict.AsReadOnly();
            Assert.IsTrue(dict.IsReadOnly);
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(0, dict.IndexOfKey("a"));
            Assert.AreEqual(0, dict.IndexOfValue(1));
            Assert.ThrowsException<NotSupportedException>(() => dict["b"] = 2);
        }
    }
}

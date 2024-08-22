namespace Wan24_Core_Tests
{
    [TestClass]
    public class EnumerationBase_Tests : TestBase
    {
        [TestMethod]
        public void General_Test()
        {
            Assert.IsTrue(EnumerationTest.Value1 == EnumerationTest.Value1);
            Assert.IsTrue(EnumerationTest.Value1 != EnumerationTest.Value2);

            Assert.IsTrue(EnumerationTest.Value1 < EnumerationTest.Value2);
            Assert.IsFalse(EnumerationTest.Value1 > EnumerationTest.Value2);

            Assert.IsTrue(EnumerationTest.Value1 <= EnumerationTest.Value1);
            Assert.IsTrue(EnumerationTest.Value1 >= EnumerationTest.Value1);

            Assert.AreEqual(2, EnumerationTest.AllValues.Count);
            Assert.IsTrue(EnumerationTest.ValueKeys.ContainsKey(1));
            Assert.IsTrue(EnumerationTest.ValueKeys.ContainsKey(2));

            Assert.IsTrue(EnumerationTest.KeyValues.ContainsKey(nameof(EnumerationTest.Value1)));
            Assert.IsTrue(EnumerationTest.KeyValues.ContainsKey(nameof(EnumerationTest.Value2)));

            Assert.AreEqual(EnumerationTest.ValueKeys[1], EnumerationTest.Value1.Name);
            Assert.AreEqual(EnumerationTest.ValueKeys[2], EnumerationTest.Value2.Name);

            Assert.AreEqual(EnumerationTest.KeyValues[EnumerationTest.Value1.Name], EnumerationTest.Value1.Value);
            Assert.AreEqual(EnumerationTest.KeyValues[EnumerationTest.Value2.Name], EnumerationTest.Value2.Value);

            Assert.IsTrue(EnumerationTest.TryParse(nameof(EnumerationTest.Value1), out EnumerationTest? value));
            Assert.AreEqual(EnumerationTest.Value1, value);
            Assert.IsFalse(EnumerationTest.TryParse(nameof(EnumerationTest), out value));
        }
    }
}

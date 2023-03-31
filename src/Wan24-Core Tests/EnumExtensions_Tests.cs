using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class EnumExtensions_Tests
    {
        [TestMethod]
        public void SignedEnum_Tests()
        {
            TestEnum value = TestEnum.Value1 | TestEnum.Flag1;
            Assert.IsTrue(value.MayContainFlags());
            Assert.AreEqual(TestEnum.Value1, value.RemoveFlags());
            Assert.AreEqual(TestEnum.Flag1, value.OnlyFlags());
            Assert.IsTrue(TestEnum.Value1.IsValue());
            Assert.IsFalse(TestEnum.Value1.IsFlag());
            Assert.IsTrue(TestEnum.Flag1.IsFlag());
            Assert.IsFalse(TestEnum.Flag1.IsValue());
            Assert.AreEqual("Test", TestEnum.Value1.GetDisplayText());
            Assert.AreEqual(nameof(TestEnum.Value2), TestEnum.Value2.GetDisplayText());
        }

        [TestMethod]
        public void UnsignedEnum_Tests()
        {
            TestEnum2 value = TestEnum2.Value1 | TestEnum2.Flag1;
            Assert.IsTrue(value.MayContainFlags());
            Assert.AreEqual(TestEnum2.Value1, value.RemoveFlags());
            Assert.AreEqual(TestEnum2.Flag1, value.OnlyFlags());
            Assert.IsTrue(TestEnum2.Value1.IsValue());
            Assert.IsFalse(TestEnum2.Value1.IsFlag());
            Assert.IsTrue(TestEnum2.Flag1.IsFlag());
            Assert.IsFalse(TestEnum2.Flag1.IsValue());
        }

        [Flags]
        public enum TestEnum : int
        {
            None = 0,
            [DisplayText("Test")]
            Value1 = 1,
            Value2 = 2,
            Value3 = 3,
            Flag1 = 1 << 8,
            Flag2 = 1 << 9,
            FLAGS = Flag1 | Flag2
        }

        [Flags]
        public enum TestEnum2 : uint
        {
            None = 0,
            Value1 = 1,
            Value2 = 2,
            Value3 = 3,
            Flag1 = 1 << 8,
            Flag2 = 1 << 9,
            FLAGS = Flag1 | Flag2
        }
    }
}

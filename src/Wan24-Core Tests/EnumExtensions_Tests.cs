using System.Collections.Frozen;
using System.Net.Sockets;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class EnumExtensions_Tests : TestBase
    {
        [TestMethod]
        public void SignedEnum_Tests()
        {
            TestEnum value = TestEnum.Value1 | TestEnum.Flag1;
            Assert.IsTrue(value.ContainsAnyFlag(TestEnum.Flag1, TestEnum.Flag2));
            Assert.IsFalse(value.ContainsAllFlags(TestEnum.Flag1 | TestEnum.Flag2));
            Assert.IsTrue(value.ContainsAllFlags(TestEnum.Flag1));
            Assert.IsFalse(value.ContainsAllFlags(TestEnum.Flag2));
            TestEnum[] flags = value.GetContainedFlags(TestEnum.Flag1, TestEnum.Flag2).ToArray();
            Assert.AreEqual(1, flags.Length);
            Assert.AreEqual(TestEnum.Flag1, flags[0]);
            EnumInfo<TestEnum> info = new();
            Assert.IsTrue(info.HasFlags);
            Assert.AreEqual(TestEnum.Value1, value.RemoveFlags());
            Assert.AreEqual(TestEnum.Flag1, value.OnlyFlags());
            Assert.IsTrue(TestEnum.Value1.IsValue());
            Assert.IsFalse(TestEnum.Value1.IsFlag());
            Assert.IsTrue(TestEnum.Flag1.IsFlag());
            Assert.IsFalse(TestEnum.Flag1.IsValue());
            Assert.IsTrue(value.IsValid());
            Assert.IsFalse(((TestEnum)4).IsValid());
            Assert.AreEqual("Test", TestEnum.Value1.GetDisplayText());
            Assert.AreEqual(nameof(TestEnum.Value2), TestEnum.Value2.GetDisplayText());
            Assert.IsTrue(typeof(TestEnum).GetEnumInfo().IsMixed);
            Assert.IsFalse(typeof(AddressFamily).GetEnumInfo().IsMixed);
            {
                FrozenDictionary<string, object> dict = info.NumericEnumValues;
                Assert.AreEqual(6, dict.Count);
                Assert.IsTrue(dict.ContainsKey(nameof(TestEnum.None)));
                Assert.IsTrue(dict.ContainsKey(nameof(TestEnum.Value1)));
                Assert.IsTrue(dict.ContainsKey(nameof(TestEnum.Value2)));
                Assert.IsTrue(dict.ContainsKey(nameof(TestEnum.Value3)));
                Assert.IsTrue(dict.ContainsKey(nameof(TestEnum.Flag1)));
                Assert.IsTrue(dict.ContainsKey(nameof(TestEnum.Flag2)));
                Assert.IsTrue(dict.ContainsValue((short)TestEnum.None));
                Assert.IsTrue(dict.ContainsValue((short)TestEnum.Value1));
                Assert.IsTrue(dict.ContainsValue((short)TestEnum.Value2));
                Assert.IsTrue(dict.ContainsValue((short)TestEnum.Value3));
                Assert.IsTrue(dict.ContainsValue((short)TestEnum.Flag1));
                Assert.IsTrue(dict.ContainsValue((short)TestEnum.Flag2));
            }
            {
                FrozenDictionary<string, TestEnum> dict = EnumInfo<TestEnum>.KeyValues;
                Assert.AreEqual(6, dict.Count);
                Assert.IsTrue(dict.ContainsKey(nameof(TestEnum.None)));
                Assert.IsTrue(dict.ContainsKey(nameof(TestEnum.Value1)));
                Assert.IsTrue(dict.ContainsKey(nameof(TestEnum.Value2)));
                Assert.IsTrue(dict.ContainsKey(nameof(TestEnum.Value3)));
                Assert.IsTrue(dict.ContainsKey(nameof(TestEnum.Flag1)));
                Assert.IsTrue(dict.ContainsKey(nameof(TestEnum.Flag2)));
                Assert.IsTrue(dict.ContainsValue(TestEnum.None));
                Assert.IsTrue(dict.ContainsValue(TestEnum.Value1));
                Assert.IsTrue(dict.ContainsValue(TestEnum.Value2));
                Assert.IsTrue(dict.ContainsValue(TestEnum.Value3));
                Assert.IsTrue(dict.ContainsValue(TestEnum.Flag1));
                Assert.IsTrue(dict.ContainsValue(TestEnum.Flag2));
            }
        }

        [TestMethod]
        public void UnsignedEnum_Tests()
        {
            TestEnum2 value = TestEnum2.Value1 | TestEnum2.Flag1;
            Assert.IsTrue(value.GetInfo().HasFlags);
            Assert.AreEqual(TestEnum2.Value1, value.RemoveFlags());
            Assert.AreEqual(TestEnum2.Flag1, value.OnlyFlags());
            Assert.IsTrue(TestEnum2.Value1.IsValue());
            Assert.IsFalse(TestEnum2.Value1.IsFlag());
            Assert.IsTrue(TestEnum2.Flag1.IsFlag());
            Assert.IsFalse(TestEnum2.Flag1.IsValue());
            Assert.IsTrue(value.IsValid());
            Assert.IsFalse(((TestEnum2)4).IsValid());
        }

        [Flags]
        public enum TestEnum : short
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
        public enum TestEnum2 : ushort
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

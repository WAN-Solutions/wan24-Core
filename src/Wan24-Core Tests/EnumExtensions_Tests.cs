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
                IReadOnlyDictionary<string, object> dict = info.NumericEnumValues;
                Assert.AreEqual(7, dict.Count);
                string[] keys = dict.Keys.ToArray();
                object[] values = dict.Values.ToArray();
                Assert.AreEqual(nameof(TestEnum.None), keys[0]);
                Assert.AreEqual(TestEnum.None, values[0]);
                Assert.AreEqual(nameof(TestEnum.Value1), keys[1]);
                Assert.AreEqual(TestEnum.Value1, values[1]);
                Assert.AreEqual(nameof(TestEnum.Value2), keys[2]);
                Assert.AreEqual(TestEnum.Value2, values[2]);
                Assert.AreEqual(nameof(TestEnum.Value3), keys[3]);
                Assert.AreEqual(TestEnum.Value3, values[3]);
                Assert.AreEqual(nameof(TestEnum.Flag1), keys[4]);
                Assert.AreEqual(TestEnum.Flag1, values[4]);
                Assert.AreEqual(nameof(TestEnum.Flag2), keys[5]);
                Assert.AreEqual(TestEnum.Flag2, values[5]);
                Assert.AreEqual(nameof(TestEnum.FLAGS), keys[6]);
                Assert.AreEqual(TestEnum.FLAGS, values[6]);
            }
            {
                IReadOnlyDictionary<string, TestEnum> dict = EnumInfo<TestEnum>.KeyValues;
                Assert.AreEqual(7, dict.Count);
                string[] keys = dict.Keys.ToArray();
                TestEnum[] values = dict.Values.ToArray();
                Assert.AreEqual(nameof(TestEnum.None), keys[0]);
                Assert.AreEqual(TestEnum.None, values[0]);
                Assert.AreEqual(nameof(TestEnum.Value1), keys[1]);
                Assert.AreEqual(TestEnum.Value1, values[1]);
                Assert.AreEqual(nameof(TestEnum.Value2), keys[2]);
                Assert.AreEqual(TestEnum.Value2, values[2]);
                Assert.AreEqual(nameof(TestEnum.Value3), keys[3]);
                Assert.AreEqual(TestEnum.Value3, values[3]);
                Assert.AreEqual(nameof(TestEnum.Flag1), keys[4]);
                Assert.AreEqual(TestEnum.Flag1, values[4]);
                Assert.AreEqual(nameof(TestEnum.Flag2), keys[5]);
                Assert.AreEqual(TestEnum.Flag2, values[5]);
                Assert.AreEqual(nameof(TestEnum.FLAGS), keys[6]);
                Assert.AreEqual(TestEnum.FLAGS, values[6]);
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

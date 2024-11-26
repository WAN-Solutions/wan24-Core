using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class NumberExtensions_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            Assert.IsFalse(1.IsUnsigned());
            Assert.IsTrue(1u.IsUnsigned());
            Assert.IsTrue(typeof(int).IsNumeric());
            Assert.IsFalse(typeof(int).IsNumericAndUnsigned());
            Assert.IsTrue(typeof(uint).IsNumericAndUnsigned());
            Assert.IsTrue(1.IsBetween(0, 2));
            Assert.IsFalse(3.IsBetween(0, 2));
            Assert.AreEqual(16, Int128.MinValue.GetBytes().Length);
            Assert.AreEqual(16, Int128.MaxValue.GetBytes().Length);
            Assert.AreEqual(16, UInt128.MinValue.GetBytes().Length);
            Assert.AreEqual(16, UInt128.MaxValue.GetBytes().Length);
            Assert.AreEqual(Int128.MinValue, Int128.MinValue.GetBytes().AsSpan().ToInt128());
            Assert.AreEqual(Int128.MaxValue, Int128.MaxValue.GetBytes().AsSpan().ToInt128());
            Assert.AreEqual(UInt128.MinValue, UInt128.MinValue.GetBytes().AsSpan().ToUInt128());
            Assert.AreEqual(UInt128.MaxValue, UInt128.MaxValue.GetBytes().AsSpan().ToUInt128());
        }
    }
}

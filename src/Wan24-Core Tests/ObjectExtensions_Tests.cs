﻿using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ObjectExtensions_Tests : TestBase
    {
        [TestMethod]
        public void In_Tests()
        {
            Assert.IsTrue(1.In(1, 2, 3));
            Assert.IsFalse(1.In(4, 5, 6));
            Assert.IsTrue(1.In(new int[] { 1, 2, 3 }.AsEnumerable()));
        }

        [TestMethod]
        public void TypeConversion_Tests()
        {
            Assert.AreEqual(1, ((ushort)1).CastType<int>());
        }

        [TestMethod]
        public void IsDisposable_Tests()
        {
            Assert.IsFalse(new object().IsDisposable());
            using DisposableAdapter temp = new((a) => { });
            Assert.IsTrue(temp.IsDisposable());
        }
    }
}

﻿using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class SecureCharArrayStruct_Tests
    {
        [TestMethod]
        public void Instance_Tests()
        {
            char[] arr = new char[] { 'a', 'b', 'c' };
            using (SecureCharArrayStruct secureArr = new(arr)) { }
            Assert.AreEqual('\0', arr[0]);
            Assert.AreEqual('\0', arr[1]);
            Assert.AreEqual('\0', arr[2]);
        }

        [TestMethod]
        public void Detach_Tests()
        {
            char[] arr = new char[] { 'a', 'b', 'c' };
            using (SecureCharArrayStruct secureArr = new(arr)) secureArr.DetachAndDispose();
            Assert.AreEqual('a', arr[0]);
            Assert.AreEqual('b', arr[1]);
            Assert.AreEqual('c', arr[2]);
        }

        [TestMethod]
        public void ExplicitCast_Tests()
        {
            char[] arr = new char[] { 'a', 'b', 'c' };
            using (SecureCharArrayStruct secureArr = (SecureCharArrayStruct)arr) { }
            Assert.AreEqual('\0', arr[0]);
            Assert.AreEqual('\0', arr[1]);
            Assert.AreEqual('\0', arr[2]);
        }

        [TestMethod]
        public void ImplicitCast_Tests()
        {
            char[] arr = new char[] { 'a', 'b', 'c' };
            using SecureCharArrayStruct secureArr = (SecureCharArrayStruct)arr;
            Assert.AreEqual(arr, (char[])secureArr);
            Assert.IsTrue(arr.SequenceEqual((char[])secureArr));
            Span<char> span = (Span<char>)secureArr;
            Assert.AreEqual('a', span[0]);
            Assert.AreEqual('b', span[1]);
            Assert.AreEqual('c', span[2]);
            span = ((Memory<char>)secureArr).Span;
            Assert.AreEqual('a', span[0]);
            Assert.AreEqual('b', span[1]);
            Assert.AreEqual('c', span[2]);
            Assert.AreNotEqual(IntPtr.Zero, (IntPtr)secureArr);
            Assert.AreEqual(3, (int)secureArr);
            Assert.AreEqual(3L, (long)secureArr);
        }
    }
}

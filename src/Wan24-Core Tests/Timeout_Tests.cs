﻿namespace Wan24_Core_Tests
{
    [TestClass]
    public class Timeout_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            using wan24.Core.Timeout timeout = new(TimeSpan.FromMilliseconds(30));
            int count = 0;
            timeout.OnTimeout += (s, e) => count++;
            timeout.Start();
            Thread.Sleep(80);
            Assert.AreEqual(1, count);
            Assert.IsFalse(timeout.IsRunning);
            timeout.AutoReset = true;
            timeout.Start();
            Thread.Sleep(80);
            Assert.AreEqual(3, count);
            Assert.IsTrue(timeout.IsRunning);
            timeout.Stop();
            count = 0;
            timeout.Start();
            Thread.Sleep(10);
            timeout.Reset();
            Thread.Sleep(10);
            timeout.Reset();
            Thread.Sleep(10);
            timeout.Reset();
            Thread.Sleep(10);
            timeout.Reset();
            Thread.Sleep(10);
            timeout.Reset();
            Assert.IsTrue(timeout.IsRunning);
            Assert.AreEqual(0, count);
        }
    }
}
﻿using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class Timeout_Tests : TestBase
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

        [TestMethod]
        public async Task RunAction_Tests()
        {
            bool didRun = false;
            wan24.Core.Timeout timeout = wan24.Core.Timeout.RunAction(TimeSpan.FromMilliseconds(20), () => didRun = true);
            await Task.Delay(200);
            Assert.IsTrue(didRun);
            Assert.IsTrue(timeout.IsDisposed);
            didRun = false;
            timeout = wan24.Core.Timeout.RunAction(TimeSpan.FromMilliseconds(20), async () =>
            {
                await Task.Yield();
                didRun = true;
            });
            await Task.Delay(200);
            Assert.IsTrue(didRun);
            Assert.IsTrue(timeout.IsDisposed);
        }

        [TestMethod, Timeout(3000)]
        public void WaitCondition_Tests()
        {
            bool condition = false;
            Task delay = ((Func<Task>)(async () =>
            {
                await Task.Yield();
                await Task.Delay(200);
                condition = true;
            })).StartFairTask();
            Thread.Sleep(50);
            Assert.IsFalse(delay.IsCompleted);
            Assert.IsFalse(condition);
            wan24.Core.Timeout.WaitCondition(TimeSpan.FromMilliseconds(50), (ct) => condition);
            Assert.IsTrue(delay.IsCompleted);
            Assert.IsTrue(condition);
        }

        [TestMethod, Timeout(3000)]
        public async Task WaitConditionAsync_Tests()
        {
            bool condition = false;
            Task task = wan24.Core.Timeout.WaitConditionAsync(TimeSpan.FromMilliseconds(50), (ct) => Task.FromResult(!ct.IsCancellationRequested && condition));
            await Task.Delay(200);
            Assert.IsFalse(task.IsCompleted);
            condition = true;
            await Task.Delay(200);
            Assert.IsTrue(task.IsCompleted);
        }
    }
}

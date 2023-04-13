﻿using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class QueueWorker_Tests
    {
        [TestMethod]
        public async Task General_Tests()
        {
            using QueueWorker worker = new(1);
            await worker.StartAsync(default);
            int worked = 0;
            using ManualResetEventSlim task1Event = new(initialState: false);
            await worker.EnqueueAsync(async (ct) =>
            {
                await Task.Yield();
                task1Event.Wait(ct);
                worked++;
            });
            using ManualResetEventSlim task2Event = new(initialState: false);
            ValueTask addTask = worker.EnqueueAsync(async (ct) =>
            {
                await Task.Yield();
                task2Event.Wait(ct);
                worked++;
            });
            task1Event.Set();
            Thread.Sleep(20);
            Assert.AreEqual(1, worked);
            await addTask;
            task2Event.Set();
            Thread.Sleep(20);
            Assert.AreEqual(2, worked);
        }
    }
}
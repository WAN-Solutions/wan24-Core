using wan24.Core.Threading;

namespace Wan24_Core_Tests.Threading
{
    [TestClass]
    public class Worker_Tests
    {
        [TestMethod, Timeout(3000)]
        public void General_Tests()
        {
            using TestWorker worker = new();
            int work = 0,
                workDone = 0,
                started = 0,
                stopped = 0,
                didStart = 0,
                didStop = 0;
            worker.OnStart += (s, e) => started++;
            worker.OnStop += (s, e) => stopped++;
            worker.OnWork += (s, e) => work++;
            worker.OnWorkDone += (s, e) => workDone++;
            Task waitStart = Task.Run(() =>
                {
                    worker.WaitStart();
                    didStart++;
                }),
                waitStop = Task.Run(() =>
                {
                    worker.WaitStop();
                    didStop++;
                });
            // Test initial state
            Assert.IsFalse((bool)worker);
            Assert.IsFalse(worker.IsWorking);
            Assert.IsTrue(worker.IsCancelled);
            // Test start
            worker.Start();
            Assert.IsTrue((bool)worker);
            Assert.IsFalse(worker.IsWorking);
            Assert.IsFalse(worker.IsCancelled);
            Assert.AreEqual(0, work);
            Assert.AreEqual(0, workDone);
            Assert.AreEqual(1, started);
            Assert.AreEqual(0, stopped);
            // Test working
            worker.SetWork();
            Thread.Sleep(20);
            Assert.AreEqual(1, work);
            Assert.AreEqual(0, workDone);
            Assert.IsTrue(worker.IsWorking);
            // Test work done
            worker.TestState.Set(true);
            worker.TestState.WaitReset();
            Thread.Sleep(20);
            Assert.AreEqual(1, work);
            Assert.AreEqual(1, workDone);
            Assert.IsFalse(worker.IsWorking);
            // Test stop
            worker.Stop();
            Assert.IsFalse((bool)worker);
            Assert.IsFalse(worker.IsWorking);
            Assert.IsTrue(worker.IsCancelled);
            Assert.AreEqual(1, started);
            Assert.AreEqual(1, stopped);
            Assert.AreEqual(1, work);
            Assert.AreEqual(1, workDone);
            // Test wait start/stop
            Thread.Sleep(20);
            Assert.AreEqual(1, didStart);
            Assert.AreEqual(1, didStop);
        }

        public sealed class TestWorker : Worker
        {
            public readonly State TestState = new();

            public TestWorker() : base(syncWork: true) => Work.Set(false);

            public void SetWork() => Work.Set(true);

            protected override void DoWork()
            {
                TestState.WaitSet();
                Work.Set(false);
                TestState.Set(false);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                TestState.Dispose();
            }
        }
    }
}

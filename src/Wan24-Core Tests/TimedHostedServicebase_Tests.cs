using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class TimedHostedServicebase_Tests
    {
        [TestMethod, Timeout(3000)]
        public async Task Default_Tests()
        {
            using TestObject worker = new(50, HostedServiceTimers.Default);
            await worker.StartAsync();
            await Task.Delay(350);
            await worker.StopAsync();
            Assert.AreEqual(2, worker.Worked);
        }

        [TestMethod, Timeout(3000)]
        public async Task Exact_Tests()
        {
            using TestObject worker = new(20, HostedServiceTimers.Exact);
            await worker.StartAsync();
            await Task.Delay(250);
            await worker.StopAsync();
            Assert.AreEqual(3, worker.Worked);
        }

        [TestMethod, Timeout(3000)]
        public async Task ExactCatchingUp_Tests()
        {
            using TestObject worker = new(20, HostedServiceTimers.ExactCatchingUp);
            await worker.StartAsync();
            await Task.Delay(350);
            await worker.StopAsync();
            Assert.AreEqual(4, worker.Worked);
        }

        [TestMethod, Timeout(3000)]
        public async Task RunOnceAndNextRun_Tests()
        {
            using TestObject worker = new(20, HostedServiceTimers.ExactCatchingUp)
            {
                RunOnce = true
            };
            int ran = 0;
            worker.OnRan += (s, e) => ran++;
            await worker.SetTimerAsync(20, HostedServiceTimers.Default, DateTime.Now.AddMilliseconds(100));
            await Task.Delay(50);
            Assert.AreEqual(0, worker.Worked);
            await Task.Delay(200);
            Assert.IsFalse(worker.IsRunning);
            Assert.AreEqual(1, worker.Worked);
            Assert.AreEqual(1, ran);
        }

        public sealed class TestObject : TimedHostedServiceBase
        {
            public readonly int Delay;
            public int Worked = 0;

            public TestObject(int delay, HostedServiceTimers timer, double interval = 100) : base(interval, timer) => Delay = delay;

            protected override async Task WorkerAsync()
            {
                if (TimerType == HostedServiceTimers.ExactCatchingUp && Worked == 0)
                {
                    await Task.Delay(Delay * 3);
                }
                else
                {
                    await Task.Delay(Delay);
                }
                Worked++;
            }
        }
    }
}

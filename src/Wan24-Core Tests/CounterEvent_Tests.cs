using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class CounterEvent_Tests : TestBase
    {
        [TestMethod, Timeout(3000)]
        public void General_Tests()
        {
            using CounterEvent counter = new()
            {
                MinCounter = 0,
                MaxCounter = 10,
                FitNewValue = true,
                ForceRaiseEvent = true
            };
            int eventCount = 0;
            counter.OnCount += (s, e) => eventCount++;
            Assert.AreEqual(0, counter.Counter, "Invalid initial counter value");

            // Count
            Assert.AreEqual(1, counter.Count(), "Wrong count");
            Assert.AreEqual(1, eventCount, "Wrong event count");

            // Negative count
            Assert.AreEqual(0, counter.Count(-1), "Wrong negative count");
            Assert.AreEqual(2, eventCount, "Wrong negative event count");

            // Fit value
            Assert.AreEqual(10, counter.Count(20), "Wrong fitted value");
            Assert.AreEqual(3, eventCount, "Wrong event count 2");
            Assert.AreEqual(0, counter.Count(-20), "Wrong fitted negative value");
            Assert.AreEqual(4, eventCount, "Wrong event count 3");

            // Event on same value
            Assert.AreEqual(0, counter.SetCounter(10), "Wrong new value");
            Assert.AreEqual(5, eventCount, "Wrong event count 4");
            Assert.AreEqual(10, counter.SetCounter(10), "Wrong new value");
            Assert.AreEqual(6, eventCount, "No event on same value");

            // Try count
            Assert.IsTrue(counter.TryCount(1) is null, "Can overflow on try");
            Assert.IsTrue(counter.TryCount(-11) is null, "Can underflow on try");

            // Wait counter value
            Logging.WriteInfo("Test count wait");
            Task<int> task = Task.Run(() => counter.WaitCount());
            Thread.Sleep(200);
            counter.SetCounter(0);
            Assert.AreEqual(0, task.Result, "Wrong count after wait");
            Logging.WriteInfo("Test counter equals wait");
            Task task2 = Task.Run(() => counter.WaitCounterEquals(1));
            Thread.Sleep(200);
            counter.Count();
            task2.Wait();
            Logging.WriteInfo("Test counter not equals wait");
            task = Task.Run(() => counter.WaitCounterNotEquals(1));
            Thread.Sleep(200);
            counter.Count();
            Assert.AreEqual(2, task.Result, "Wrong count after wait not equal");
            Logging.WriteInfo("Test counter greater than");
            task = Task.Run(() => counter.WaitCounterGreater(2));
            Thread.Sleep(200);
            counter.Count();
            Assert.AreEqual(3, task.Result, "Wrong count after wait greater than");
            Logging.WriteInfo("Test counter lower than");
            task = Task.Run(() => counter.WaitCounterLower(3));
            Thread.Sleep(200);
            counter.Count(-1);
            Assert.AreEqual(2, task.Result, "Wrong count after wait lower than");
            Logging.WriteInfo("Test condition"); ;
            task = Task.Run(() => counter.WaitCondition(() => counter.Counter == 3 ? -1 : null));
            Thread.Sleep(200);
            counter.Count();
            Assert.AreEqual(-1, task.Result, "Wrong return value after wait condition");
        }

        [TestMethod, Timeout(3000)]
        public async Task GeneralAsync_Tests()
        {
            using CounterEvent counter = new()
            {
                MinCounter = 0,
                MaxCounter = 10,
                FitNewValue = true,
                ForceRaiseEvent = true
            };
            int eventCount = 0;
            counter.OnCount += (s, e) => eventCount++;
            Assert.AreEqual(0, counter.Counter, "Invalid initial counter value");

            // Count
            Assert.AreEqual(1, await counter.CountAsync(), "Wrong count");
            Assert.AreEqual(1, eventCount, "Wrong event count");

            // Negative count
            Assert.AreEqual(0, await counter.CountAsync(-1), "Wrong negative count");
            Assert.AreEqual(2, eventCount, "Wrong negative event count");

            // Fit value
            Assert.AreEqual(10, await counter.CountAsync(20), "Wrong fitted value");
            Assert.AreEqual(3, eventCount, "Wrong event count 2");
            Assert.AreEqual(0, await counter.CountAsync(-20), "Wrong fitted negative value");
            Assert.AreEqual(4, eventCount, "Wrong event count 3");

            // Event on same value
            Assert.AreEqual(0, await counter.SetCounterAsync(10), "Wrong new value");
            Assert.AreEqual(5, eventCount, "Wrong event count 4");
            Assert.AreEqual(10, await counter.SetCounterAsync(10), "Wrong new value");
            Assert.AreEqual(6, eventCount, "No event on same value");

            // Try count
            Assert.IsTrue(await counter.TryCountAsync(1) is null, "Can overflow on try");
            Assert.IsTrue(await counter.TryCountAsync(-11) is null, "Can underflow on try");

            // Wait counter value
            Logging.WriteInfo("Test count wait");
            Task<int> task = Task.Run(async () => await counter.WaitCountAsync());
            await Task.Delay(200);
            await counter.SetCounterAsync(0);
            Assert.AreEqual(0, task.Result, "Wrong count after wait");
            Logging.WriteInfo("Test counter equals wait");
            Task task2 = Task.Run(async () => await counter.WaitCounterEqualsAsync(1));
            await Task.Delay(200);
            await counter.CountAsync();
            task2.Wait();
            Logging.WriteInfo("Test counter not equals wait");
            task = Task.Run(async () => await counter.WaitCounterNotEqualsAsync(1));
            await Task.Delay(200);
            await counter.CountAsync();
            Assert.AreEqual(2, task.Result, "Wrong count after wait not equal");
            Logging.WriteInfo("Test counter greater than");
            task = Task.Run(async () => await counter.WaitCounterGreaterAsync(2));
            await Task.Delay(200);
            await counter.CountAsync();
            Assert.AreEqual(3, task.Result, "Wrong count after wait greater than");
            Logging.WriteInfo("Test counter lower than");
            task = Task.Run(async () => await counter.WaitCounterLowerAsync(3));
            await Task.Delay(200);
            await counter.CountAsync(-1);
            Assert.AreEqual(2, task.Result, "Wrong count after wait lower than");
            Logging.WriteInfo("Test condition"); ;
            task = Task.Run(async () => await counter.WaitConditionAsync(() => counter.Counter == 3 ? -1 : null));
            await Task.Delay(200);
            await counter.CountAsync();
            Assert.AreEqual(-1, task.Result, "Wrong return value after wait condition");
        }
    }
}

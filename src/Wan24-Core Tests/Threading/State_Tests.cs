using wan24.Core.Threading;

namespace Wan24_Core_Tests.Threading
{
    [TestClass]
    public class State_Tests
    {
        [TestMethod, Timeout(3000)]
        public async Task General_Tests()
        {
            using State state = new()
            {
                StateWhenDisposing = false
            };
            Assert.IsFalse(state.IsSet);
            int changedLocked = 0,
                changed = 0,
                setLocked = 0,
                set = 0,
                resetLocked = 0,
                reset = 0;
            state.OnStateChangedLocked += (s, e) => changedLocked++;
            state.OnStateChanged += (s, e) => changed++;
            state.OnSetLocked += (s, e) => setLocked++;
            state.OnSet += (s, e) => set++;
            state.OnResetLocked += (s, e) => resetLocked++;
            state.OnReset += (s, e) => reset++;
            // Test set events
            state.Set(true);
            Assert.IsTrue(state.IsSet);
            Assert.AreEqual(1, changedLocked);
            Assert.AreEqual(1, changed);
            Assert.AreEqual(1, setLocked);
            Assert.AreEqual(1, set);
            // Test reset events
            state.Set(false);
            Assert.AreEqual(2, changedLocked);
            Assert.AreEqual(2, changed);
            Assert.AreEqual(1, setLocked);
            Assert.AreEqual(1, set);
            Assert.AreEqual(1, resetLocked);
            Assert.AreEqual(1, reset);
            // Test set waiting
            bool waited = false;
            Task stateWaiter = Task.Run(() => waited = state.WaitSet());
            await Task.Delay(10);
            Assert.IsFalse(waited);
            state.Set(true);
            await stateWaiter;
            Assert.IsTrue(waited);
            // Test reset waiting
            waited = false;
            stateWaiter = Task.Run(() => waited = state.WaitReset());
            await Task.Delay(10);
            Assert.IsFalse(waited);
            state.Set(false);
            await stateWaiter;
            Assert.IsTrue(waited);
            // Test set waiting timeout
            waited = false;
            stateWaiter = Task.Run(() => waited = state.WaitSet(TimeSpan.FromMilliseconds(20)));
            await Task.Delay(50);
            Assert.IsFalse(waited);
            state.Set(true);
            await stateWaiter;
            Assert.IsFalse(waited);
            // Test reset waiting timeout
            waited = false;
            stateWaiter = Task.Run(() => waited = state.WaitReset(TimeSpan.FromMilliseconds(20)));
            await Task.Delay(50);
            Assert.IsFalse(waited);
            await stateWaiter;
            Assert.IsFalse(waited);
            // Test wait set and reset
            state.Set(false);
            Task setTask = Task.Run(() => state.WaitSetAndReset());
            await Task.Delay(50);
            state.Set(true);
            await Task.Delay(50);
            Assert.IsFalse(state.IsSet);
            // Test wait reset and set
            state.Set(true);
            Task resetTask = Task.Run(() => state.WaitResetAndSet());
            await Task.Delay(50);
            state.Set(false);
            await Task.Delay(50);
            Assert.IsTrue(state.IsSet);
            // Test state when disposing
            state.Dispose();
            Assert.IsFalse(state.IsSet);
        }
    }
}

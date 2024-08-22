using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class VolatileValue_Tests : TestBase
    {
        [TestMethod, Timeout(3000)]
        public async Task General_Tests()
        {
            using VV value = new();
            Task<bool> getter = value.CurrentValue;
            await Task.Delay(500);
            Assert.IsFalse(getter.IsCompleted);
            value.Set();
            Assert.IsTrue(getter.IsCompleted);
            Assert.IsTrue(await getter);
        }

        [TestMethod, Timeout(3000)]
        public async Task Lazy_Tests()
        {
            using LVV value = new();
            Assert.IsTrue(await value.CurrentValue);
        }

        [TestMethod, Timeout(3000)]
        public async Task Timeout_Tests()
        {
            using LTVV value = new();
            Assert.IsTrue(await value.CurrentValue);
            Assert.IsTrue(value.CurrentValue.IsCompleted);
            Assert.IsTrue(await value.CurrentValue);
            await Task.Delay(500);
            Assert.IsFalse(value.CurrentValue.IsCompleted);
            Assert.IsTrue(await value.CurrentValue);
        }

        public sealed class VV() : VolatileValueBase<bool>()
        {
            public void Set() => SetCurrentValue();

            protected override void SetCurrentValue()
            {
                try
                {
                    ValueCreated = DateTime.Now;
                    _CurrentValue.SetResult(true);
                }
                catch
                {
                }
            }
        }

        public sealed class LVV : LazyVolatileValueBase<bool>
        {
            public LVV() : base() => SetCurrentValue();

            protected override async void SetCurrentValue()
            {
                await Task.Yield();
                while (!Cancellation.IsCancellationRequested)
                    try
                    {
                        await ValueRequestEvent.WaitAndResetAsync();
                        ValueCreated = DateTime.Now;
                        _CurrentValue.SetResult(true);
                    }
                    catch
                    {
                    }
            }
        }

        public sealed class LTVV : LazyVolatileTimeoutValueBase<bool>
        {
            public LTVV() : base(TimeSpan.FromMilliseconds(200)) => SetCurrentValue();

            protected override async void SetCurrentValue()
            {
                await Task.Yield();
                while (!Cancellation.IsCancellationRequested)
                    try
                    {
                        await ValueRequestEvent.WaitAndResetAsync();
                        ValueCreated = DateTime.Now;
                        _CurrentValue.SetResult(true);
                    }
                    catch
                    {
                    }
            }
        }
    }
}

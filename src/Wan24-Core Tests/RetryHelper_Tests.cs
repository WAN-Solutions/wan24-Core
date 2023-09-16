using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class RetryHelper_Tests : TestBase
    {
        [TestMethod, Timeout(3000)]
        public void General_Tests()
        {
            // All failed
            {
                RetryInfo<object> info = RetryHelper.TryAction(
                    (i, ct) => throw new Exception(),
                    maxNumberOfTries: 3
                    );
                Assert.IsFalse(info.Succeed);
                Assert.AreEqual(3, info.NumberOfTries);
                Assert.AreEqual(3, info.Exceptions.Count);
                Assert.IsFalse(info.WasCanceled);
                Assert.IsFalse(info.WasTimeout);
            }
            // Last succeed
            {
                RetryInfo<object> info = RetryHelper.TryAction(
                    (i, ct) =>
                    {
                        if (i != 3) throw new Exception();
                    },
                    maxNumberOfTries: 3
                    );
                Assert.IsTrue(info.Succeed);
                Assert.AreEqual(3, info.NumberOfTries);
                Assert.AreEqual(2, info.Exceptions.Count);
                Assert.IsFalse(info.WasCanceled);
                Assert.IsFalse(info.WasTimeout);
            }
            // Cancelled
            {
                using CancellationTokenSource cts = new();
                cts.Cancel();
                RetryInfo<object> info = RetryHelper.TryAction(
                    (i, ct) => throw new Exception(),
                    maxNumberOfTries: 3,
                    cancellationToken: cts.Token
                    );
                Assert.IsFalse(info.Succeed);
                Assert.AreEqual(1, info.NumberOfTries);
                Assert.AreEqual(1, info.Exceptions.Count);
                Assert.IsTrue(info.WasCanceled);
                Assert.IsFalse(info.WasTimeout);
            }
            // Timeout
            {
                RetryInfo<object> info = RetryHelper.TryAction(
                    (i, ct) => throw new Exception(),
                    maxNumberOfTries: 3,
                    timeout: TimeSpan.FromMilliseconds(100),
                    delay: TimeSpan.FromMilliseconds(200)
                    );
                Assert.IsFalse(info.Succeed);
                Assert.AreEqual(2, info.NumberOfTries);
                Assert.AreEqual(2, info.Exceptions.Count);
                Assert.IsFalse(info.WasCanceled);
                Assert.IsTrue(info.WasTimeout);
                Assert.IsTrue(info.Runtime > TimeSpan.FromMilliseconds(100));
            }
            // Value
            {
                RetryInfo<bool> info = RetryHelper.TryAction(
                    (i, ct) => true,
                    maxNumberOfTries: 3
                    );
                Assert.IsTrue(info.Succeed);
                Assert.AreEqual(1, info.NumberOfTries);
                Assert.AreEqual(0, info.Exceptions.Count);
                Assert.IsFalse(info.WasCanceled);
                Assert.IsFalse(info.WasTimeout);
                Assert.IsTrue(info.Result);
            }
        }

        [TestMethod, Timeout(3000)]
        public async Task GeneralAsync_Tests()
        {
            // All failed
            {
                RetryInfo<object> info = await RetryHelper.TryActionAsync(
                    (i, ct) => throw new Exception(),
                    maxNumberOfTries: 3
                    );
                Assert.IsFalse(info.Succeed);
                Assert.AreEqual(3, info.NumberOfTries);
                Assert.AreEqual(3, info.Exceptions.Count);
                Assert.IsFalse(info.WasCanceled);
                Assert.IsFalse(info.WasTimeout);
            }
            // Last succeed
            {
                RetryInfo<object> info = await RetryHelper.TryActionAsync(
                    (i, ct) =>
                    {
                        if (i != 3) throw new Exception();
                        return Task.CompletedTask;
                    },
                    maxNumberOfTries: 3
                    );
                Assert.IsTrue(info.Succeed);
                Assert.AreEqual(3, info.NumberOfTries);
                Assert.AreEqual(2, info.Exceptions.Count);
                Assert.IsFalse(info.WasCanceled);
                Assert.IsFalse(info.WasTimeout);
            }
            // Cancelled
            {
                using CancellationTokenSource cts = new();
                cts.Cancel();
                RetryInfo<object> info = await RetryHelper.TryActionAsync(
                    (i, ct) => throw new Exception(),
                    maxNumberOfTries: 3,
                    cancellationToken: cts.Token
                    );
                Assert.IsFalse(info.Succeed);
                Assert.AreEqual(1, info.NumberOfTries);
                Assert.AreEqual(1, info.Exceptions.Count);
                Assert.IsTrue(info.WasCanceled);
                Assert.IsFalse(info.WasTimeout);
            }
            // Timeout
            {
                RetryInfo<object> info = await RetryHelper.TryActionAsync(
                    (i, ct) => throw new Exception(),
                    maxNumberOfTries: 3,
                    timeout: TimeSpan.FromMilliseconds(100),
                    delay: TimeSpan.FromMilliseconds(200)
                    );
                Assert.IsFalse(info.Succeed);
                Assert.AreEqual(2, info.NumberOfTries);
                Assert.AreEqual(2, info.Exceptions.Count);
                Assert.IsFalse(info.WasCanceled);
                Assert.IsTrue(info.WasTimeout);
                Assert.IsTrue(info.Runtime > TimeSpan.FromMilliseconds(100));
            }
            // Value
            {
                RetryInfo<bool> info = await RetryHelper.TryActionAsync(
                    (i, ct) => Task.FromResult(true),
                    maxNumberOfTries: 3
                    );
                Assert.IsTrue(info.Succeed);
                Assert.AreEqual(1, info.NumberOfTries);
                Assert.AreEqual(0, info.Exceptions.Count);
                Assert.IsFalse(info.WasCanceled);
                Assert.IsFalse(info.WasTimeout);
                Assert.IsTrue(info.Result);
            }
        }
    }
}

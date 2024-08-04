using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class CancellationTokenExtensions_Tests : TestBase
    {
        [TestMethod, Timeout(3000)]
        public async Task GetAwaiter_Tests()
        {
            using CancellationTokenSource cts = new();
            Task waiting = Task.Run(async () => await cts.Token);
            await Task.Delay(500);
            Assert.IsFalse(waiting.IsCompleted);
            cts.Cancel();
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await waiting);
        }

        [TestMethod]
        public void GetIsCancellationRequested_Tests()
        {
            using CancellationTokenSource cts = new();
            Assert.IsFalse(cts.Token.GetIsCancellationRequested());
            cts.Cancel();
            Assert.ThrowsException<OperationCanceledException>(() => cts.Token.GetIsCancellationRequested());
        }

        [TestMethod]
        public void EnsureNotDefault_Tests()
        {
            using CancellationTokenSource cts = new();
            CancellationToken token = default;
            token = token.EnsureNotDefault(cts.Token);
            Assert.IsTrue(token.IsEqualTo(cts.Token));
            token = token.EnsureNotDefault(default);
            Assert.IsFalse(token.IsEqualTo(default));
            Assert.IsTrue(token.IsEqualTo(cts.Token));
        }

        [TestMethod]
        public void IsEqualTo_Tests()
        {
            using CancellationTokenSource cts = new();
            CancellationToken token = default;
            Assert.IsTrue(token.IsEqualTo(default));
            Assert.IsFalse(token.IsEqualTo(cts.Token));
            Assert.IsFalse(cts.Token.IsEqualTo(default));
            Assert.IsTrue(cts.Token.IsEqualTo(cts.Token));
        }

        [TestMethod]
        public void Remove_Tests()
        {
            using CancellationTokenSource cts1 = new(),
                cts2 = new();
            CancellationToken[] tokens = [.. new CancellationToken[] { default, CancellationToken.None, cts1.Token, cts1.Token, cts2.Token, cts2.Token, default }
                .RemoveNoneAndDefaultAnd(cts1.Token)
                .RemoveDoubles()
                ];
            Assert.AreEqual(1, tokens.Length);
            Assert.IsTrue(tokens[0].IsEqualTo(cts2.Token));
        }
    }
}

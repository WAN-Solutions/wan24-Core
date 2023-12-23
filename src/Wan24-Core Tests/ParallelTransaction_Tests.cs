using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ParallelTransaction_Tests
    {
        [TestMethod, Timeout(10000)]
        public async Task GeneralAsync_Tests()
        {
            int counter = 0;
            ParallelTransaction transaction = new();
            await using (transaction)
            {
                // Succeeding actions
                await transaction.ExecuteAsync(IncreaseCounter, DecreaseCounter);
                await transaction.ExecuteAsync(IncreaseCounter, DecreaseCounter);
                await transaction.ExecuteAsync(IncreaseCounter, DecreaseCounter);
                await transaction.ExecuteAsync(IncreaseCounter, DecreaseCounter);
                await transaction.ExecuteAsync(IncreaseCounter, DecreaseCounter);
                await transaction.WaitDoneAsync();
                Assert.IsFalse(transaction.IsCommitted);
                Assert.AreEqual(5, transaction.ActionCount);
                Assert.AreEqual(5, counter);

                // Commit
                transaction.Commit();
                Assert.IsTrue(transaction.IsCommitted);
                Assert.AreEqual(0, transaction.ActionCount);
                Assert.AreEqual(5, counter);

                // Failing action, early commit, failing commit and rollback
                await transaction.ExecuteAsync(IncreaseCounter, DecreaseCounter);
                await transaction.ExecuteAsync(IncreaseCounter, DecreaseCounter);
                await transaction.ExecuteAsync(Error, DecreaseCounter);
                await transaction.ExecuteAsync(IncreaseCounter, DecreaseCounter);
                await transaction.ExecuteAsync(IncreaseCounter, DecreaseCounter);
                Assert.ThrowsException<InvalidOperationException>(() => transaction.Commit());
                Assert.IsFalse(transaction.IsCommitted);
                await Assert.ThrowsExceptionAsync<AggregateException>(async () => await transaction.WaitDoneAsync());
                Assert.IsFalse(transaction.IsCommitted);
                Assert.ThrowsException<InvalidOperationException>(() => transaction.Commit());
                Assert.IsFalse(transaction.IsCommitted);
                await transaction.RollbackAsync().DynamicContext();
                Assert.IsTrue(transaction.IsCommitted);
                Assert.AreEqual(0, transaction.ActionCount);
                Assert.AreEqual(5, counter);

                // Rollback during dispose
                await transaction.ExecuteAsync(IncreaseCounter, DecreaseCounter);
                await transaction.ExecuteAsync(IncreaseCounter, DecreaseCounter);
                await transaction.ExecuteAsync(IncreaseCounter, DecreaseCounter);
                await transaction.ExecuteAsync(IncreaseCounter, DecreaseCounter);
                await transaction.ExecuteAsync(IncreaseCounter, DecreaseCounter);
                await transaction.DisposeAsync();
                Assert.IsTrue(transaction.IsCommitted);
                Assert.AreEqual(0, transaction.ActionCount);
                Assert.AreEqual(5, counter);
            }
            async Task<object?> IncreaseCounter(CancellationToken ct)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(RandomNumberGenerator.GetInt32(100, 500), ct).DynamicContext();
                ct.ThrowIfCancellationRequested();
                return Interlocked.Increment(ref counter);
            }
            async Task<object?> Error(CancellationToken ct)
            {
                await Task.Yield();
                ct.ThrowIfCancellationRequested();
                throw new InvalidProgramException();
            }
            async Task DecreaseCounter(ParallelTransaction ta, object? ret, CancellationToken ct)
            {
                await Task.Yield();
                ct.ThrowIfCancellationRequested();
                if (ret is not null) Interlocked.Decrement(ref counter);
            }
        }
    }
}

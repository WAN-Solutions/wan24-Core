using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class Transaction_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            int counter = 0;
            using Transaction transaction = new();

            // Succeeding action
            Assert.AreEqual(1, transaction.Execute(() => ++counter, (ta, ret) => counter--));
            Assert.IsFalse(transaction.IsCommitted);
            Assert.AreEqual(1, transaction.ActionCount);
            Assert.AreEqual(1, counter);

            // Commit
            transaction.Commit();
            Assert.IsTrue(transaction.IsCommitted);
            Assert.AreEqual(0, transaction.ActionCount);
            Assert.AreEqual(1, counter);

            // Failing action
            Assert.AreEqual(2, transaction.Execute(() => ++counter, (ta, ret) => counter--));
            Assert.ThrowsException<InvalidProgramException>(() => transaction.Execute(() => throw new InvalidProgramException(), (ta, ret) => { }));
            Assert.IsFalse(transaction.IsCommitted);
            Assert.AreEqual(2, transaction.ActionCount);
            Assert.AreEqual(2, counter);

            // Rollback
            transaction.Rollback();
            Assert.IsTrue(transaction.IsCommitted);
            Assert.AreEqual(0, transaction.ActionCount);
            Assert.AreEqual(1, counter);

            // Rollback during dispose
            Assert.AreEqual(2, transaction.Execute(() => ++counter, (ta, ret) => counter--));
            Assert.IsFalse(transaction.IsCommitted);
            Assert.AreEqual(1, transaction.ActionCount);
            Assert.AreEqual(2, counter);
            transaction.Dispose();
            Assert.IsTrue(transaction.IsCommitted);
            Assert.AreEqual(0, transaction.ActionCount);
            Assert.AreEqual(1, counter);
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            int counter = 0;
            using Transaction transaction = new();

            // Succeeding action
            Assert.AreEqual(1, await transaction.ExecuteAsync(() => Task.FromResult(++counter), (ta, ret, ct) =>
            {
                counter--;
                return Task.CompletedTask;
            }));
            Assert.IsFalse(transaction.IsCommitted);
            Assert.AreEqual(1, transaction.ActionCount);
            Assert.AreEqual(1, counter);

            // Commit
            transaction.Commit();
            Assert.IsTrue(transaction.IsCommitted);
            Assert.AreEqual(0, transaction.ActionCount);
            Assert.AreEqual(1, counter);

            // Failing action
            Assert.AreEqual(2, await transaction.ExecuteAsync(() => Task.FromResult(++counter), (ta, ret, ct) =>
            {
                counter--;
                return Task.CompletedTask;
            }));
            await Assert.ThrowsExceptionAsync<InvalidProgramException>(async () => await transaction.ExecuteAsync(() => throw new InvalidProgramException(), (ta, ret, ct) => Task.CompletedTask));
            Assert.IsFalse(transaction.IsCommitted);
            Assert.AreEqual(2, transaction.ActionCount);
            Assert.AreEqual(2, counter);

            // Rollback
            await transaction.RollbackAsync();
            Assert.IsTrue(transaction.IsCommitted);
            Assert.AreEqual(0, transaction.ActionCount);
            Assert.AreEqual(1, counter);

            // Rollback during dispose
            Assert.AreEqual(2, await transaction.ExecuteAsync(() => Task.FromResult(++counter), (ta, ret, ct) =>
            {
                counter--;
                return Task.CompletedTask;
            }));
            Assert.IsFalse(transaction.IsCommitted);
            Assert.AreEqual(1, transaction.ActionCount);
            Assert.AreEqual(2, counter);
            await transaction.DisposeAsync();
            Assert.IsTrue(transaction.IsCommitted);
            Assert.AreEqual(0, transaction.ActionCount);
            Assert.AreEqual(1, counter);
        }
    }
}

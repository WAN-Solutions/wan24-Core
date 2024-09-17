using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class AsyncEnumerablePagination_Tests : TestBase
    {
        [TestMethod]
        public async Task General_TestsAsync()
        {
            static async IAsyncEnumerable<int> Enumerate()
            {
                await Task.Yield();
                for (int i = 0; i < 5; i++) yield return i;
            }
            AsyncEnumerablePagination<int> pagination = new(Enumerate().GetAsyncEnumerator(), itemsPerPage: 2);
            await using (pagination)
            {

                // Page 1
                {
                    IAsyncEnumerator<int> enumerator = pagination.NextPageAsync().GetAsyncEnumerator();
                    await using (enumerator)
                    {
                        Assert.IsFalse(pagination.IsEnumerating);
                        Assert.IsTrue(await enumerator.MoveNextAsync());
                        Assert.IsTrue(pagination.IsEnumerating);
                        Assert.AreEqual(1, pagination.CurrentPage);
                        Assert.AreEqual(0, enumerator.Current);
                        Assert.IsTrue(await enumerator.MoveNextAsync());
                        Assert.AreEqual(1, enumerator.Current);
                        Assert.IsFalse(await enumerator.MoveNextAsync());
                    }
                }
                Assert.IsFalse(pagination.IsEnumerating);
                Assert.IsFalse(pagination.IsDone);

                // Page 2
                {
                    IAsyncEnumerator<int> enumerator = pagination.NextPageAsync().GetAsyncEnumerator();
                    await using (enumerator)
                    {
                        Assert.IsFalse(pagination.IsEnumerating);
                        Assert.IsTrue(await enumerator.MoveNextAsync());
                        Assert.IsTrue(pagination.IsEnumerating);
                        Assert.AreEqual(2, pagination.CurrentPage);
                        Assert.AreEqual(2, enumerator.Current);
                        Assert.IsTrue(await enumerator.MoveNextAsync());
                        Assert.AreEqual(3, enumerator.Current);
                        Assert.IsFalse(await enumerator.MoveNextAsync());
                    }
                }
                Assert.IsFalse(pagination.IsEnumerating);
                Assert.IsFalse(pagination.IsDone);

                // Page 3
                {
                    IAsyncEnumerator<int> enumerator = pagination.NextPageAsync().GetAsyncEnumerator();
                    await using (enumerator)
                    {
                        Assert.IsFalse(pagination.IsEnumerating);
                        Assert.IsTrue(await enumerator.MoveNextAsync());
                        Assert.IsTrue(pagination.IsEnumerating);
                        Assert.AreEqual(3, pagination.CurrentPage);
                        Assert.AreEqual(4, enumerator.Current);
                        Assert.IsFalse(await enumerator.MoveNextAsync());
                    }
                }
                Assert.IsFalse(pagination.IsEnumerating);
                Assert.IsTrue(pagination.IsDone);

                // The used enumerator can't move back
                await Assert.ThrowsExceptionAsync<NotSupportedException>(async () =>
                {
                    IAsyncEnumerator<int> enumerator = pagination.NextPageAsync(page: 2).GetAsyncEnumerator();
                    await using (enumerator)
                        await enumerator.MoveNextAsync();
                });
            }
        }
    }
}

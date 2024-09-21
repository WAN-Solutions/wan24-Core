using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class CachedEnumerablePagination_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            static IEnumerable<int> Enumerate()
            {
                for (int i = 0; i < 5; i++) yield return i;
            }
            using CachedEnumerablePagination<int> pagination = new(Enumerate().GetEnumerator(), itemsPerPage: 2);

            // Page 1
            using (IEnumerator<int> enumerator = pagination.NextPage().GetEnumerator())
            {
                Assert.IsFalse(pagination.IsEnumerating);
                Assert.IsTrue(enumerator.MoveNext());
                Assert.IsTrue(pagination.IsEnumerating);
                Assert.AreEqual(1, pagination.CurrentPage);
                Assert.AreEqual(0, enumerator.Current);
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(1, enumerator.Current);
                Assert.IsFalse(enumerator.MoveNext());
            }
            Assert.IsFalse(pagination.IsEnumerating);
            Assert.IsFalse(pagination.IsDone);
            Assert.IsFalse(pagination.FullyEnumerated);

            // Page 2
            using (IEnumerator<int> enumerator = pagination.NextPage().GetEnumerator())
            {
                Assert.IsFalse(pagination.IsEnumerating);
                Assert.IsTrue(enumerator.MoveNext());
                Assert.IsTrue(pagination.IsEnumerating);
                Assert.AreEqual(2, pagination.CurrentPage);
                Assert.AreEqual(2, enumerator.Current);
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(3, enumerator.Current);
                Assert.IsFalse(enumerator.MoveNext());
            }
            Assert.IsFalse(pagination.IsEnumerating);
            Assert.IsFalse(pagination.IsDone);
            Assert.IsFalse(pagination.FullyEnumerated);

            // Page 3
            using (IEnumerator<int> enumerator = pagination.NextPage().GetEnumerator())
            {
                Assert.IsFalse(pagination.IsEnumerating);
                Assert.IsTrue(enumerator.MoveNext());
                Assert.IsTrue(pagination.IsEnumerating);
                Assert.AreEqual(3, pagination.CurrentPage);
                Assert.AreEqual(4, enumerator.Current);
                Assert.IsFalse(enumerator.MoveNext());
            }
            Assert.IsFalse(pagination.IsEnumerating);
            Assert.IsTrue(pagination.IsDone);
            Assert.IsTrue(pagination.FullyEnumerated);

            // Move back to page 2
            using (IEnumerator<int> enumerator = pagination.NextPage(page: 2).GetEnumerator())
            {
                Assert.IsFalse(pagination.IsEnumerating);
                Assert.IsTrue(enumerator.MoveNext());
                Assert.IsTrue(pagination.IsEnumerating);
                Assert.AreEqual(2, pagination.CurrentPage);
                Assert.AreEqual(2, enumerator.Current);
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(3, enumerator.Current);
                Assert.IsFalse(enumerator.MoveNext());
            }
            Assert.IsFalse(pagination.IsEnumerating);
            Assert.IsFalse(pagination.IsDone);
            Assert.IsTrue(pagination.FullyEnumerated);
        }
    }
}

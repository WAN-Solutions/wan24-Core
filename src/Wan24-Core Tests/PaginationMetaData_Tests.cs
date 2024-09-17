using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class PaginationMetaData_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            PaginationMetaData data = new(count: 14, itemsPerPage: 5, page: 2);

            Assert.AreEqual(14, data.Count);
            Assert.AreEqual(5, data.ItemsPerPage);
            Assert.AreEqual(2, data.Page);
            Assert.AreEqual(5, data.ItemsOnPage);

            Assert.AreEqual(3, data.TotalPages);
            Assert.IsTrue(data.HasPreviousPage);
            Assert.IsTrue(data.HasNextPage);
            Assert.AreEqual(5, data.FirstItemIndex);
            Assert.AreEqual(10, data.LastItemIndex);

            Assert.AreEqual(1, data.PreviousPage());
            Assert.IsFalse(data.HasPreviousPage);
            Assert.IsTrue(data.HasNextPage);
            Assert.AreEqual(0, data.FirstItemIndex);
            Assert.AreEqual(5, data.LastItemIndex);
            Assert.AreEqual(5, data.ItemsOnPage);

            data.Page = 3;
            Assert.IsTrue(data.HasPreviousPage);
            Assert.IsFalse(data.HasNextPage);
            Assert.AreEqual(10, data.FirstItemIndex);
            Assert.AreEqual(13, data.LastItemIndex);
            Assert.AreEqual(4, data.ItemsOnPage);

            Assert.AreEqual(1, data.FirstPage());
            Assert.ThrowsException<InvalidOperationException>(() => data.PreviousPage());
            Assert.AreEqual(data.TotalPages, data.LastPage());
            Assert.ThrowsException<InvalidOperationException>(() => data.NextPage());

            byte[] serialized = data;
            Assert.AreEqual(PaginationMetaData.STRUCTURE_SIZE, serialized.Length);
            data = serialized;
            Assert.AreEqual(14, data.Count);
            Assert.AreEqual(5, data.ItemsPerPage);
            Assert.AreEqual(3, data.Page);
            Assert.AreEqual(4, data.ItemsOnPage);

            string str = data.ToString();
            data = PaginationMetaData.Parse(str);
            Assert.AreEqual(14, data.Count);
            Assert.AreEqual(5, data.ItemsPerPage);
            Assert.AreEqual(3, data.Page);
            Assert.AreEqual(4, data.ItemsOnPage);

            Assert.IsTrue(PaginationMetaData.TryParse(str, out data));
            Assert.AreEqual(14, data.Count);
            Assert.AreEqual(5, data.ItemsPerPage);
            Assert.AreEqual(3, data.Page);
            Assert.AreEqual(4, data.ItemsOnPage);
        }
    }
}

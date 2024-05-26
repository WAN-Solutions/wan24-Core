using System.Text;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ExceptionExtensions_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            Exception ex = new();
            ex = ex.Append(new Exception());
            AggregateException? aex = ex as AggregateException;
            Assert.IsNotNull(aex);
            Assert.AreEqual(2, aex.InnerExceptions.Count);
            aex = ex.Append(new Exception());
            Assert.AreEqual(3, aex.InnerExceptions.Count);
        }
    }
}

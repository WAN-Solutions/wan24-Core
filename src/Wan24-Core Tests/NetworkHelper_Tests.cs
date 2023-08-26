using System.Net;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class NetworkHelper_Tests
    {
        [TestMethod]
        public void SubNet_Tests()
        {
            IPAddress ipV4 = IPAddress.Loopback,
                ipV6 = IPAddress.IPv6Loopback,
                lan = IPAddress.Parse("192.168.0.1"),
                wan = IPAddress.Parse("8.8.8.8");

            Assert.IsTrue(NetworkHelper.IsLoopBack(ipV4));
            Assert.IsFalse(NetworkHelper.IsLan(ipV4));
            Assert.IsFalse(NetworkHelper.IsWan(ipV4));

            Assert.IsTrue(NetworkHelper.IsLoopBack(ipV6));
            Assert.IsFalse(NetworkHelper.IsLan(ipV6));
            Assert.IsFalse(NetworkHelper.IsWan(ipV6));

            Assert.IsFalse(NetworkHelper.IsLoopBack(lan));
            Assert.IsTrue(NetworkHelper.IsLan(lan));
            Assert.IsFalse(NetworkHelper.IsWan(lan));

            Assert.IsFalse(NetworkHelper.IsLoopBack(wan));
            Assert.IsFalse(NetworkHelper.IsLan(wan));
            Assert.IsTrue(NetworkHelper.IsWan(wan));
        }
    }
}

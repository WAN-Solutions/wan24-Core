using System.Net.Sockets;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class IpSubNets_Tests
    {
        [TestMethod]
        public void LoopBack_Lan_Wan_Tests()
        {
            Assert.IsTrue(NetworkHelper.LoopBack.IsLoopback);
            Assert.IsFalse(NetworkHelper.LoopBack.IsLan);
            Assert.IsFalse(NetworkHelper.LoopBack.IsWan);

            Assert.IsFalse(NetworkHelper.LAN.IsLoopback);
            Assert.IsTrue(NetworkHelper.LAN.IsLan);
            Assert.IsFalse(NetworkHelper.LAN.IsWan);

            IpSubNets nets = new(NetworkHelper.LoopBack.ToArray());
            Assert.IsTrue(nets.IsLoopback);
            Assert.IsFalse(nets.IsLan);
            Assert.IsFalse(nets.IsWan);

            nets = new(NetworkHelper.LAN.ToArray());
            Assert.IsFalse(nets.IsLoopback);
            Assert.IsTrue(nets.IsLan);
            Assert.IsFalse(nets.IsWan);

            nets = new(new IpSubNet("8.8.8.8/32"));
            Assert.IsFalse(nets.IsLoopback);
            Assert.IsFalse(nets.IsLan);
            Assert.IsTrue(nets.IsWan);
        }

        [TestMethod]
        public void NetworkKind_Tests()
        {
            Assert.AreEqual(IpNetworkKind.Loopback, NetworkHelper.LoopBack.NetworkKind);
            Assert.AreEqual(IpNetworkKind.LAN, NetworkHelper.LAN.NetworkKind);

            IpSubNets nets = new(new IpSubNet("8.8.8.8/32"));
            Assert.AreEqual(IpNetworkKind.WAN, nets.NetworkKind);

            nets += NetworkHelper.LoopBack;
            Assert.AreEqual(IpNetworkKind.WAN | IpNetworkKind.Loopback, nets.NetworkKind);

            nets += NetworkHelper.LAN;
            Assert.AreEqual(IpNetworkKind.ALL, nets.NetworkKind);
        }

        [TestMethod]
        public void AddressFamily_Tests()
        {
            IpSubNets nets = new(IpSubNet.LoopbackIPv4);
            Assert.AreEqual(AddressFamily.InterNetwork, nets.AddressFamily);

            nets = new(IpSubNet.LoopbackIPv6);
            Assert.AreEqual(AddressFamily.InterNetworkV6, nets.AddressFamily);

            nets += IpSubNet.LoopbackIPv4;
            Assert.AreEqual(AddressFamily.Unspecified, nets.AddressFamily);
        }
    }
}

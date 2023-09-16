using System.Net;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class IpSubNet_Tests : TestBase
    {
        private static readonly Dictionary<string, string[]> ValidTestData = new()
            {
                {"192.168.5.85/24", new string[] {"192.168.5.1", "192.168.5.254" } },
                {"10.128.240.50/30", new string[] {"10.128.240.48", "10.128.240.49", "10.128.240.50", "10.128.240.51" } },
                {"192.168.5.85/0", new string[] {"0.0.0.0", "255.255.255.255" } },
                {"2001:db8:abcd:0012::0/64", new string[] {"2001:0DB8:ABCD:0012:0000:0000:0000:0000", "2001:0DB8:ABCD:0012:FFFF:FFFF:FFFF:FFFF", "2001:0DB8:ABCD:0012:0001:0000:0000:0000", "2001:0DB8:ABCD:0012:FFFF:FFFF:FFFF:FFF0" } },
                {"2001:db8:abcd:0012::0/128", new string[] {"2001:0DB8:ABCD:0012:0000:0000:0000:0000" } },
                {"2001:db8:abcd:5678::0/53", new string[] {"2001:0db8:abcd:5000:0000:0000:0000:0000", "2001:0db8:abcd:57ff:ffff:ffff:ffff:ffff" } },
                {"2001:db8:abcd:0012::0/0", new string[] {"::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff" } }
            };
        private static readonly Dictionary<string, string[]> InvalidTestData = new()
            {
                {"192.168.5.85/24", new string[] {"192.168.4.254", "191.168.5.254" } },
                {"10.128.240.50/30", new string[] {"10.128.240.47", "10.128.240.52", "10.128.239.50", "10.127.240.51" } },
                {"2001:db8:abcd:0012::0/64", new string[] {"2001:0DB8:ABCD:0011:FFFF:FFFF:FFFF:FFFF", "2001:0DB8:ABCD:0013:0000:0000:0000:0000", "2001:0DB8:ABCD:0013:0001:0000:0000:0000", "2001:0DB8:ABCD:0011:FFFF:FFFF:FFFF:FFF0" } },
                {"2001:db8:abcd:0012::0/128", new string[] {"2001:0DB8:ABCD:0012:0000:0000:0000:0001" } },
                {"2001:db8:abcd:5678::0/53", new string[] {"2001:0db8:abcd:4999:0000:0000:0000:0000", "2001:0db8:abcd:5800:0000:0000:0000:0000" } }
            };

        [TestMethod]
        public void General_Tests()
        {
            IpSubNet net;
            foreach (KeyValuePair<string, string[]> nets in ValidTestData)
            {
                net = new(nets.Key);
                Logging.WriteInfo($"Valid test {nets.Key} (mask {net.MaskIPAddress}, {(net.IsIPv4 ? $"broadcast {net.BroadcastIPAddress}" : "IPv6")}, gateway {net.MaskedNetworkIPAddress}, addresses {net.UsableIPAddressCount}/{net.IPAddressCount}, first {net.FirstUsable}, last {net.LastUsable})");
                foreach (string ip in nets.Value)
                {
                    Logging.WriteInfo($"\tIP {ip}");
                    Assert.IsTrue(net.Includes(IPAddress.Parse(ip)));
                }
            }
            foreach (KeyValuePair<string, string[]> nets in InvalidTestData)
            {
                net = new(nets.Key);
                Logging.WriteInfo($"Invalid test {nets.Key} (mask {net.MaskIPAddress}, {(net.IsIPv4 ? $"broadcast {net.BroadcastIPAddress}" : "IPv6")}, gateway {net.MaskedNetworkIPAddress}, addresses {net.UsableIPAddressCount}/{net.IPAddressCount}, first {net.FirstUsable}, last {net.LastUsable})");
                foreach (string ip in nets.Value)
                {
                    Logging.WriteInfo($"\tIP {ip}");
                    Assert.IsFalse(net.Includes(IPAddress.Parse(ip)));
                }
            }
        }

        [TestMethod]
        public void TryParse_Tests()
        {
            foreach (string key in ValidTestData.Keys)
            {
                Logging.WriteInfo($"Sub-net {key}");
                Assert.IsTrue(IpSubNet.TryParse(key, out IpSubNet net));
                Assert.AreEqual(new IpSubNet(key), net);
            }
            Assert.IsFalse(IpSubNet.TryParse("test", out _));
            Assert.IsFalse(IpSubNet.TryParse("123/123", out _));
            Assert.IsFalse(IpSubNet.TryParse("123.123.123.123/abc", out _));
            Assert.IsFalse(IpSubNet.TryParse("123.123.123.123/129", out _));
            Assert.IsFalse(IpSubNet.TryParse("123.123.123.123/-1", out _));
        }

        [TestMethod]
        public void ConstructIPs_Tests()
        {
            IpSubNet net,
                net2;
            foreach(string key in ValidTestData.Keys)
            {
                Logging.WriteInfo($"Sub-net {key}");
                net = new(key);
                net2 = new(net.NetworkIPAddress, net.MaskIPAddress);
                Assert.AreEqual(net, net2);
            }
        }

        [TestMethod]
        public void ConstructIpAndBits_Tests()
        {
            IpSubNet net,
                net2;
            foreach (string key in ValidTestData.Keys)
            {
                Logging.WriteInfo($"Sub-net {key}");
                net = new(key);
                net2 = new(net.NetworkIPAddress, net.MaskBits);
                Assert.AreEqual(net, net2);
            }
        }

        [TestMethod]
        public void ConstructNumeric_Tests()
        {
            IpSubNet net,
                net2;
            foreach (string key in ValidTestData.Keys)
            {
                Logging.WriteInfo($"Sub-net {key}");
                net = new(key);
                net2 = net.IsIPv4 ? new((uint)net.Network, net.MaskBits) : new(net.Network, net.MaskBits);
                Assert.AreEqual(net, net2);
            }
        }

        [TestMethod]
        public void Serialization_Test()
        {
            IpSubNet net,
                net2;
            byte[] data;
            foreach (string key in ValidTestData.Keys)
            {
                Logging.WriteInfo($"Sub-net {key}");
                net = new(key);
                data = net;
                Assert.AreEqual(net.IsIPv4 ? IpSubNet.IPV4_STRUCTURE_SIZE : IpSubNet.IPV6_STRUCTURE_SIZE, data.Length);
                net2 = new(data);
                Assert.AreEqual(net, net2);
            }
        }

        [TestMethod]
        public void RangeBorders_Tests()
        {
            IpSubNet net;
            foreach (string key in ValidTestData.Keys)
            {
                Logging.WriteInfo($"Sub-net {key}");
                net = new(key);
                Assert.IsTrue(net == net.MaskedNetworkIPAddress);
                if (net.IsIPv4) Assert.IsTrue(net == net.BroadcastIPAddress);
                Assert.IsTrue(net == net.FirstUsable);
                Assert.IsTrue(net == net.LastUsable);
            }
        }

        [TestMethod]
        public void LanWanLoopback_Tests()
        {
            // Loopback
            Assert.IsTrue(IpSubNet.LoopbackIPv4.IsLoopback);
            Assert.IsTrue(IpSubNet.LoopbackIPv6.IsLoopback);
            Assert.IsFalse(IpSubNet.LoopbackIPv4.IsLan);
            Assert.IsFalse(IpSubNet.LoopbackIPv6.IsLan);
            Assert.IsFalse(IpSubNet.LoopbackIPv4.IsWan);
            Assert.IsFalse(IpSubNet.LoopbackIPv6.IsWan);

            // LAN / WAN
            IpSubNet lan = new("192.168.0.1/24"),
                wan = new("8.8.8.8/32");
            Assert.IsFalse(lan.IsLoopback);
            Assert.IsTrue(lan.IsLan);
            Assert.IsFalse(lan.IsWan);
            Assert.IsFalse(wan.IsLoopback);
            Assert.IsFalse(wan.IsLan);
            Assert.IsTrue(wan.IsWan);
        }

        [TestMethod]
        public void Combine_Tests()
        {
            {
                IpSubNet a = new("10.1.2.0/8"),
                    b = new("192.168.0.0/16"),
                    c = a.CombineWith(b);
                Logging.WriteInfo($"Sub-net {a} + {b} = {c}");
                Assert.AreEqual(0, c.MaskBits);
                Assert.AreEqual(IPAddress.Parse("0.0.0.0"), c.NetworkIPAddress);
            }
            {
                IpSubNet a = new("192.168.128.0/24"),
                    b = new("192.168.254.0/16"),
                    c = a.CombineWith(b);
                Logging.WriteInfo($"Sub-net {a} + {b} = {c}");
                Assert.AreEqual(16, c.MaskBits);
                Assert.AreEqual(IPAddress.Parse("192.168.0.0"), c.NetworkIPAddress);
            }
        }

        [TestMethod]
        public void Network_Constructor_Tests()
        {
            {
                IPAddress network = IPAddress.Parse("0.0.0.0");
                IpSubNet net = new(network);
                Assert.AreEqual(network, net.NetworkIPAddress);
                Assert.AreEqual(0, net.MaskBits);
            }
            {
                IPAddress network = IPAddress.Parse("192.168.0.0");
                IpSubNet net = new(network);
                Assert.AreEqual(network, net.NetworkIPAddress);
                Assert.AreEqual(16, net.MaskBits);
            }
            {
                IPAddress network = IPAddress.Parse("192.168.2.0");
                IpSubNet net = new(network, countAllBits: true);
                Assert.AreEqual(network, net.NetworkIPAddress);
                Assert.AreEqual(23, net.MaskBits);
            }
            {
                IPAddress network = IPAddress.Parse("192.168.2.1");
                IpSubNet net = new(network, countAllBits: true);
                Assert.AreEqual(network, net.NetworkIPAddress);
                Assert.AreEqual(32, net.MaskBits);
            }
        }
    }
}

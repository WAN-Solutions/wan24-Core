using System.Net;
using wan24.Core;

//TODO Write more tests

namespace Wan24_Core_Tests
{
    [TestClass]
    public class IpSubNet_Tests
    {
        private static readonly Dictionary<string, string[]> ValidTestData = new Dictionary<string, string[]>()
            {
                {"192.168.5.85/24", new string[] {"192.168.5.1", "192.168.5.254" } },
                {"10.128.240.50/30", new string[] {"10.128.240.48", "10.128.240.49", "10.128.240.50", "10.128.240.51" } },
                {"192.168.5.85/0", new string[] {"0.0.0.0", "255.255.255.255" } },
                {"2001:db8:abcd:0012::0/64", new string[] {"2001:0DB8:ABCD:0012:0000:0000:0000:0000", "2001:0DB8:ABCD:0012:FFFF:FFFF:FFFF:FFFF", "2001:0DB8:ABCD:0012:0001:0000:0000:0000", "2001:0DB8:ABCD:0012:FFFF:FFFF:FFFF:FFF0" } },
                {"2001:db8:abcd:0012::0/128", new string[] {"2001:0DB8:ABCD:0012:0000:0000:0000:0000" } },
                {"2001:db8:abcd:5678::0/53", new string[] {"2001:0db8:abcd:5000:0000:0000:0000:0000", "2001:0db8:abcd:57ff:ffff:ffff:ffff:ffff" } },
                {"2001:db8:abcd:0012::0/0", new string[] {"::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff" } }
            };
        private static readonly Dictionary<string, string[]> InvalidTestData = new Dictionary<string, string[]>()
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
                Logging.WriteInfo($"Valid test {nets.Key}");
                foreach (string ip in nets.Value)
                {
                    Logging.WriteInfo($"\tIP {ip}");
                    Assert.IsTrue(net.DoesMatch(IPAddress.Parse(ip)));
                }
            }
            foreach (KeyValuePair<string, string[]> nets in InvalidTestData)
            {
                net = new(nets.Key);
                Logging.WriteInfo($"Invalid test {nets.Key}");
                foreach (string ip in nets.Value)
                {
                    Logging.WriteInfo($"\tIP {ip}");
                    Assert.IsFalse(net.DoesMatch(IPAddress.Parse(ip)));
                }
            }
        }

        [TestMethod]
        public void TryParse_Tests()
        {
            IpSubNet net;
            foreach (string key in ValidTestData.Keys)
            {
                Logging.WriteInfo($"Sub-net {key}");
                Assert.IsTrue(IpSubNet.TryParse(key, out net));
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
    }
}

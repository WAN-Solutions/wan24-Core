using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class MacAddress_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            Assert.AreEqual(0u, MacAddress.Zero.Address);
            Assert.IsFalse(MacAddress.Zero.IsBroadcast);
            Assert.IsFalse(MacAddress.Zero.IsIPv4Multicast);
            Assert.IsFalse(MacAddress.Zero.IsIPv6Multicast);

            Assert.AreEqual(MacAddress.BROADCAST, MacAddress.Broadcast.Address);
            Assert.IsTrue(MacAddress.Broadcast.IsBroadcast);
            Assert.IsTrue(MacAddress.Parse("ff:ff:ff:ff:ff:ff").IsBroadcast);

            Assert.IsTrue(MacAddress.Parse("01-00-5E-00-00-00").IsIPv4Multicast);
            Assert.IsTrue(MacAddress.Parse("01-00-5E-7f-ff-ff").IsIPv4Multicast);
            Assert.IsFalse(MacAddress.Parse("01-00-5E-00-00-00").IsIPv6Multicast);
            Assert.IsFalse(MacAddress.Parse("01-00-5E-7f-ff-ff").IsIPv6Multicast);

            Assert.IsTrue(MacAddress.Parse("33-33-00-00-00-00").IsIPv6Multicast);
            Assert.IsTrue(MacAddress.Parse("33-33-FF-FF-FF-FF").IsIPv6Multicast);
            Assert.IsFalse(MacAddress.Parse("33-33-00-00-00-00").IsIPv4Multicast);
            Assert.IsFalse(MacAddress.Parse("33-33-FF-FF-FF-FF").IsIPv4Multicast);

            Assert.IsTrue(MacAddress.Parse("01-00-0C-CC-CC-CC").IsGroup);
            Assert.IsFalse(MacAddress.Parse("01-00-0C-CC-CC-CC").IsIndividual);

            Assert.IsFalse(MacAddress.Parse("00-00-0C-CC-CC-CC").IsGroup);
            Assert.IsTrue(MacAddress.Parse("00-00-0C-CC-CC-CC").IsIndividual);

            Assert.IsTrue(new MacAddress([0x02, 0, 0x0c, 0, 0xcc, 0xcc]).IsLocal);
            Assert.IsTrue(new MacAddress([0x06, 0, 0x0c, 0, 0xcc, 0xcc]).IsLocal);
            Assert.IsTrue(new MacAddress([0x0A, 0, 0x0c, 0, 0xcc, 0xcc]).IsLocal);
            Assert.IsTrue(new MacAddress([0x0E, 0, 0x0c, 0, 0xcc, 0xcc]).IsLocal);

            Assert.IsFalse(new MacAddress([0x02, 0, 0x0c, 0, 0xcc, 0xcc]).IsUniversal);
            Assert.IsFalse(new MacAddress([0x06, 0, 0x0c, 0, 0xcc, 0xcc]).IsUniversal);
            Assert.IsFalse(new MacAddress([0x0A, 0, 0x0c, 0, 0xcc, 0xcc]).IsUniversal);
            Assert.IsFalse(new MacAddress([0x0E, 0, 0x0c, 0, 0xcc, 0xcc]).IsUniversal);

            Assert.IsFalse(new MacAddress([0x01, 0, 0x0c, 0xcc, 0xcc, 0xcc]).IsLocal);
            Assert.IsTrue(new MacAddress([0x01, 0, 0x0c, 0xcc, 0xcc, 0xcc]).IsUniversal);

            MacAddress mac1 = MacAddress.Parse("01-00-5e-00-00-00"),
                mac2 = new(mac1.Address),
                mac3 = new(mac1.GetBytes()),
                mac4 = MacAddress.Parse(mac1.ToString()),
                mac5 = new(mac1.ToPhysicalAddress());
            Assert.AreEqual(mac1, mac2);
            Assert.AreEqual(mac2, mac3);
            Assert.AreEqual(mac3, mac4);
            Assert.AreEqual(mac4, mac5);
            Assert.AreEqual("01:00:5e:00:00:00", mac3.ToString());
            Assert.AreEqual(mac1.ToPhysicalAddress().ToString().ToLower(), mac1.ToString(delimiter: null));

            Assert.IsTrue(mac1.OUI >= 0);
            Assert.IsTrue(mac1.VendorId == 0);
        }
    }
}

using BenchmarkDotNet.Attributes;
using System.Net;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class IpSubNet_Tests
    {
        private const string IPV4_SUBNET = "127.0.0.1/32";
        private const string IPV6_SUBNET = "::1/128";

        private static readonly IPAddress IpV4 = IpSubNet.LoopbackIPv4.NetworkIPAddress;
        private static readonly int IpV4Cidr = IpSubNet.LoopbackIPv4.MaskBits;
        private static readonly byte IpV4Cidr2 = IpSubNet.LoopbackIPv4.MaskBits;

        private static readonly IPAddress IpV6 = IpSubNet.LoopbackIPv6.NetworkIPAddress;
        private static readonly int IpV6Cidr = IpSubNet.LoopbackIPv6.MaskBits;
        private static readonly byte IpV6Cidr2 = IpSubNet.LoopbackIPv6.MaskBits;

        private static readonly IpSubNet Wan24LoopbackV4 = IpSubNet.Parse(IPV4_SUBNET);
        private static readonly IPNetwork2 IPNetwork2LoopbackV4 = IPNetwork2.Parse(IPV4_SUBNET);
        private static readonly IPNetwork NetLoopbackV4 = IPNetwork.Parse(IPV4_SUBNET);

        private static readonly IpSubNet Wan24LoopbackV6 = IpSubNet.Parse(IPV6_SUBNET);
        private static readonly IPNetwork2 IPNetwork2LoopbackV6 = IPNetwork2.Parse(IPV6_SUBNET);
        private static readonly IPNetwork NetLoopbackV6 = IPNetwork.Parse(IPV6_SUBNET);

        [Benchmark]
        public void Wan24_V4_Constr() => new IpSubNet(IpV4, IpV4Cidr);

        [Benchmark]
        public void IPNetwork2_V4_Constr() => new IPNetwork2(IpV4, IpV4Cidr2);

        [Benchmark]
        public void Net_V4_Constr() => new IPNetwork(IpV4, IpV4Cidr);

        [Benchmark]
        public void Wan24_V6_Constr() => new IpSubNet(IpV6, IpV6Cidr);

        [Benchmark]
        public void IPNetwork2_V6_Constr() => new IPNetwork2(IpV6, IpV6Cidr2);

        [Benchmark]
        public void Net_V6_Constr() => new IPNetwork(IpV6, IpV6Cidr);

        [Benchmark]
        public void Wan24_V4_Parse() => IpSubNet.Parse(IPV4_SUBNET);

        [Benchmark]
        public void IPNetwork2_V4_Parse() => IPNetwork2.Parse(IPV4_SUBNET);

        [Benchmark]
        public void Net_V4_Parse() => IPNetwork.Parse(IPV4_SUBNET);

        [Benchmark]
        public void Wan24_V6_Parse() => IpSubNet.Parse(IPV6_SUBNET);

        [Benchmark]
        public void IPNetwork2_V6_Parse() => IPNetwork2.Parse(IPV6_SUBNET);

        [Benchmark]
        public void Net_V6_Parse() => IPNetwork.Parse(IPV6_SUBNET);

        [Benchmark]
        public void Wan24_V4_TryParse() => IpSubNet.TryParse(IPV4_SUBNET, out _);

        [Benchmark]
        public void IPNetwork2_V4_TryParse() => IPNetwork2.TryParse(IPV4_SUBNET, out _);

        [Benchmark]
        public void Net_V4_TryParse() => IPNetwork.TryParse(IPV4_SUBNET, out _);

        [Benchmark]
        public void Wan24_V6_TryParse() => IpSubNet.TryParse(IPV6_SUBNET, out _);

        [Benchmark]
        public void IPNetwork2_V6_TryParse() => IPNetwork2.TryParse(IPV6_SUBNET, out _);

        [Benchmark]
        public void Net_V6_TryParse() => IPNetwork.TryParse(IPV6_SUBNET, out _);

        [Benchmark]
        public void Wan24_V4_Contains() => Wan24LoopbackV4.Includes(IPAddress.Loopback);

        [Benchmark]
        public void IPNetwork2_V4_Contains() => IPNetwork2LoopbackV4.Contains(IPAddress.Loopback);

        [Benchmark]
        public void Net_V4_Contains() => NetLoopbackV4.Contains(IPAddress.Loopback);

        [Benchmark]
        public void Wan24_V6_Contains() => Wan24LoopbackV6.Includes(IPAddress.IPv6Loopback);

        [Benchmark]
        public void IPNetwork2_V6_Contains() => IPNetwork2LoopbackV6.Contains(IPAddress.IPv6Loopback);

        [Benchmark]
        public void Net_V6_Contains() => NetLoopbackV6.Contains(IPAddress.IPv6Loopback);
    }
}

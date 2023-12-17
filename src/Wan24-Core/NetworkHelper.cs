using System.Buffers.Binary;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Network helper
    /// </summary>
    public static class NetworkHelper
    {
        /// <summary>
        /// LAN sub-nets
        /// </summary>
        public static readonly IpSubNets LAN = new(
        [
            new("10.0.0.0/8"),
            new("172.16.0.0/12"),
            new("192.168.0.0/16"),
            new("fd00::/8")
        ], isLoopBack: false, isLan: true);

        /// <summary>
        /// Loopback sub-nets
        /// </summary>
        public static readonly IpSubNets LoopBack = new(
        [
            IpSubNet.LoopbackIPv4,
            IpSubNet.LoopbackIPv6
        ], isLoopBack: true, isLan: false);

        /// <summary>
        /// Get all (real) online ethernet adapters
        /// </summary>
        /// <returns>Ethernet adapters which are online</returns>
        public static IEnumerable<NetworkInterface> GetOnlineEthernetAdapters()
            => from adapter in NetworkInterface.GetAllNetworkInterfaces()
               where adapter.OperationalStatus == OperationalStatus.Up &&
                   adapter.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                   adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback
               select adapter;

        /// <summary>
        /// Get all (real) online ethernet adapters and have a WAN IP address
        /// </summary>
        /// <returns>Ethernet adapters which are online</returns>
        public static IEnumerable<NetworkInterface> GetOnlineWanEthernetAdapters()
            => from adapter in GetOnlineEthernetAdapters()
               where adapter.GetIPProperties().UnicastAddresses.Any(ip => LoopBack != ip.Address && LAN != ip.Address)
               select adapter;

        /// <summary>
        /// Get all (real) online ethernet adapters and have a LAN IP address
        /// </summary>
        /// <returns>Ethernet adapters which are online</returns>
        public static IEnumerable<NetworkInterface> GetOnlineLanEthernetAdapters()
            => from adapter in GetOnlineEthernetAdapters()
               where adapter.GetIPProperties().UnicastAddresses.Any(ip => LoopBack == ip.Address || LAN == ip.Address)
               select adapter;

        /// <summary>
        /// Determine if an IP address is a LAN IP address
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <returns>If the IP address is a LAN IP address</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsLan(this IPAddress ip) => LAN == ip;

        /// <summary>
        /// Determine if an IP address is a loopback IP address
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <returns>If the IP address is a loopback IP address</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsLoopBack(this IPAddress ip) => LoopBack == ip;

        /// <summary>
        /// Determine if an IP address is a WAN IP address
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <returns>If the IP address is a WAN IP address</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsWan(this IPAddress ip) => LoopBack != ip && LAN != ip;

        /// <summary>
        /// Get all IP addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetIPAddresses(this NetworkInterface adapter)
            => from ip in adapter.GetIPProperties().UnicastAddresses
               where ip.Address.AddressFamily == AddressFamily.InterNetwork ||
                ip.Address.AddressFamily == AddressFamily.InterNetworkV6
               select ip.Address;

        /// <summary>
        /// Get all IP addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetIPV4Addresses(this NetworkInterface adapter)
            => from ip in adapter.GetIPProperties().UnicastAddresses
               where ip.Address.AddressFamily == AddressFamily.InterNetwork
               select ip.Address;

        /// <summary>
        /// Get all IP addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetIPV6Addresses(this NetworkInterface adapter)
            => from ip in adapter.GetIPProperties().UnicastAddresses
               where ip.Address.AddressFamily == AddressFamily.InterNetworkV6
               select ip.Address;

        /// <summary>
        /// Get all LAN IP addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetLanIPAddresses(this NetworkInterface adapter)
            => from ip in GetIPAddresses(adapter)
               where LAN == ip
               select ip;

        /// <summary>
        /// Get all LAN IPv4 addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetLanIPv4Addresses(this NetworkInterface adapter)
            => from ip in GetIPV4Addresses(adapter)
               where LAN == ip
               select ip;

        /// <summary>
        /// Get all LAN IPv6 addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetLanIPv6Addresses(this NetworkInterface adapter)
            => from ip in GetIPV6Addresses(adapter)
               where LAN == ip
               select ip;

        /// <summary>
        /// Get all WAN IP addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetWanIPAddresses(this NetworkInterface adapter)
            => from ip in GetIPAddresses(adapter)
               where LoopBack != ip && LAN != ip
               select ip;

        /// <summary>
        /// Get all WAN IPv4 addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetWanIPv4Addresses(this NetworkInterface adapter)
            => from ip in GetIPV4Addresses(adapter)
               where LoopBack != ip && LAN != ip
               select ip;

        /// <summary>
        /// Get all WAN IPv6 addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetWanIPv6Addresses(this NetworkInterface adapter)
            => from ip in GetIPV6Addresses(adapter)
               where LoopBack != ip && LAN != ip
               select ip;

        /// <summary>
        /// Get all (real) online ethernet adapter IP addresses
        /// </summary>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetOnlineIPAddresses()
            => from adapter in GetOnlineEthernetAdapters()
               from ip in adapter.GetIPProperties().UnicastAddresses
               where ip.Address.AddressFamily == AddressFamily.InterNetwork ||
                ip.Address.AddressFamily == AddressFamily.InterNetworkV6
               select ip.Address;

        /// <summary>
        /// Get all (real) online ethernet adapter IPv4 addresses
        /// </summary>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetOnlineIPv4Addresses()
            => from adapter in GetOnlineEthernetAdapters()
               from ip in adapter.GetIPProperties().UnicastAddresses
               where ip.Address.AddressFamily == AddressFamily.InterNetwork
               select ip.Address;

        /// <summary>
        /// Get all (real) online ethernet adapter IPv6 addresses
        /// </summary>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetOnlineIPv6Addresses()
            => from adapter in GetOnlineEthernetAdapters()
               from ip in adapter.GetIPProperties().UnicastAddresses
               where ip.Address.AddressFamily == AddressFamily.InterNetworkV6
               select ip.Address;

        /// <summary>
        /// Get all (real) online ethernet adapter LAN IP addresses
        /// </summary>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetOnlineLanIPAddresses()
            => from ip in GetOnlineIPAddresses()
               where LAN == ip
               select ip;

        /// <summary>
        /// Get all (real) online ethernet adapter LAN IPv4 addresses
        /// </summary>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetOnlineLanIPv4Addresses()
            => from ip in GetOnlineIPv4Addresses()
               where ip.AddressFamily == AddressFamily.InterNetwork &&
                LAN == ip
               select ip;

        /// <summary>
        /// Get all (real) online ethernet adapter LAN IPv6 addresses
        /// </summary>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetOnlineLanIPv6Addresses()
            => from ip in GetOnlineIPv6Addresses()
               where ip.AddressFamily == AddressFamily.InterNetworkV6 &&
                LAN == ip
               select ip;

        /// <summary>
        /// Get all (real) online ethernet adapter WAN IP addresses
        /// </summary>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetOnlineWanIPAddresses()
            => from ip in GetOnlineIPAddresses()
               where LoopBack != ip && LAN != ip
               select ip;

        /// <summary>
        /// Get all (real) online ethernet adapter WAN IPv4 addresses
        /// </summary>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetOnlineWanIPv4Addresses()
            => from ip in GetOnlineIPv4Addresses()
               where LoopBack != ip && LAN != ip
               select ip;

        /// <summary>
        /// Get all (real) online ethernet adapter WAN IPv6 addresses
        /// </summary>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IPAddress> GetOnlineWanIPv6Addresses()
            => from ip in GetOnlineIPv6Addresses()
               where LoopBack != ip && LAN != ip
               select ip;

        /// <summary>
        /// Get the private sub-net of a private IP address
        /// </summary>
        /// <param name="ip">Private IP address (may be a loopback address)</param>
        /// <returns>Sub-net or <see langword="null"/>, if the IP address isn't private</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static IpSubNet? GetLocalSubNet(this IPAddress ip) => LoopBack.Including(ip) ?? LAN.Including(ip);

        /// <summary>
        /// Get the sub-net of an IP address
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <returns>Sub-net</returns>
        public static IpSubNet GetSubNet(this UnicastIPAddressInformation ip)
            => ip.Address.AddressFamily == AddressFamily.InterNetworkV6
                ? new(ip.Address, BitOperations.PopCount(BinaryPrimitives.ReadUInt32BigEndian(ip.IPv4Mask.GetAddressBytes())))
                : new(ip.Address, ip.IPv4Mask);

        /// <summary>
        /// Get the sub-nets of all IP addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IpSubNet> GetSubNets(this NetworkInterface adapter)
            => (from ip in adapter.GetIPProperties().UnicastAddresses
                where ip.Address.AddressFamily == AddressFamily.InterNetwork ||
                    ip.Address.AddressFamily == AddressFamily.InterNetworkV6
                select GetSubNet(ip))
                .Distinct();

        /// <summary>
        /// Get the sub-nets of all IPv4 addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IpSubNet> GetIPv4SubNets(this NetworkInterface adapter)
            => (from ip in adapter.GetIPProperties().UnicastAddresses
                where ip.Address.AddressFamily == AddressFamily.InterNetwork
                select GetSubNet(ip))
                .Distinct();

        /// <summary>
        /// Get the sub-nets of all IPv6 addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IpSubNet> GetIPv6SubNets(this NetworkInterface adapter)
            => (from ip in adapter.GetIPProperties().UnicastAddresses
                where ip.Address.AddressFamily == AddressFamily.InterNetworkV6
                select GetSubNet(ip))
                .Distinct();

        /// <summary>
        /// Get the sub-nets of all LAN IP addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IpSubNet> GetLanSubNets(this NetworkInterface adapter)
            => (from ip in adapter.GetIPProperties().UnicastAddresses
                where (ip.Address.AddressFamily == AddressFamily.InterNetwork ||
                    ip.Address.AddressFamily == AddressFamily.InterNetworkV6) &&
                    LAN == ip.Address
                select GetSubNet(ip))
                .Distinct();

        /// <summary>
        /// Get the sub-nets of all LAN IPv4 addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IpSubNet> GetIPv4LanSubNets(this NetworkInterface adapter)
            => (from ip in adapter.GetIPProperties().UnicastAddresses
                where ip.Address.AddressFamily == AddressFamily.InterNetwork &&
                    LAN == ip.Address
                select GetSubNet(ip))
                .Distinct();

        /// <summary>
        /// Get the sub-nets of all LAN IPv6 addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IpSubNet> GetIPv6LanSubNets(this NetworkInterface adapter)
            => (from ip in adapter.GetIPProperties().UnicastAddresses
                where ip.Address.AddressFamily == AddressFamily.InterNetworkV6 &&
                    LAN == ip.Address
                select GetSubNet(ip))
                .Distinct();

        /// <summary>
        /// Get the sub-nets of all WAN IP addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IpSubNet> GetWanSubNets(this NetworkInterface adapter)
            => (from ip in adapter.GetIPProperties().UnicastAddresses
                where (ip.Address.AddressFamily == AddressFamily.InterNetwork ||
                    ip.Address.AddressFamily == AddressFamily.InterNetworkV6) &&
                    LoopBack != ip.Address &&
                    LAN != ip.Address
                select GetSubNet(ip))
                .Distinct();

        /// <summary>
        /// Get the sub-nets of all WAN IPv4 addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IpSubNet> GetIPv4WanSubNets(this NetworkInterface adapter)
            => (from ip in adapter.GetIPProperties().UnicastAddresses
                where ip.Address.AddressFamily == AddressFamily.InterNetwork &&
                    LoopBack != ip.Address &&
                    LAN != ip.Address
                select GetSubNet(ip))
                .Distinct();

        /// <summary>
        /// Get the sub-nets of all WAN IPv6 addresses from an ethernet adapter
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <returns>IP addresses</returns>
        public static IEnumerable<IpSubNet> GetIPv6WanSubNets(this NetworkInterface adapter)
            => (from ip in adapter.GetIPProperties().UnicastAddresses
                where ip.Address.AddressFamily == AddressFamily.InterNetworkV6 &&
                    LoopBack != ip.Address &&
                    LAN != ip.Address
                select GetSubNet(ip))
                .Distinct();

        /// <summary>
        /// Get the broadcast IP address
        /// </summary>
        /// <param name="ip">IP address (IPv4 required)</param>
        /// <param name="mask">Network mask</param>
        /// <returns>Broadcast IP address</returns>
        public static IPAddress GetBroadcastAddress(this IPAddress ip, in IPAddress mask)
        {
            if (ip.AddressFamily != AddressFamily.InterNetwork) throw new ArgumentException("An IPv4 IP address is required", nameof(ip));
            if (mask.AddressFamily != AddressFamily.InterNetwork) throw new ArgumentException("An IPv4 IP address is required", nameof(mask));
            uint ipAddress = BinaryPrimitives.ReadUInt32BigEndian(ip.GetAddressBytes()),
                maskAddress = BinaryPrimitives.ReadUInt32BigEndian(mask.GetAddressBytes());
            RentedArrayStruct<byte> buffer = new(len: sizeof(uint));
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Span, (ipAddress & maskAddress) | ~maskAddress);
            return new(buffer.Span);
        }

        /// <summary>
        /// Get the broadcast address
        /// </summary>
        /// <param name="ip">IP address (IPv4 required)</param>
        /// <returns>Broadcast IP address</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static IPAddress GetBroadcastAddress(this UnicastIPAddressInformation ip) => GetBroadcastAddress(ip.Address, ip.IPv4Mask);

        /// <summary>
        /// Get the unicast IP address information
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <returns>Unicast IP address information</returns>
        public static UnicastIPAddressInformation? GetUnicastIP(this IPAddress ip)
            => (from adapter in NetworkInterface.GetAllNetworkInterfaces()
                from addr in adapter.GetIPProperties().UnicastAddresses
                where addr.Address == ip
                select addr)
                .FirstOrDefault();

        /// <summary>
        /// Get the network interface of an IP address
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <returns>Network interface</returns>
        public static NetworkInterface? GetInterface(this IPAddress ip)
            => (from adapter in NetworkInterface.GetAllNetworkInterfaces()
                from addr in adapter.GetIPProperties().UnicastAddresses
                where addr.Address == ip
                select adapter)
                .FirstOrDefault();

        /// <summary>
        /// Find the next free TCP port
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <returns>Next free TCP port</returns>
        public static int FindTcpPort(this IPAddress ip)
        {
            TcpListener listener = new(new IPEndPoint(ip, 0));
            listener.Start();
            try
            {
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}

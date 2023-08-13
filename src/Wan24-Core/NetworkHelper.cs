using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace wan24.Core
{
    /// <summary>
    /// Network helper
    /// </summary>
    public static class NetworkHelper
    {
        public static IEnumerable<NetworkInterface> GetEthernetAdapters()
            => from adapter in NetworkInterface.GetAllNetworkInterfaces()
               where adapter.OperationalStatus == OperationalStatus.Up &&
                   adapter.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                   adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback
               select adapter;
    }
}

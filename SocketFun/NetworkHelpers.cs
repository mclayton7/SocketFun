using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SocketFun
{
    public static class NetworkHelpers
    {
        public static IEnumerable<IPAddress> GetIpAddresses()
        {
            return (from networkInterface in NetworkInterface.GetAllNetworkInterfaces()
                where networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                      networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                from ip in networkInterface.GetIPProperties().UnicastAddresses
                where ip.Address.AddressFamily == AddressFamily.InterNetwork
                select ip.Address).ToList();
        }
    }
}
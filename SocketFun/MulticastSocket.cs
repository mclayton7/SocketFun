using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SocketFun
{
	public class MulticastSocket
    {
        private readonly IPAddress MulticastAddress;
        private readonly int Port;
        private readonly UdpClient Client;

        public MulticastSocket(IPAddress multicastAddress, int port)
        {
            MulticastAddress = multicastAddress;
            Port = port;

            Client = new UdpClient(port);

            Console.WriteLine($"==> ExclusiveAddressUse {Client.ExclusiveAddressUse} MulticastLoopback {Client.MulticastLoopback}");

            var localIpAddresses = GetIpAddresses();
            foreach(var localIpAddress in localIpAddresses)
            {
                try
                {   
                    
                    Client.JoinMulticastGroup(multicastAddress, localIpAddress);
                    Console.WriteLine($"{localIpAddress} Worked");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{localIpAddress} Did not work");
                }
            }
        }

        public void Send(byte[] bytes)
        {
            Client.Send(bytes, bytes.Length, new IPEndPoint(MulticastAddress, Port));
        }

        private List<IPAddress> GetIpAddresses()
        {
            var ipAddresses = new List<IPAddress>();

            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (var ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddresses.Add(ip.Address);
                        }
                    }
                }
            }

            return ipAddresses;
        }
	}
}
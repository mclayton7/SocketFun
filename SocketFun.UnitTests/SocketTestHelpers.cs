using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SocketFun.UnitTests
{
    public static class SocketTestHelpers
    {
        public static IPAddress LocalAddress = IPAddress.Parse("192.168.1.31");
        public static Socket CreateMulticastSocket(IPAddress multicastAddress, int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multicastAddress));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
            socket.ExclusiveAddressUse = false;
            try
            {
                socket.Bind(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? new IPEndPoint(LocalAddress, port)
                    : new IPEndPoint(IPAddress.Any, port));
            }
            catch (Exception)
            {
                Console.WriteLine($"Could not bind to {LocalAddress}:{port}");
                throw;
            }

            return socket;
        }

        public static Socket CreateLocalMulticastSocket(IPAddress multicastAddress, int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multicastAddress));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
            socket.ExclusiveAddressUse = false;
            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
            }
            catch (Exception)
            {
                Console.WriteLine($"Could not bind to {LocalAddress}:{port}");
                throw;
            }

            return socket;
        }

        public static byte[] RandomBytes(int size = 256)
        {
            var array = new byte[size];
            for (var i = 0; i < size; i++) array[i] = TestContext.CurrentContext.Random.NextByte();

            return array;
        }
    }
}
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using NUnit.Framework;

namespace SocketFun.UnitTests
{
    public static class SocketTestHelpers
    {
        public static IPAddress LocalAddress = IPAddress.Parse("192.168.1.31");
        public static Socket CreateMulticastSocket(IPAddress address, int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(address));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
            socket.ExclusiveAddressUse = false;

            socket.Bind(new IPEndPoint(LocalAddress, port));

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
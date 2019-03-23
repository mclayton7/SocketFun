using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using SocketFun;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var ipAddress = IPAddress.Parse("225.0.100.1");
            var endpoint = (EndPoint)new IPEndPoint(ipAddress, 3001);

            var uut = new MulticastSocket(ipAddress, 3001);

            var socket = CreateMulticastSocket(ipAddress, 3001);
            var bytes = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

            uut.Send(bytes);


            var buffer = new byte[4];
            socket.ReceiveFrom(buffer, ref endpoint);

            Console.WriteLine($"{buffer}");

            Assert.Fail("asdf");
        }

        private Socket CreateMulticastSocket(IPAddress address, int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);

            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(address));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
            socket.ExclusiveAddressUse = false;

            socket.Bind(new IPEndPoint(IPAddress.Parse("192.168.49.1"), port));

            return socket;
        }
    }
}
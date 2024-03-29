﻿using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;

namespace SocketFun.UnitTests
{
    public class AsyncMulticastClientWithEventsTests
    {
        private const int Port = 3002;
        private readonly IPAddress MulticastAddress = IPAddress.Parse("225.0.100.1");
        private const int PacketSize = 8192;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void WillSendPackets()
        {
            var uut = new AsyncMulticastClientWithEvents(MulticastAddress, Port);
            var socket = SocketTestHelpers.CreateMulticastSocket(MulticastAddress, Port);
            var bytes = SocketTestHelpers.RandomBytes(PacketSize);
            var buffer = new byte[PacketSize];
            var result = socket.ReceiveFromAsync(buffer, new SocketFlags(), new IPEndPoint(MulticastAddress, Port));

            uut.Send(bytes);

            result.Wait();
            Assert.That(buffer, Is.EqualTo(bytes));

            socket.Close();
            uut.Dispose();
        }

        [Test]
        public void WillReceivePackets()
        {
            var uut = new AsyncMulticastClientWithEvents(MulticastAddress, Port);
            var socket = SocketTestHelpers.CreateMulticastSocket(MulticastAddress, Port);
            var expectedBytes = SocketTestHelpers.RandomBytes(256);
            var resultingBytes = new byte[0];
            var received = false;
            uut.PacketReceived += (sender, bytes) =>
            {
                received = true;
                resultingBytes = bytes;
            };

            socket.SendTo(expectedBytes, new IPEndPoint(MulticastAddress, Port));
            Assert.That(() => received, Is.True.After(4000, 100));
            Assert.That(resultingBytes, Is.EqualTo(expectedBytes));
            socket.Close();
            uut.Dispose();
        }

        [Test]
        public void WillReceiveMultiplePackets()
        {
            var uut = new AsyncMulticastClientWithEvents(MulticastAddress, Port);
            var socket = SocketTestHelpers.CreateMulticastSocket(MulticastAddress, Port);
            var expectedBytes = SocketTestHelpers.RandomBytes(PacketSize);
            var resultingBytes = new List<byte[]>();
            uut.PacketReceived += (sender, bytes) =>
            {
                resultingBytes.Add(bytes);
            };

            socket.SendTo(expectedBytes, new IPEndPoint(MulticastAddress, Port));
            socket.SendTo(expectedBytes, new IPEndPoint(MulticastAddress, Port));

            Assert.That(() => resultingBytes.Count, Is.EqualTo(2).After(4000, 100));
            socket.Close();
            uut.Dispose();
        }
    }
}
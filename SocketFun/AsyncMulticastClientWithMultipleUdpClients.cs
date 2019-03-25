using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SocketFun
{
    internal class State
    {
        public UdpClient Client;
        public IPAddress LocalIpAddress;
    }
    public class AsyncMulticastClientWithMultipleUdpClients : IMulticastSocket
    {
        public event EventHandler<byte[]> PacketReceived;

        private IPAddress MulticastAddress { get; }
        private int Port { get; }
        private readonly List<UdpClient> Clients = new List<UdpClient>();
        private bool isQuitting = false;

        public AsyncMulticastClientWithMultipleUdpClients(IPAddress multicastAddress, int port)
        {
            MulticastAddress = multicastAddress;
            Port = port;

            var localIpAddresses = NetworkHelpers.GetOtherIpAddresses();
            foreach (var localIpAddress in localIpAddresses)
            {
                try
                {
                    //var client = new UdpClient(new IPEndPoint(localIpAddress, port));
                    var client = new UdpClient(port);
                    Console.WriteLine($"Trying to join {multicastAddress} on {localIpAddress}");
                    client.JoinMulticastGroup(multicastAddress, localIpAddress);

                    var state = new State
                    {
                        Client = client,
                        LocalIpAddress = localIpAddress
                    };
                    Clients.Add(client);

                    client.BeginReceive(ReceiveCallback, state);

                    Console.WriteLine($"{localIpAddress}:{port} Worked");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{localIpAddress}:{port} Did not work: {e}");
                }
            }
        }

        public void Send(byte[] bytes)
        {
            foreach (var client in Clients)
            {
                client.SendAsync(bytes, bytes.Length, new IPEndPoint(MulticastAddress, Port));
            }
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            var client = ((State)(ar.AsyncState))?.Client;
            var localIpAddress = ((State)(ar.AsyncState))?.LocalIpAddress;

            var endpoint = new IPEndPoint(MulticastAddress, Port);
            try
            {
                var receiveBytes = client?.EndReceive(ar, ref endpoint);
                Console.WriteLine($"Received on {localIpAddress}");
                PacketReceived?.Invoke(this, receiveBytes);
            }
            catch (ObjectDisposedException)
            {
                if (isQuitting) return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            Console.WriteLine($"Endpoint: {endpoint}");

            client?.BeginReceive(ReceiveCallback, null);
        }

        public void Dispose()
        {
            isQuitting = true;
            Clients.ForEach(x => x.Dispose());
        }
    }
}
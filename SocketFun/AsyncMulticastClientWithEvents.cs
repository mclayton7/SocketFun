using System;
using System.Net;
using System.Net.Sockets;

namespace SocketFun
{
    public class AsyncMulticastClientWithEvents : IMulticastSocket, IDisposable
    {
        public event EventHandler<byte[]> PacketReceived;

        private IPAddress MulticastAddress { get; }
        private int Port { get; }
        private readonly UdpClient Client;
        private bool isQuitting = false;

        public AsyncMulticastClientWithEvents(IPAddress multicastAddress, int port)
        {
            MulticastAddress = multicastAddress;
            Port = port;
            Client = new UdpClient()
            {
                ExclusiveAddressUse = false
            };
            Client.Client.Bind(new IPEndPoint(IPAddress.Any, Port));

            var localIpAddresses = NetworkHelpers.GetIpAddresses();
            foreach (var localIpAddress in localIpAddresses)
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

            Client.BeginReceive(ReceiveCallback, null);
        }

        public void Send(byte[] bytes)
        {
            Client.SendAsync(bytes, bytes.Length, new IPEndPoint(MulticastAddress, Port));
        }

        public void ReceiveCallback(IAsyncResult ar)  
        {
            var endpoint = new IPEndPoint(MulticastAddress, Port);
            try
            {
                var receiveBytes = Client?.EndReceive(ar, ref endpoint);
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

            Client?.BeginReceive(ReceiveCallback, null);
        }

        public void Dispose()
        {
            isQuitting = true;
            Client?.Dispose();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SocketFun
{
    public class AsyncMulticastClientWithTap : IDisposable, IMulticastSocket
    {
        public void Dispose()
        {
            CancellationSource.Cancel();
            Client.Close();
            Client?.Dispose();
        }

        private readonly IPAddress MulticastAddress;
        private readonly int Port;
        private readonly UdpClient Client;
        private readonly CancellationTokenSource CancellationSource;

        public event EventHandler<byte[]> PacketReceived;

        public AsyncMulticastClientWithTap(IPAddress multicastAddress, int port)
        {
            MulticastAddress = multicastAddress;
            Port = port;

            Client = new UdpClient()
            {
                ExclusiveAddressUse = false
            };
            Client.Client.Bind(new IPEndPoint(IPAddress.Any, Port));
            CancellationSource = new CancellationTokenSource();

            Console.WriteLine(
                $"==> ExclusiveAddressUse {Client.ExclusiveAddressUse} MulticastLoopback {Client.MulticastLoopback}");

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

            Task.Factory.StartNew(ReceiveNextMessage, CancellationSource.Token);
        }

        private void ReceiveNextMessage()
        {
            if (CancellationSource.IsCancellationRequested == false)
            {
                //Task.Delay(TimeSpan.FromMilliseconds(100))
                //    .ContinueWith(t => ReceiveMessage());
                Task.Factory.StartNew(ReceiveMessage);
            }
        }

        private void ReceiveMessage()
        {
            Task.Run(async () => await Client.ReceiveAsync(), CancellationSource.Token)
                .ContinueWith(t =>
                {
                    Console.WriteLine("Continue With");
                    Task.Factory.StartNew(() =>
                        {
                            Console.WriteLine("invoke");
                            PacketReceived?.Invoke(this, t.Result.Buffer);
                            ReceiveNextMessage();
                        },
                        CancellationSource.Token);
                });
        }

        public void Send(byte[] bytes)
        {
            Client.Send(bytes, bytes.Length, new IPEndPoint(MulticastAddress, Port));
        }
    }
}
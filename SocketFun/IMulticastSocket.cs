using System;

namespace SocketFun
{
    public interface IMulticastSocket
    {
        event EventHandler<byte[]> PacketReceived;
        void Send(byte[] bytes);
    }
}
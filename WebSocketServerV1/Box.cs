using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace WebSocketServerV1
{
    public class Box
    {
        public TcpClient TcpClient { get; private set; }
        public Guid Guid { get; private set; }
        public Dictionary<string, string> Headers { get; private set; } = new Dictionary<string, string>();

        public Box(TcpClient tcpClient, Guid guid)
        {
            TcpClient = tcpClient;
            Guid = guid;
        }

    }
}

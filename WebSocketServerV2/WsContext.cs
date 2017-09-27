using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using WebSocketServerV2.Interfaces;

namespace WebSocketServerV2
{
    public class WsContext : IWsContext
    {
        public Dictionary<string, string> Headers { get; internal set; } = new Dictionary<string, string>();
        public string Id { get; private set; }
        public IWsData WsData { get; internal set; }

        internal NetworkStream NetworkStream { get; set; }
        internal TcpClient TcpClient { get; set; }

        public WsContext(IWsServer wsServer, TcpClient client, string id)
        {
            this.Id = id;
            this.TcpClient = client;
        }

        public void Send(string msg)
        {
            var send = BuildMsg(msg);

            NetworkStream.Write(send, 0, send.Length);
        }

        private byte[] BuildMsg(string msg)
        {
            Byte[] response = Encoding.UTF8.GetBytes(msg);
            byte[] send = new byte[2];
            send[0] = 0x81; // last frame, text
            send[1] = Byte.Parse(response.Length.ToString(), System.Globalization.NumberStyles.Integer); // not masked, length 3

            var z = new byte[send.Length + response.Length];
            send.CopyTo(z, 0);
            response.CopyTo(z, send.Length);

            return z;
        }
    }
}
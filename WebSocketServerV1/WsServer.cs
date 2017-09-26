using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace WebSocketServerV1
{
    public class WsServer
    {
        public ConcurrentDictionary<Guid, TcpClient> Clients { get; private set; } = new ConcurrentDictionary<Guid, TcpClient>();

        public WsServer()
        {
            TcpListener ServerSocket = new TcpListener(IPAddress.Any, 80);
            ServerSocket.Start();

            while (true)
            {
                TcpClient client = ServerSocket.AcceptTcpClient();

                var gui = Guid.NewGuid();

                Clients.TryAdd(gui, client);

                Console.WriteLine("Someone connected!!");

                Box box = new Box(client, gui);

                Thread t = new Thread(handle_clients);
                t.Start(box);
            }
        }

        private void handle_clients(object o)
        {
            Box box = (Box)o;

            while (true)
            {

                parseData(box, out NetworkStream stream, out byte[] formated, out string data);


                if (isHeader(data))
                {
                    sendHandshake(box, stream, data);
                }
                else
                {
                    var data2 = getDecodedData(formated, formated.Length);
                    Console.WriteLine(data2);



                    var send = buildMsg("Hallo");

                    stream.Write(send, 0, send.Length);
                }
            }
        }

        private void sendHandshake(Box box, NetworkStream stream, string data)
        {
            var rows = data.Split(Environment.NewLine);
            rows = rows.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            foreach (var row in rows)
            {
                var pair = row.Split(":");
                if (pair.Length > 1)
                {
                    box.Headers.Add(pair[0].Trim(), pair[1].Trim());
                }
                else
                {
                    box.Headers.Add("Methode", pair[0].Trim());
                }
            }

            Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                   + "Connection: Upgrade" + Environment.NewLine
                   + "Upgrade: websocket" + Environment.NewLine
                   + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                       SHA1.Create().ComputeHash(
                           Encoding.UTF8.GetBytes(
                               new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                           )
                       )
                   ) + Environment.NewLine
                   + Environment.NewLine);



            stream.Write(response, 0, response.Length);
        }

        private bool isHeader(string data)
        {
            return new Regex("^GET").IsMatch(data);
        }

        private void parseData(Box box, out NetworkStream stream, out byte[] formated, out string _data)
        {
            stream = box.TcpClient.GetStream();
            byte[] buffer = new byte[1024];
            int byte_count = stream.Read(buffer, 0, buffer.Length);
            formated = new Byte[byte_count];
            Array.Copy(buffer, formated, byte_count); //handle  the null characteres in the byte array
            _data = Encoding.ASCII.GetString(formated);
        }

        private byte[] buildMsg(string msg)
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

        private string getDecodedData(byte[] buffer, int length)
        {
            byte b = buffer[1];
            int dataLength = 0;
            int totalLength = 0;
            int keyIndex = 0;

            if (b - 128 <= 125)
            {
                dataLength = b - 128;
                keyIndex = 2;
                totalLength = dataLength + 6;
            }

            if (b - 128 == 126)
            {
                dataLength = BitConverter.ToInt16(new byte[] { buffer[3], buffer[2] }, 0);
                keyIndex = 4;
                totalLength = dataLength + 8;
            }

            if (b - 128 == 127)
            {
                dataLength = (int)BitConverter.ToInt64(new byte[] { buffer[9], buffer[8], buffer[7], buffer[6], buffer[5], buffer[4], buffer[3], buffer[2] }, 0);
                keyIndex = 10;
                totalLength = dataLength + 14;
            }

            if (totalLength > length)
                throw new Exception("The buffer length is small than the data length");

            byte[] key = new byte[] { buffer[keyIndex], buffer[keyIndex + 1], buffer[keyIndex + 2], buffer[keyIndex + 3] };

            int dataIndex = keyIndex + 4;
            int count = 0;
            for (int i = dataIndex; i < totalLength; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ key[count % 4]);
                count++;
            }

            return Encoding.ASCII.GetString(buffer, dataIndex, dataLength);
        }

        //public static void broadcast(Dictionary<int, TcpClient> conexoes, string data)
        //{
        //    foreach (TcpClient c in conexoes.Values)
        //    {
        //        NetworkStream stream = c.GetStream();

        //        byte[] buffer = Encoding.ASCII.GetBytes(data);
        //        stream.Write(buffer, 0, buffer.Length);
        //    }
        //}
    }
}

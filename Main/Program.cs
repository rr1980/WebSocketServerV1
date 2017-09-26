using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServerV1;

namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 80);
            server.Server.NoDelay = true;

            server.Start();
            Console.WriteLine("Server has started on 127.0.0.1:80.{0}Waiting for a connection...", Environment.NewLine);

            TcpClient client = server.AcceptTcpClient();

            Console.WriteLine("A client connected.");

            NetworkStream stream = client.GetStream();

            //enter to an infinite cycle to be able to handle every change in stream
            while (true)
            {
                while (!stream.DataAvailable) ;

                Byte[] bytes = new Byte[client.Available];

                stream.Read(bytes, 0, bytes.Length);

                String data = Encoding.UTF8.GetString(bytes);

                if (new Regex("^GET").IsMatch(data))
                {

                    Console.WriteLine(data);
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
                else
                {
                    var data2 = GetDecodedData(bytes, bytes.Length);
                    Console.WriteLine(data2);



                    var send = BuildMsg("Hallo");

                    stream.Write(send, 0, send.Length);
                }
            }


            Console.ReadLine();
        }

        public static byte[] BuildMsg(string msg)
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

        public static string GetDecodedData(byte[] buffer, int length)
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
    }
}

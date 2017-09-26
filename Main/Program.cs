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
            WsServer wss = new WsServer();
        }
    }
}

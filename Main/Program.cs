using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServerV2;
using WebSocketServerV2.Interfaces;

namespace Main
{
    public class WsController : IWsServerController
    {
        public void OnConnect(IWsContext wsContext)
        {
            Console.WriteLine("Someone connected!!");
        }

        public void OnMessage(IWsContext wsContext, IWsData wsData)
        {
            Console.WriteLine("--: " + wsData.Data);

            wsContext.Send("Was geht ab?");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //WsServer wss = new WsServer();

            IWsServer server = WsServerFactory
                .Use(new WsController())
                .BuildServer();

            server.Start();
        }
    }
}

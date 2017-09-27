using System;
using WebSocketServerV2.Interfaces;

namespace WebSocketServerV2
{
    public sealed class WsServerFactory
    {
        private static WsServerFactory _instance;
        private static object _syncRoot = new Object();
        private static IWsServerController _wsController;

        private WsServerFactory()
        {
        }

        public static WsServerFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new WsServerFactory();
                    }
                }
                return _instance;
            }
        }

        public IWsServer BuildServer()
        {
            if (_wsController == null)
            {
                throw new NullReferenceException("IWsServerController is null!");
            }

            return new WsServer(_wsController);
        }

        public static WsServerFactory Use(IWsServerController wsController)
        {
            _wsController = wsController;
            return Instance;
        }
    }
}
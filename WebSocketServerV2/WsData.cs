using WebSocketServerV2.Interfaces;

namespace WebSocketServerV2
{
    public class WsData : IWsData
    {
        public byte[] Bytes { get; private set; }
        public string Raw { get; private set; }
        public string Data { get; private set; }

        public WsData(byte[] bytes, string raw, string data)
        {
            Bytes = bytes;
            Raw = raw;
            Data = data;
        }
    }
}
using System.Collections.Generic;

namespace WebSocketServerV2.Interfaces
{
    public interface IWsContext
    {
        string Id { get; }
        Dictionary<string, string> Headers { get; }

        void Send(string msg);
    }
}
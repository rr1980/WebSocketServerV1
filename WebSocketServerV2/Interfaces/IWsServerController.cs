namespace WebSocketServerV2.Interfaces
{
    public interface IWsServerController
    {
        void OnConnect(IWsContext wsContext);

        void OnMessage(IWsContext wsContext, IWsData wsData);
    }
}
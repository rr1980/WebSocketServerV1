namespace WebSocketServerV2.Interfaces
{
    public interface IWsData
    {
        byte[] Bytes { get; }
        string Raw { get; }
        string Data { get; }
    }
}
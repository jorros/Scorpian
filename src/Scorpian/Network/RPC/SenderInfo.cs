namespace Scorpian.Network.RPC;

public readonly struct SenderInfo
{
    internal SenderInfo(ushort senderId)
    {
        Id = senderId;
    }
    
    public ushort Id { get; }
}
using System;

namespace Scorpian.Network.RPC;

public readonly struct SenderInfo
{
    internal SenderInfo(uint senderId)
    {
        Id = senderId;
    }
    
    public uint Id { get; }
}
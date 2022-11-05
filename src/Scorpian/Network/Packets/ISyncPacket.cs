namespace Scorpian.Network.Packets;

public interface ISyncPacket : INetworkPacket
{
    string Scene { get; set; }
}
using System.IO;
using CommunityToolkit.HighPerformance;

namespace Scorpian.Network.Packets;

public struct LoginResponsePacket : INetworkPacket
{
    public bool Succeeded { get; set; }
    public string Reason { get; set; }
    public ushort ClientId { get; set; }

    public LoginResponsePacket()
    {
        Reason = string.Empty;
        Succeeded = false;
        ClientId = 0;
    }
    
    public void Write(Stream stream, PacketManager packetManager)
    {
        stream.Write(Succeeded);
        stream.Write(Reason);
        stream.Write(ClientId);
    }

    public void Read(Stream stream, PacketManager packetManager)
    {
        Succeeded = stream.Read<bool>();
        Reason = stream.ReadString();
        ClientId = stream.Read<ushort>();
    }
}
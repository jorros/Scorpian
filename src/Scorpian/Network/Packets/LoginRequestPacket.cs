using System.IO;

namespace Scorpian.Network.Packets;

public struct LoginRequestPacket : INetworkPacket
{
    public string Auth { get; set; }

    public LoginRequestPacket(string auth)
    {
        Auth = auth;
    }
    
    public void Write(BinaryWriter writer, PacketManager packetManager)
    {
        writer.Write(Auth ?? string.Empty);
    }

    public void Read(BinaryReader reader, PacketManager packetManager)
    {
        Auth = reader.ReadString();
    }
}
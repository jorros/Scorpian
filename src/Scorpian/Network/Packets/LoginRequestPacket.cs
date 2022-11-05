using System.IO;

namespace Scorpian.Network.Packets;

public class LoginRequestPacket : INetworkPacket
{
    public string Auth { get; set; }
    
    public void Write(Stream stream, PacketManager packetManager)
    {
        stream.Write(Auth ?? string.Empty);
    }

    public void Read(Stream stream, PacketManager packetManager)
    {
        Auth = stream.ReadString();
    }
}
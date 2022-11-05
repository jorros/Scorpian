using System.IO;

namespace Scorpian.Network.Packets;

public interface INetworkPacket
{
    void Write(Stream stream, PacketManager packetManager);
    void Read(Stream stream, PacketManager packetManager);
}
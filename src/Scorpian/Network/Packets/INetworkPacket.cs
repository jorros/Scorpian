using System.IO;

namespace Scorpian.Network.Packets;

public interface INetworkPacket
{
    void Write(BinaryWriter writer, PacketManager packetManager);
    void Read(BinaryReader reader, PacketManager packetManager);
}
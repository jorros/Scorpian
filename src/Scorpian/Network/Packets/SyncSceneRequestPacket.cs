using System.IO;

namespace Scorpian.Network.Packets;

public struct SyncSceneRequestPacket : ISyncPacket
{
    public string Scene { get; set; }
    
    public void Write(BinaryWriter writer, PacketManager packetManager)
    {
        writer.Write(Scene);
    }

    public void Read(BinaryReader reader, PacketManager packetManager)
    {
        Scene = reader.ReadString();
    }
}
using System.IO;
using CommunityToolkit.HighPerformance;

namespace Scorpian.Network.Packets;

public struct SyncVarPacket : ISyncPacket
{
    public string Scene { get; set; }
    public ulong NodeId { get; set; }
    public int Field { get; set; }
    public object Value { get; set; }

    public void Write(BinaryWriter writer, PacketManager packetManager)
    {
        writer.Write(Scene);
        writer.Write(NodeId);
        writer.Write(Field);
        packetManager.Write(Value, writer);
    }

    public void Read(BinaryReader reader, PacketManager packetManager)
    {
        Scene = reader.ReadString();
        NodeId = reader.ReadUInt64();
        Field = reader.ReadInt32();
        Value = packetManager.Read(reader);
    }
}
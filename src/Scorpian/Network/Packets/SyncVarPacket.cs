using System.IO;
using CommunityToolkit.HighPerformance;

namespace Scorpian.Network.Packets;

public struct SyncVarPacket : ISyncPacket
{
    public string Scene { get; set; }
    public ulong NodeId { get; set; }
    public int Field { get; set; }
    public object Value { get; set; }

    public void Write(Stream stream, PacketManager packetManager)
    {
        stream.Write(Scene);
        stream.Write(NodeId);
        stream.Write(Field);
        packetManager.Write(Value, stream);
    }

    public void Read(Stream stream, PacketManager packetManager)
    {
        Scene = stream.ReadString();
        NodeId = stream.Read<ulong>();
        Field = stream.Read<int>();
        Value = packetManager.Read(stream);
    }
}
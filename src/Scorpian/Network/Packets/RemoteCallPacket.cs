using System.IO;
using CommunityToolkit.HighPerformance;

namespace Scorpian.Network.Packets;

public struct RemoteCallPacket : ISyncPacket
{
    public string Scene { get; set; }
    public ulong NodeId { get; set; }
    public int Method { get; set; }
    public object Arguments { get; set; }
    
    public void Write(BinaryWriter writer, PacketManager packetManager)
    {
        writer.Write(Scene);
        writer.Write(NodeId);
        writer.Write(Method);
        packetManager.Write(Arguments, writer);
    }

    public void Read(BinaryReader reader, PacketManager packetManager)
    {
        Scene = reader.ReadString();
        NodeId = reader.ReadUInt64();
        Method = reader.ReadInt32();
        Arguments = packetManager.Read(reader);
    }
}
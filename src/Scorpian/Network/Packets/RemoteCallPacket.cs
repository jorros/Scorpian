using System.IO;
using CommunityToolkit.HighPerformance;

namespace Scorpian.Network.Packets;

public struct RemoteCallPacket : ISyncPacket
{
    public string Scene { get; set; }
    public ulong NodeId { get; set; }
    public int Method { get; set; }
    public object Arguments { get; set; }
    
    public void Write(Stream stream, PacketManager packetManager)
    {
        stream.Write(Scene);
        stream.Write(NodeId);
        stream.Write(Method);
        packetManager.Write(Arguments, stream);
    }

    public void Read(Stream stream, PacketManager packetManager)
    {
        Scene = stream.ReadString();
        NodeId = stream.Read<ulong>();
        Method = stream.Read<int>();
        Arguments = packetManager.Read(stream);
    }
}
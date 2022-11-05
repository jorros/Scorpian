using System.IO;
using System.Linq;
using CommunityToolkit.HighPerformance;

namespace Scorpian.Network.Packets;

public struct CreateNodePacket : ISyncPacket
{
    public ulong NetworkId { get; set; }
    public string Node { get; set; }
    public string Scene { get; set; }
    public SyncVarPacket[] Variables { get; set; }
    public SyncListPacket[] Lists { get; set; }

    public void Write(Stream stream, PacketManager packetManager)
    {
        stream.Write(NetworkId);
        stream.Write(Node);
        stream.Write(Scene);
        
        packetManager.Write(Variables.Cast<INetworkPacket>(), stream);
        // packetManager.Write(Lists.Cast<INetworkPacket>(), stream);
    }

    public void Read(Stream stream, PacketManager packetManager)
    {
        NetworkId = stream.Read<ulong>();
        Node = stream.ReadString();
        Scene = stream.ReadString();

        Variables = packetManager.Read(stream) as SyncVarPacket[];
        // Lists = packetManager.Read(stream) as SyncListPacket[];
    }
}
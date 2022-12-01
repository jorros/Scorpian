using System.IO;
using System.Linq;

namespace Scorpian.Network.Packets;

public struct CreateNodePacket : ISyncPacket
{
    public ulong NetworkId { get; set; }
    public string Node { get; set; }
    public string Scene { get; set; }
    public SyncVarPacket[] Variables { get; set; }
    public SyncListPacket[] Lists { get; set; }

    public void Write(BinaryWriter writer, PacketManager packetManager)
    {
        writer.Write(NetworkId);
        writer.Write(Node);
        writer.Write(Scene);
        
        packetManager.Write(Variables.Cast<INetworkPacket>(), writer);
        // packetManager.Write(Lists.Cast<INetworkPacket>(), stream);
    }

    public void Read(BinaryReader reader, PacketManager packetManager)
    {
        NetworkId = reader.ReadUInt64();
        Node = reader.ReadString();
        Scene = reader.ReadString();

        Variables = (packetManager.Read(reader) as object[])?.Cast<SyncVarPacket>().ToArray();
        // Lists = packetManager.Read(stream) as SyncListPacket[];
    }
}
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.HighPerformance;
using Scorpian.Network.Protocol;

namespace Scorpian.Network.Packets;

public struct SyncListPacket : ISyncPacket
{
    public string Scene { get; set; }
    public ulong NodeId { get; set; }
    public int Field { get; set; }
    public NetworkedListAction Action { get; set; }

    public object Value { get; set; }
    public int Index { get; set; }
    public List<object> List { get; set; }
    
    public void Write(BinaryWriter writer, PacketManager packetManager)
    {
        writer.Write(Scene);
        writer.Write(NodeId);
        writer.Write(Field);
        
        writer.Write((byte)Action);

        switch (Action)
        {
            case NetworkedListAction.Add:
                packetManager.Write(Value, writer);
                break;
            
            case NetworkedListAction.Insert:
                packetManager.Write(Value, writer);
                writer.Write(Index);
                break;
            
            case NetworkedListAction.Remove:
                packetManager.Write(Value, writer);
                break;
            
            case NetworkedListAction.Set:
                packetManager.Write(Value, writer);
                writer.Write(Index);
                break;
            
            case NetworkedListAction.RemoveAt:
                writer.Write(Index);
                break;
            
            case NetworkedListAction.Sync:
                packetManager.Write(List, writer);
                break;
        }
    }

    public void Read(BinaryReader reader, PacketManager packetManager)
    {
        Scene = reader.ReadString();
        NodeId = reader.ReadUInt64();
        Field = reader.ReadInt32();
        
        Action = (NetworkedListAction)reader.ReadByte();

        switch (Action)
        {
            case NetworkedListAction.Add:
                Value = packetManager.Read(reader);
                break;
            
            case NetworkedListAction.Insert:
                Value = packetManager.Read(reader);
                Index = reader.ReadInt32();
                break;
            
            case NetworkedListAction.Remove:
                Value = packetManager.Read(reader);
                break;
            
            case NetworkedListAction.Set:
                Value = packetManager.Read(reader);
                Index = reader.ReadInt32();
                break;
            
            case NetworkedListAction.RemoveAt:
                Index = reader.ReadInt32();
                break;
            
            case NetworkedListAction.Sync:
                List = (List<object>)packetManager.Read(reader);
                break;
        }
    }
}
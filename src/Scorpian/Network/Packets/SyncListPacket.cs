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
    
    public void Write(Stream stream, PacketManager packetManager)
    {
        stream.Write(Scene);
        stream.Write(NodeId);
        stream.Write(Field);
        
        stream.Write((byte)Action);

        switch (Action)
        {
            case NetworkedListAction.Add:
                packetManager.Write(Value, stream);
                break;
            
            case NetworkedListAction.Insert:
                packetManager.Write(Value, stream);
                stream.Write(Index);
                break;
            
            case NetworkedListAction.Remove:
                packetManager.Write(Value, stream);
                break;
            
            case NetworkedListAction.Set:
                packetManager.Write(Value, stream);
                stream.Write(Index);
                break;
            
            case NetworkedListAction.RemoveAt:
                stream.Write(Index);
                break;
            
            case NetworkedListAction.Sync:
                packetManager.Write(List, stream);
                break;
        }
    }

    public void Read(Stream stream, PacketManager packetManager)
    {
        Scene = stream.ReadString();
        NodeId = stream.Read<ulong>();
        Field = stream.Read<int>();
        
        Action = (NetworkedListAction)stream.Read<byte>();

        switch (Action)
        {
            case NetworkedListAction.Add:
                Value = packetManager.Read(stream);
                break;
            
            case NetworkedListAction.Insert:
                Value = packetManager.Read(stream);
                Index = stream.Read<int>();
                break;
            
            case NetworkedListAction.Remove:
                Value = packetManager.Read(stream);
                break;
            
            case NetworkedListAction.Set:
                Value = packetManager.Read(stream);
                Index = stream.Read<int>();
                break;
            
            case NetworkedListAction.RemoveAt:
                Index = stream.Read<int>();
                break;
            
            case NetworkedListAction.Sync:
                List = (List<object>)packetManager.Read(stream);
                break;
        }
    }
}
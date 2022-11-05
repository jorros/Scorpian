using System.IO;

namespace Scorpian.Network.Packets;

public struct SwitchScenePacket : ISyncPacket
{
    public string Scene { get; set; }
    
    public void Write(Stream stream, PacketManager packetManager)
    {
        stream.Write(Scene);
    }

    public void Read(Stream stream, PacketManager packetManager)
    {
        Scene = stream.ReadString();
    }
}
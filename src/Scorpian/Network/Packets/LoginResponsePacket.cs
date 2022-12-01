using System;
using System.IO;

namespace Scorpian.Network.Packets;

public struct LoginResponsePacket : INetworkPacket
{
    public bool Succeeded { get; set; }
    public string Reason { get; set; }
    public uint ClientId { get; set; }

    public LoginResponsePacket()
    {
        Reason = string.Empty;
        Succeeded = false;
        ClientId = 0;
    }

    public LoginResponsePacket(uint clientId)
    {
        Reason = string.Empty;
        Succeeded = true;
        ClientId = clientId;
    }
    
    public void Write(BinaryWriter writer, PacketManager packetManager)
    {
        writer.Write(Succeeded);
        writer.Write(Reason);
        writer.Write(ClientId);
    }

    public void Read(BinaryReader reader, PacketManager packetManager)
    {
        Succeeded = reader.ReadBoolean();
        Reason = reader.ReadString();
        ClientId = reader.ReadUInt32();
    }
}
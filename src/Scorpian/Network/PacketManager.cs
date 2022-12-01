using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Scorpian.Helper;
using Scorpian.HexMap;
using Scorpian.Network.Packets;

namespace Scorpian.Network;

public class PacketManager
{
    private readonly EngineSettings _settings;
    private readonly ILogger<PacketManager> _logger;

    private enum Mapping
    {
        Null,
        String,
        Byte,
        Short,
        Ushort,
        Int,
        Uint,
        Long,
        Ulong,
        Float,
        Double,
        Bool,
        Packet,
        Array,
        Hex,
        Guid
    };

    public PacketManager(EngineSettings settings, ILogger<PacketManager> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public byte[] Serialize<T>(T packet) where T : INetworkPacket
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        Write(packet, writer);

        var data = stream.ToArray();

        return data;
    }

    public object Deserialize(byte[] data)
    {
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);

        var packet = Read(reader);

        return packet;
    }

    public object Read(BinaryReader reader)
    {
        var firstByte = (Mapping) reader.ReadByte();
        
        // _logger.LogDebug("Receiving {Type}", firstByte.ToString());

        switch (firstByte)
        {
            case Mapping.Null:
                return null;
            case Mapping.Byte:
                return reader.ReadByte();
            case Mapping.String:
                return reader.ReadString();
            case Mapping.Short:
                return reader.ReadInt16();
            case Mapping.Ushort:
                return reader.ReadUInt16();
            case Mapping.Int:
                return reader.ReadInt32();
            case Mapping.Uint:
                return reader.ReadUInt32();
            case Mapping.Long:
                return reader.ReadInt64();
            case Mapping.Ulong:
                return reader.ReadUInt64();
            case Mapping.Float:
                return reader.ReadSingle();
            case Mapping.Double:
                return reader.ReadDouble();
            case Mapping.Bool:
                return reader.ReadBoolean();
            case Mapping.Hex:
                return new Hex(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
            case Mapping.Packet:
            {
                var id = reader.ReadUInt16();

                if (!_settings.NetworkPackets.ContainsKey(id))
                {
                    _logger.LogError("Trying to read unknown packet with ID {Id}", id);
                    return null;
                }

                var packetType = _settings.NetworkPackets[id];
                
                // _logger.LogDebug("Packet is {PacketType}", packetType.Name);

                var packet = Activator.CreateInstance(packetType) as INetworkPacket;
                packet?.Read(reader, this);

                return packet;
            }
            case Mapping.Array:
            {
                var length = reader.ReadUInt16();
                var array = new object[length];

                foreach (var i in Enumerable.Range(0, length))
                {
                    array[i] = Read(reader);
                }

                return array;
            }
            case Mapping.Guid:
                return new Guid(reader.ReadBytes(16));
            default:
                _logger.LogError("Received unknown first byte {FirstByte}", firstByte);

                return null;
        }
    }

    public void Write<T>(T data, BinaryWriter writer)
    {
        switch (data)
        {
            case null:
                writer.Write((byte) Mapping.Null);
                break;
            case Guid val:
                writer.Write((byte) Mapping.Guid);
                writer.Write(val.ToByteArray());
                break;
            case byte val:
                writer.Write((byte) Mapping.Byte);
                writer.Write(val);
                break;
            case string val:
                writer.Write((byte) Mapping.String);
                writer.Write(val);
                break;
            case short val:
                writer.Write((byte) Mapping.Short);
                writer.Write(val);
                break;
            case ushort val:
                writer.Write((byte) Mapping.Ushort);
                writer.Write(val);
                break;
            case int val:
                writer.Write((byte) Mapping.Int);
                writer.Write(val);
                break;
            case uint val:
                writer.Write((byte) Mapping.Uint);
                writer.Write(val);
                break;
            case long val:
                writer.Write((byte) Mapping.Long);
                writer.Write(val);
                break;
            case ulong val:
                writer.Write((byte) Mapping.Ulong);
                writer.Write(val);
                break;
            case float val:
                writer.Write((byte) Mapping.Float);
                writer.Write(val);
                break;
            case double val:
                writer.Write((byte) Mapping.Double);
                writer.Write(val);
                break;
            case bool val:
                writer.Write((byte) Mapping.Bool);
                writer.Write(val);
                break;
            case Hex hex:
                writer.Write((byte)Mapping.Hex);
                writer.Write(hex.Q);
                writer.Write(hex.R);
                writer.Write(hex.S);
                break;
            case INetworkPacket packet:
            {
                writer.Write((byte) Mapping.Packet);
                
                var hash = packet.GetType().FullName.GetDeterministicHashCode16();

                writer.Write(hash);
                packet.Write(writer, this);

                break;
            }
            case IEnumerable<INetworkPacket> enumerable:
            {
                var array = enumerable.ToArray();
                writer.Write((byte) Mapping.Array);
                writer.Write((ushort) array.Count());

                foreach (var element in array)
                {
                    Write(element, writer);
                }
                
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
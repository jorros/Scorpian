using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Scorpian.Network;

public static class StreamExtensions
{
    public static void Write(this Stream stream, string value)
    {
        foreach (var c in value)
        {
            var bytes = BitConverter.GetBytes(c);
            stream.Write(bytes);
        }

        stream.Write(BitConverter.GetBytes('\0'));
    }
    
    public static string ReadString(this Stream stream)
    {
        var sb = new StringBuilder();
        char c;
        var buffer = new byte[sizeof(char)];

        do
        {
            stream.Read(buffer);
            c = BitConverter.ToChar(buffer);

            if (c != '\0')
            {
                sb.Append(c);
            }
        } while (c != '\0');

        return sb.ToString();
    }

    // public static string ReadString(this Stream stream)
    // {
    //     var sb = new StringBuilder();
    //     char c;
    //     Span<byte> buffer = stackalloc byte[Marshal.SizeOf<char>()];
    //
    //     do
    //     {
    //         if (stream.Read(buffer) != buffer.Length)
    //         {
    //             throw new InvalidOperationException("The stream didn't contain enough data to read the requested item");
    //         }
    //
    //         ref var r0 = ref MemoryMarshal.GetReference(buffer);
    //         c = Unsafe.ReadUnaligned<char>(ref r0);
    //
    //         if (c != '\0')
    //         {
    //             sb.Append(c);
    //         }
    //     } while (c != '\0');
    //
    //     return sb.ToString();
    // }

    // public static void Write(this Stream stream, INetworkPacket packet)
    // {
    //     packet.Write(stream);
    // }
    //
    // public static T Read<T>(this Stream stream) where T : INetworkPacket
    // {
    //     var packet = Activator.CreateInstance<T>();
    //     packet.Read(stream);
    //
    //     return packet;
    // }

    public static void Write<T>(this Stream stream, IEnumerable<T> list) where T : unmanaged
    {
        var array = list.ToArray();

        CommunityToolkit.HighPerformance.StreamExtensions.Write(stream, array.Length);
        foreach (var item in array)
        {
            CommunityToolkit.HighPerformance.StreamExtensions.Write(stream, item);
        }
    }

    public static IEnumerable<T> ReadArray<T>(this Stream stream) where T : unmanaged
    {
        var length = CommunityToolkit.HighPerformance.StreamExtensions.Read<int>(stream);

        var array = new T[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = CommunityToolkit.HighPerformance.StreamExtensions.Read<T>(stream);
        }

        return array;
    }

    public static void Write(this Stream stream, IEnumerable<string> list)
    {
        var array = list.ToArray();

        CommunityToolkit.HighPerformance.StreamExtensions.Write(stream, array.Length);
        foreach (var item in array)
        {
            Write(stream, item);
        }
    }

    public static IEnumerable<string> ReadArray(this Stream stream)
    {
        var length = CommunityToolkit.HighPerformance.StreamExtensions.Read<int>(stream);

        var array = new string[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = ReadString(stream);
        }

        return array;
    }

    // public static void Write(this Stream stream, IReadOnlyList<INetworkPacket> list)
    // {
    //     CommunityToolkit.HighPerformance.StreamExtensions.Write(stream, list.Count);
    //     foreach (var item in list)
    //     {
    //         Write(stream, item);
    //     }
    // }
    //
    // public static IEnumerable<T> ReadPackets<T>(this Stream stream) where T : INetworkPacket
    // {
    //     var length = CommunityToolkit.HighPerformance.StreamExtensions.Read<int>(stream);
    //
    //     var array = new T[length];
    //     for (var i = 0; i < length; i++)
    //     {
    //         array[i] = Read<T>(stream);
    //     }
    //
    //     return array;
    // }

    // public static void Write<TKey, TValue>(this Stream stream, IDictionary<TKey, TValue> dict)
    //     where TValue : unmanaged
    // {
    //     CommunityToolkit.HighPerformance.StreamExtensions.Write(stream, dict.Count);
    //
    //     foreach (var (key, value) in dict)
    //     {
    //     }
    // }
    
    public static async Task<MemoryStream> ReadIntoBuffer(this NetworkStream stream)
    {
        var buffer = new byte[256];
        var length = await stream.ReadAsync(buffer);
        return new MemoryStream(buffer, 0, length);
    }
}
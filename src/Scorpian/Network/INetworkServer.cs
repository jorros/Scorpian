using System.Collections.Generic;
using System.Threading.Tasks;
using Scorpian.Network.Packets;

namespace Scorpian.Network;

public interface INetworkServer
{
    Task Start();
    Task Stop();
    
    Task Disconnect(uint client);
    
    IEnumerable<uint> ConnectedClients { get; }

    void Send<T>(T packet, uint? receiver = null) where T : INetworkPacket;
}
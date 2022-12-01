using System;
using System.Threading.Tasks;
using Scorpian.Network.Packets;

namespace Scorpian.Network;

public interface INetworkClient
{
    Task Start();
    Task Stop();

    Task Connect(string host, int port, string auth);
    Task Disconnect();
    
    bool IsConnected { get; }
    uint ClientId { get; }

    void Send<T>(T packet) where T : INetworkPacket;
}
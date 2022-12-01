using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LiteNetwork;
using LiteNetwork.Server;
using Microsoft.Extensions.DependencyInjection;
using Scorpian.Network.Packets;

namespace Scorpian.Network;

public class LiteNetworkServer : LiteServer<LiteServerUser>, INetworkServer
{
    private readonly EngineSettings _settings;
    private readonly PacketManager _packetManager;
    private readonly LiteConnectionManager _connectionManager;
    
    public LiteNetworkServer(IServiceProvider serviceProvider) : base(new LiteServerOptions(), serviceProvider)
    {
        _settings = serviceProvider.GetRequiredService<EngineSettings>();
        _packetManager = serviceProvider.GetRequiredService<PacketManager>();
        _connectionManager = serviceProvider.GetRequiredService<LiteConnectionManager>();
    }

    protected override void OnError(LiteConnection connection, Exception exception)
    {
        Console.WriteLine("Test");
    }

    public async Task Start()
    {
        Options.Host = IPAddress.Any.ToString();
        Options.Port = _settings.Port;

        await StartAsync();
    }

    public async Task Stop()
    {
        await StopAsync();
    }

    public Task Disconnect(uint client)
    {
        var connection = _connectionManager.Get(client);
        DisconnectUser(connection);
        
        return Task.CompletedTask;
    }

    public IEnumerable<uint> ConnectedClients => _connectionManager.IDs;
    public void Send<T>(T packet, uint? receiver = null) where T : INetworkPacket
    {
        var data = _packetManager.Serialize(packet);
        
        if (receiver is null)
        {
            SendToAll(data);

            return;
        }

        var connection = _connectionManager.Get(receiver.Value);
        SendTo(connection, data);
    }
}
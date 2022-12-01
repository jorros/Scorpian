using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scorpian.Network.Packets;

namespace Scorpian.Network;

public class NetworkManager
{
    private readonly EngineSettings _settings;
    private readonly ILogger<NetworkManager> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly NetworkQueue _queue;

    private LiteNetworkServer _server;
    private LiteNetworkClient _client;

    public event EventHandler<DataReceivedEventArgs> OnPacketReceive;
    public event EventHandler<UserConnectedEventArgs> OnUserConnect;
    public event EventHandler<UserDisconnectedEventArgs> OnUserDisconnect;
    public event EventHandler<AuthenticationFailedEventArgs> OnAuthenticationFail;

    public uint ClientId => _client.ClientId;

    public NetworkManager(EngineSettings settings, ILogger<NetworkManager> logger, PacketManager packetManager,
        IServiceProvider serviceProvider, NetworkQueue queue)
    {
        _settings = settings;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _queue = queue;
    }

    internal async Task Start()
    {
        if (IsClient)
        {
            _client = new LiteNetworkClient(_serviceProvider);
            await _client.Start();
            
            return;
        }
        
        _server = new LiteNetworkServer(_serviceProvider);
        await _server.Start();
        _logger.LogInformation("Started server");
    }

    public bool IsConnected => _client?.IsConnected == true;

    public IEnumerable<uint> ConnectedClients => _server.ConnectedClients;

    public void Send<T>(T packet, uint? receiver = null) where T : INetworkPacket
    {
        if (IsClient)
        {
            _client.Send(packet);

            return;
        }

        _server.Send(packet, receiver);
    }

    public void RunQueue()
    {
        while (_queue.HasAny)
        {
            var evt = _queue.Dequeue();

            switch (evt)
            {
                case AuthenticationFailedEventArgs authFailed:
                    OnAuthenticationFail?.Invoke(this, authFailed);
                    break;
                
                case DataReceivedEventArgs dataReceived:
                    OnPacketReceive?.Invoke(this, dataReceived);
                    break;
                
                case UserConnectedEventArgs userConnected:
                    OnUserConnect?.Invoke(this, userConnected);
                    break;
                
                case UserDisconnectedEventArgs userDisconnected:
                    OnUserDisconnect?.Invoke(this, userDisconnected);
                    break;
            }
        }
    }

    public async Task Disconnect(uint? client = null)
    {
        if (IsServer && client is not null)
        {
            await _server.Disconnect(client.Value);
            
            return;
        }

        await _client.Disconnect();
    }

    public async Task Connect(string host, int port, string auth = null)
    {
        if (!IsClient)
        {
            return;
        }

        _logger.LogInformation("Connecting to server");

        await _client.Connect(host, port, auth);
    }

    public bool IsClient => _settings.NetworkMode == NetworkMode.Client;
    public bool IsServer => _settings.NetworkMode == NetworkMode.Server;

    internal async Task Stop()
    {
        if (IsServer)
        {
            _logger.LogInformation("Stop server");
            await _server.Stop();
            _server.Dispose();

            return;
        }

        _logger.LogInformation("Close remote connection");
        await _client.Stop();
    }
}
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scorpian.Network.Packets;

namespace Scorpian.Network;

public class LiteServerUser : LiteNetwork.Server.LiteServerUser
{
    private readonly ILogger<LiteServerUser> _logger;
    private readonly PacketManager _packetManager;
    private readonly EngineSettings _settings;
    private readonly LiteConnectionManager _connectionManager;
    private readonly NetworkQueue _queue;
    private readonly IServiceProvider _serviceProvider;

    private uint _clientId;

    public LiteServerUser(ILogger<LiteServerUser> logger, PacketManager packetManager, EngineSettings settings,
        LiteConnectionManager connectionManager, NetworkQueue queue, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _packetManager = packetManager;
        _settings = settings;
        _connectionManager = connectionManager;
        _queue = queue;
        _serviceProvider = serviceProvider;
    }

    public override Task HandleMessageAsync(byte[] packetBuffer)
    {
        _logger.LogDebug("[{ClientId}] Receiving data from remote endpoint", _clientId);

        var packet = _packetManager.Deserialize(packetBuffer);

        if (packet is LoginRequestPacket loginRequest)
        {
            if (Authenticate(loginRequest))
            {
                var endpoint = Socket?.RemoteEndPoint as IPEndPoint;
                _logger.LogInformation("[{ClientId}] New client connected from {IpAddress}", _clientId,
                    endpoint?.Address);

                _queue.Enqueue(new UserConnectedEventArgs(_clientId));
            }

            return Task.CompletedTask;
        }
        
        _queue.Enqueue(new DataReceivedEventArgs { Data = packet, ClientId = _clientId });

        return Task.CompletedTask;
    }

    private bool Authenticate(LoginRequestPacket loginRequest)
    {
        var response = _settings.Authentication(loginRequest.Auth, _serviceProvider);
        response.ClientId = _clientId;

        var data = _packetManager.Serialize(response);
        Send(data);

        if (!response.Succeeded)
        {
            _logger.LogDebug("Failed authentication: {Reason}", response.Reason);
            _queue.Enqueue(new AuthenticationFailedEventArgs(response.Reason));
        }

        return response.Succeeded;
    }

    protected override void OnConnected()
    {
        _clientId = _connectionManager.Add(Context?.Users.First(x => x.Id == Id));
        
        var endpoint = Socket?.RemoteEndPoint as IPEndPoint;
        _logger.LogInformation("[{ClientId}] New connection initiated from {IpAddress}", _clientId, endpoint?.Address);
    }

    protected override void OnDisconnected()
    {
        _connectionManager.Remove(_clientId);
        _queue.Enqueue(new UserDisconnectedEventArgs(_clientId));
        
        _logger.LogInformation("[{ClientId}] Disconnected", _clientId);
    }
}
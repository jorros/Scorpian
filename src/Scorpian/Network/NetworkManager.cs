using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using Microsoft.Extensions.Logging;
using Scorpian.Helper;
using Scorpian.Network.Packets;

namespace Scorpian.Network;

public class NetworkManager
{
    private readonly EngineSettings _settings;
    private readonly ILogger<NetworkManager> _logger;
    private readonly PacketManager _packetManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<ushort, TcpClient> _connectedClients = new();
    private TcpClient _client;
    private TcpListener _listener;

    private ushort _clientCounter = 1;

    public event EventHandler<DataReceivedEventArgs> OnPacketReceive;
    public event EventHandler<UserConnectedEventArgs> OnUserConnect;
    public event EventHandler<UserDisconnectedEventArgs> OnUserDisconnect;
    public event EventHandler<AuthenticationFailedEventArgs> OnAuthenticationFail;

    public bool IsConnected { get; private set; }

    public ushort ClientId { get; private set; }

    public NetworkManager(EngineSettings settings, ILogger<NetworkManager> logger, PacketManager packetManager,
        IServiceProvider serviceProvider)
    {
        _settings = settings;
        _logger = logger;
        _packetManager = packetManager;
        _serviceProvider = serviceProvider;
    }

    internal void Start()
    {
        if (_settings.NetworkMode == NetworkMode.Server)
        {
            try
            {
                var endPoint = new IPEndPoint(IPAddress.Any, _settings.Port);
                _listener = new TcpListener(endPoint);

                try
                {
                    Task.Run(RunServer);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            catch (SocketException e)
            {
                _logger.LogCritical("Failed to start server: {Error}", e.Message);
                _listener.Stop();
                throw;
            }

            return;
        }

        _client = new TcpClient
        {
            NoDelay = true
        };
    }

    private async Task RunServer()
    {
        try
        {
            _logger.LogInformation("Start server listening on {Address}:{Port}", IPAddress.Any.ToString(),
                _settings.Port);

            _listener?.Start();

            while (_listener is not null)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _logger.LogDebug("Incoming connection");

                var stream = client.GetStream();
                _logger.LogDebug("Receive authentication");

                var clientId = _clientCounter;

                try
                {
                    using var buffer = new NetworkBuffer(stream, 2000);
                    await buffer.Read();

                    var loginRequest = new LoginRequestPacket();
                    loginRequest.Read(buffer.Stream, _packetManager);

                    buffer.Flush();

                    var response = _settings.Authentication(loginRequest.Auth, _serviceProvider);
                    response.ClientId = clientId;
                    response.Write(buffer.Stream, _packetManager);

                    buffer.Write();

                    if (!response.Succeeded)
                    {
                        _logger.LogDebug("Failed authentication: {Reason}", response.Reason);
                        client.Close();
                        continue;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogDebug("Failed authentication: {Reason}", e.Message);

                    var responsePacket = new LoginResponsePacket
                    {
                        Reason = "NO_AUTH",
                        Succeeded = false
                    };
                    responsePacket.Write(stream, _packetManager);

                    client.Close();
                    continue;
                }

                _clientCounter++;

                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                _logger.LogInformation("[{ClientId}] New client connected from {IpAddress}", clientId,
                    endpoint?.Address);

                stream.Write(clientId);

                OnUserConnect?.Invoke(this, new UserConnectedEventArgs
                {
                    ClientId = clientId
                });

                _connectedClients.Add(clientId, client);
                Task.Run(async () => await ReceiveData(client, clientId));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task ReceiveData(TcpClient client, ushort clientId)
    {
        using var buffer = new NetworkBuffer(client.GetStream());

        while (client.Connected)
        {
            try
            {
                await buffer.Read();

                _logger.LogDebug("[{ClientId}] Receiving data from remote endpoint", clientId);

                var packet = _packetManager.Read(buffer.Stream);

                buffer.Flush();

                OnPacketReceive?.Invoke(clientId, new DataReceivedEventArgs
                {
                    Data = packet,
                    SenderId = clientId
                });
            }
            catch (Exception e)
            {
                _logger.LogError("Exception occured during data retrieval: {Error}", e.Message);
                throw;
            }
        }
    }

    public IEnumerable<ushort> ConnectedClients => _connectedClients.Keys;

    public void Send<T>(T packet, ushort client = 0)
    {
        using var buffer = new NetworkBuffer(null);
        _packetManager.Write(packet, buffer.Stream);

        if (IsClient)
        {
            if (!_client.Connected)
            {
                return;
            }
            buffer.WriteTo(_client.GetStream());

            return;
        }

        if (client == 0)
        {
            foreach (var clientStream in _connectedClients.Keys.Select(clientId =>
                         _connectedClients[clientId].GetStream()))
            {
                buffer.WriteTo(clientStream);
            }

            return;
        }

        buffer.WriteTo(_connectedClients[client].GetStream());
    }

    public void Connect(string hostname, int port, string authString = null)
    {
        if (_client is null)
        {
            return;
        }

        _logger.LogInformation("Connecting to server on port {Port}", port);
        try
        {
            _client.Connect(hostname, port);

            if (!_client.Connected)
            {
                return;
            }

            using var buffer = new NetworkBuffer(_client.GetStream());
            var loginRequest = new LoginRequestPacket
            {
                Auth = authString
            };
            loginRequest.Write(buffer.Stream, _packetManager);
            buffer.Write();

            buffer.Flush();

            buffer.Read().Wait();

            var response = new LoginResponsePacket();
            response.Read(buffer.Stream, _packetManager);

            if (!response.Succeeded)
            {
                _client.Close();
                _client = new TcpClient();

                OnAuthenticationFail?.Invoke(this, new AuthenticationFailedEventArgs
                {
                    Reason = response.Reason
                });
                return;
            }

            ClientId = response.ClientId;

            OnUserConnect?.Invoke(this, new UserConnectedEventArgs {ClientId = ClientId});
            Task.Run(() => ReceiveData(_client, 0));
        }
        catch (SocketException e)
        {
            _logger.LogWarning("Failed to connect to server: {Error}", e.Message);
        }
    }

    internal void UpdateStatus()
    {
        if (IsClient)
        {
            if (_client.Client.LocalEndPoint is not IPEndPoint localEndpoint ||
                _client.Client.RemoteEndPoint is not IPEndPoint remoteEndpoint)
            {
                return;
            }

            var state = TcpStatus.GetState(remoteEndpoint.Port, localEndpoint.Port);

            switch (state)
            {
                case ConnectionStatus.Disconnecting or ConnectionStatus.NotReady when IsConnected:
                    IsConnected = false;
                    _client.Close();
                    _client = new TcpClient();
                    OnUserDisconnect?.Invoke(this, new UserDisconnectedEventArgs
                    {
                        ClientId = 0
                    });
                    break;
                case ConnectionStatus.Connected:
                    IsConnected = true;
                    break;
            }

            return;
        }

        foreach (var client in _connectedClients)
        {
            if (client.Value.Client.LocalEndPoint is not IPEndPoint localEndpoint ||
                client.Value.Client.RemoteEndPoint is not IPEndPoint remoteEndpoint)
            {
                return;
            }

            var state = TcpStatus.GetState(remoteEndpoint.Port, localEndpoint.Port);

            if (state != ConnectionStatus.Disconnecting)
            {
                continue;
            }

            client.Value.Close();
            _connectedClients.Remove(client.Key);
            OnUserDisconnect?.Invoke(this, new UserDisconnectedEventArgs
            {
                ClientId = client.Key
            });
        }
    }

    public bool IsClient => _client is not null;

    public bool IsServer => _listener is not null;

    internal void Stop()
    {
        if (_settings.NetworkMode == NetworkMode.Server)
        {
            _logger.LogInformation("Stop listener");
            _listener.Stop();
            _listener = null;

            return;
        }

        _logger.LogInformation("Close remote connection");
        _client.Close();
        _client = null;
    }
}
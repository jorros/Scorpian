using System;
using System.Threading.Tasks;
using LiteNetwork.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scorpian.Network.Packets;

namespace Scorpian.Network;

internal class LiteNetworkClient : INetworkClient
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LiteNetworkClient> _logger;
    private readonly PacketManager _packetManager;
    private LiteTcp _client;
    private readonly NetworkQueue _queue;
    private string _auth;

    public LiteNetworkClient(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<LiteNetworkClient>>();
        _packetManager = serviceProvider.GetRequiredService<PacketManager>();
        _queue = serviceProvider.GetRequiredService<NetworkQueue>();
    }

    private void OnConnected()
    {
        Send(new LoginRequestPacket(_auth));
    }

    private void OnDisconnected()
    {
        _queue.Enqueue(new UserDisconnectedEventArgs(ClientId));
        ClientId = 0;
    }

    private async Task HandleMessageAsync(byte[] packetBuffer)
    {
        _logger.LogDebug("Receiving data from server");

        var packet = _packetManager.Deserialize(packetBuffer);

        if (packet is LoginResponsePacket loginResponse)
        {
            if (!loginResponse.Succeeded)
            {
                _queue.Enqueue(new AuthenticationFailedEventArgs(loginResponse.Reason));
                await Disconnect();
                return;
            }

            ClientId = loginResponse.ClientId;
            _queue.Enqueue(new UserConnectedEventArgs(loginResponse.ClientId));

            return;
        }

        _queue.Enqueue(new DataReceivedEventArgs
        {
            Data = packet,
            ClientId = 0
        });
    }

    public Task Start()
    {
        return Task.CompletedTask;
    }

    public async Task Stop()
    {
        await Disconnect();
    }

    public async Task Connect(string host, int port, string auth)
    {
        var options = new LiteClientOptions
        {
            Host = host,
            Port = port
        };
        _auth = auth;

        _client = new LiteTcp(options, _serviceProvider, OnConnected, OnDisconnected, HandleMessageAsync);

        await _client.ConnectAsync();
    }

    public async Task Disconnect()
    {
        await _client.DisconnectAsync();
        _client.Dispose();
    }

    public bool IsConnected => _client?.Socket?.Connected == true;
    public uint ClientId { get; private set; }

    public void Send<T>(T packet) where T : INetworkPacket
    {
        var data = _packetManager.Serialize(packet);

        _client?.Send(data);
    }

    private class LiteTcp : LiteClient
    {
        private readonly Action _onConnected;
        private readonly Action _onDisconnected;
        private readonly Func<byte[], Task> _onHandle;

        public LiteTcp(LiteClientOptions options, IServiceProvider serviceProvider, Action onConnected,
            Action onDisconnected, Func<byte[], Task> onHandle) : base(options, serviceProvider)
        {
            _onConnected = onConnected;
            _onDisconnected = onDisconnected;
            _onHandle = onHandle;
        }

        protected override void OnError(Exception exception)
        {
            Console.WriteLine("Error");
        }

        protected override void OnError(object sender, Exception exception)
        {
            Console.WriteLine("Error");
        }

        protected override void OnConnected()
        {
            _onConnected();
        }

        protected override void OnDisconnected()
        {
            _onDisconnected();
        }

        public override Task HandleMessageAsync(byte[] packetBuffer)
        {
            return _onHandle(packetBuffer);
        }
    }
}
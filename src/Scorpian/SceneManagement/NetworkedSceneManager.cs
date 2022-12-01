using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scorpian.Network;
using Scorpian.Network.Packets;

namespace Scorpian.SceneManagement;

public class NetworkedSceneManager : DefaultSceneManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly NetworkManager _networkManager;
    private readonly ILogger<NetworkedSceneManager> _logger;
    private readonly ScenePacketManager _scenePacketManager;

    public NetworkedSceneManager(IServiceProvider serviceProvider, NetworkManager networkManager,
        ILogger<NetworkedSceneManager> logger, ScenePacketManager scenePacketManager) : base(
        serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _networkManager = networkManager;
        _logger = logger;
        _scenePacketManager = scenePacketManager;
        _networkManager.OnPacketReceive += OnPacketReceive;
    }

    private void OnPacketReceive(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is not ISyncPacket syncPacket)
        {
            return;
        }

        _scenePacketManager.Process(syncPacket, this, e.ClientId);
    }

    public override T Load<T>()
    {
        var scene = Activator.CreateInstance(typeof(T), true) as NetworkedScene;
        scene?.Load(_serviceProvider);

        loadedScenes.Add(typeof(T).Name, scene);

        var baseScene = (Scene) scene;

        return (T) baseScene;
    }

    public override void Switch(string scene, bool unloadCurrent = true)
    {
        if (_networkManager.IsClient)
        {
            base.Switch(scene);
            
            _logger.LogInformation("Switching scene to {Scene}", scene);

            return;
        }

        _logger.LogInformation("Force all clients to switch scene to {Scene}", scene);

        _networkManager.Send(new SwitchScenePacket
        {
            Scene = scene
        });

        base.Switch(scene, unloadCurrent);
    }

    internal override async Task Update()
    {
        if (currentScene is not NetworkedScene scene)
        {
            return;
        }
        
        await scene.Update();
    }
}
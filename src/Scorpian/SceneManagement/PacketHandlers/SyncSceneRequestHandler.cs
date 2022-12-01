using System;
using Scorpian.Network;
using Scorpian.Network.Packets;

namespace Scorpian.SceneManagement.PacketHandlers;

public class SyncSceneRequestHandler : IPacketHandler
{
    private readonly NetworkManager _networkManager;
    public Type Type => typeof(SyncSceneRequestPacket);
    public NetworkMode Receiver => NetworkMode.Server;

    public SyncSceneRequestHandler(NetworkManager networkManager)
    {
        _networkManager = networkManager;
    }
    
    public void Process(ISyncPacket syncPacket, NetworkedScene networkedScene, uint senderId, NetworkedSceneManager sceneManager)
    {
        if (networkedScene is null)
        {
            return;
        }
        
        foreach (var node in networkedScene.networkedNodes)
        {
            _networkManager.Send(new CreateNodePacket
            {
                NetworkId = node.Key,
                Node = node.Value.GetType().Name,
                Scene = GetType().Name
            });
        }
    }
}
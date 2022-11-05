using System;
using Microsoft.Extensions.Logging;
using Scorpian.Network;
using Scorpian.Network.Packets;

namespace Scorpian.SceneManagement.PacketHandlers;

public class SyncVarHandler : IPacketHandler
{
    private readonly ILogger<SyncVarHandler> _logger;
    public Type Type => typeof(SyncVarPacket);
    public NetworkMode Receiver => NetworkMode.Client | NetworkMode.Server;

    public SyncVarHandler(ILogger<SyncVarHandler> logger)
    {
        _logger = logger;
    }
    
    public void Process(ISyncPacket syncPacket, NetworkedScene networkedScene, ushort senderId, NetworkedSceneManager sceneManager)
    {
        if (networkedScene is null)
        {
            return;
        }
        
        var packet = (SyncVarPacket) syncPacket;
        NetworkedNode node = null;

        if (packet.NodeId > 0)
        {
            node = networkedScene.GetNetworkedNode(packet.NodeId);

            if (node is null)
            {
                _logger.LogWarning("Syncing var to non existent entity");
                return;
            }
        }

        if (node is null)
        {
            ProcessScene(packet, networkedScene);
            
            return;
        }
        
        ProcessNode(packet, node);
    }

    private void ProcessNode(SyncVarPacket packet, NetworkedNode node)
    {
        var field = node.NetworkedFieldManager.GetVar(packet.Field);
        dynamic netVar = field.GetValue(node);
        netVar!.Accept(packet.Value);
    }
    
    private void ProcessScene(SyncVarPacket packet, NetworkedScene scene)
    {
        var field = scene.NetworkedFieldManager.GetVar(packet.Field);
        dynamic netVar = field.GetValue(scene);
        netVar!.Accept(packet.Value);
    }
}
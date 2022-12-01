using System;
using System.Linq;
using Scorpian.Network;
using Scorpian.Network.Packets;
using Scorpian.Network.RPC;

namespace Scorpian.SceneManagement.PacketHandlers;

public class RemoteCallHandler : IPacketHandler
{
    private readonly NetworkManager _networkManager;
    public Type Type => typeof(RemoteCallPacket);
    public NetworkMode Receiver => NetworkMode.Client | NetworkMode.Server;

    public RemoteCallHandler(NetworkManager networkManager)
    {
        _networkManager = networkManager;
    }
    
    public void Process(ISyncPacket syncPacket, NetworkedScene networkedScene, uint senderId, NetworkedSceneManager sceneManager)
    {
        if (networkedScene is null)
        {
            return;
        }
        
        var packet = (RemoteCallPacket) syncPacket;
        NetworkedNode node = null;

        if (packet.NodeId > 0)
        {
            node = networkedScene.GetNetworkedNode(packet.NodeId);
        }

        if (node is null)
        {
            ProcessScene(packet, networkedScene, senderId);
            
            return;
        }
        
        ProcessNode(packet, node, senderId);
    }

    private void ProcessNode(RemoteCallPacket packet, NetworkedNode node, uint senderId)
    {
        var method = node.RpcManager.Get(packet.Method, _networkManager.IsClient);

        var onlySenderInfo = method.GetParameters().FirstOrDefault()?.ParameterType == typeof(SenderInfo);

        var args = method.GetParameters().Length switch
        {
            0 => null,
            1 => onlySenderInfo
                ? new object[] {new SenderInfo(senderId)}
                : new[] {packet.Arguments},
            2 => new[] {packet.Arguments, new SenderInfo(senderId)},
            _ => throw new ArgumentOutOfRangeException()
        };

        method.Invoke(node, args);
    }

    private void ProcessScene(RemoteCallPacket packet, NetworkedScene scene, uint senderId)
    {
        var method = scene.RpcManager.Get(packet.Method, _networkManager.IsClient);

        var onlySenderInfo = method.GetParameters().FirstOrDefault()?.ParameterType == typeof(SenderInfo);

        var args = method.GetParameters().Length switch
        {
            0 => null,
            1 => onlySenderInfo
                ? new object[] {new SenderInfo(senderId)}
                : new[] {packet.Arguments},
            2 => new[] {packet.Arguments, new SenderInfo(senderId)},
            _ => throw new ArgumentOutOfRangeException()
        };

        method.Invoke(scene, args);
    }
}
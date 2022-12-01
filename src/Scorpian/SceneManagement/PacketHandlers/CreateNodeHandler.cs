using System;
using Scorpian.Network;
using Scorpian.Network.Packets;

namespace Scorpian.SceneManagement.PacketHandlers;

public class CreateNodeHandler : IPacketHandler
{
    public Type Type => typeof(CreateNodePacket);
    public NetworkMode Receiver => NetworkMode.Client;

    public void Process(ISyncPacket syncPacket, NetworkedScene networkedScene, uint senderId, NetworkedSceneManager sceneManager)
    {
        if (networkedScene is null)
        {
            return;
        }
        
        var packet = (CreateNodePacket) syncPacket;
        
        networkedScene.SpawnNode(packet.Node, packet.NetworkId, packet.Variables);
        Console.WriteLine("Received spawn");
    }
}
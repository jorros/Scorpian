using System;
using Scorpian.Network;
using Scorpian.Network.Packets;

namespace Scorpian.SceneManagement.PacketHandlers;

public interface IPacketHandler
{
    Type Type { get; }
    
    NetworkMode Receiver { get; }

    void Process(ISyncPacket syncPacket, NetworkedScene networkedScene, uint senderId, NetworkedSceneManager sceneManager);
}
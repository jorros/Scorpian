using Scorpian.Network;
using Scorpian.Network.Protocol;
using Scorpian.Network.RPC;

namespace Scorpian.SceneManagement;

public abstract class NetworkedNode : Node
{
    public ulong NetworkId { get; private set; }
    protected NetworkManager NetworkManager => (Scene as NetworkedScene)?.NetworkManager;
    
    internal RpcManager RpcManager { get; private set; }
    internal NetworkedFieldManager NetworkedFieldManager { get; private set; }
    
    internal void Create(ulong networkId)
    {
        NetworkId = networkId;

        RpcManager = new RpcManager(NetworkManager, this);
        NetworkedFieldManager = new NetworkedFieldManager(NetworkManager, this);
    }
    
    public void Invoke<T>(string name, T args, ushort clientId = 0)
    {
        RpcManager.Invoke(name, args, clientId);
    }

    internal override void Update()
    {
        if (NetworkManager.IsClient)
        {
            base.Update();

            return;
        }

        NetworkedFieldManager.Update();

        base.Update();
    }

    public void Invoke(string name, ushort clientId = 0)
    {
        Invoke<object>(name, null, clientId);
    }
}
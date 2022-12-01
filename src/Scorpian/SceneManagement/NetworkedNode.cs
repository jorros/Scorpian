using System;
using System.Threading.Tasks;
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
    
    public void Invoke<T>(string name, T args, uint? clientId = null)
    {
        RpcManager.Invoke(name, args, clientId);
    }

    internal override async Task Update()
    {
        if (NetworkManager.IsClient)
        {
            await base.Update();

            return;
        }

        NetworkedFieldManager.Update();

        await base.Update();
    }

    public void Invoke(string name, uint? clientId = null)
    {
        Invoke<object>(name, null, clientId);
    }
}
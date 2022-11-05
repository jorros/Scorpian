using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Scorpian.Network;
using Scorpian.Network.Packets;
using Scorpian.Network.Protocol;
using Scorpian.Network.RPC;

namespace Scorpian.SceneManagement;

public abstract class NetworkedScene : Scene
{
    public NetworkManager NetworkManager { get; private set; }
    public EngineSettings Settings { get; set; }

    internal readonly Dictionary<ulong, NetworkedNode> networkedNodes = new();
    private ulong _lastNetworkId = 1;
    
    internal RpcManager RpcManager { get; private set; }
    internal NetworkedFieldManager NetworkedFieldManager { get; private set; }

    protected T SpawnNode<T>(Action<T> configure = null) where T : NetworkedNode
    {
        if (NetworkManager.IsClient)
        {
            return null;
        }

        var node = (T) CreateNode(typeof(T), obj => configure?.Invoke(obj as T));
        
        node.Create(_lastNetworkId);
        node.NetworkedFieldManager.CommitVariables();
        node.OnInit();

        var packet = new CreateNodePacket
        {
            Node = typeof(T).Name,
            Scene = GetType().Name,
            NetworkId = _lastNetworkId,
            // Lists = node.NetworkedFieldManager.GetLists().Select(x => new SyncListPacket
            // {
            //     Action = NetworkedListAction.Sync,
            //     Field = x.Key,
            //     List = ((NetworkedList<>)x.Value).Cast<object>().ToList(),
            //     Scene = GetType().Name,
            //     NodeId = _lastNetworkId
            // }).ToArray(),
            Variables = node.NetworkedFieldManager.GetVariables().Select(x => new SyncVarPacket
            {
                Field = x.Key,
                NodeId = _lastNetworkId,
                Scene = GetType().Name,
                Value = x.Value
            }).ToArray()
        };
        NetworkManager.Send(packet);

        networkedNodes.Add(_lastNetworkId, node);

        _lastNetworkId++;

        return node;
    }

    private void SyncNodes()
    {
        if (NetworkManager.IsServer)
        {
            return;
        }

        foreach (var node in networkedNodes.Where(node => node.Value is NetworkedNode))
        {
            networkedNodes.Remove(node.Key);
        }

        NetworkManager.Send(new SyncSceneRequestPacket
        {
            Scene = GetType().Name
        });
    }

    internal void SpawnNode(string name, ulong id)
    {
        foreach (var node in from nodeType in Settings.NetworkedNodes
                 where nodeType.Name == name
                 select CreateNode(nodeType) as NetworkedNode)
        {
            node?.Create(id);
            node?.OnInit();

            networkedNodes.Add(id, node);
            break;
        }
    }

    internal NetworkedNode GetNetworkedNode(ulong id)
    {
        networkedNodes.TryGetValue(id, out var node);

        return node;
    }

    private void OnUserConnect(object sender, UserConnectedEventArgs e)
    {
        SyncNodes();
    }

    internal override void Load(IServiceProvider serviceProvider)
    {
        NetworkManager = serviceProvider.GetRequiredService<NetworkManager>();
        Settings = serviceProvider.GetRequiredService<EngineSettings>();
        RpcManager = new RpcManager(NetworkManager, this);
        NetworkedFieldManager = new NetworkedFieldManager(NetworkManager, this);
        
        NetworkManager.OnUserConnect += OnUserConnect;
        
        SyncNodes();

        base.Load(serviceProvider);

        if (NetworkManager.IsServer)
        {
            ServerOnLoad();
        }
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

    protected virtual void ServerOnLoad()
    {
    }

    public void Invoke<T>(string name, T args, ushort clientId = 0)
    {
        RpcManager.Invoke(name, args, clientId);
    }

    public void Invoke(string name, ushort clientId = 0)
    {
        Invoke<object>(name, null, clientId);
    }

    public override void Dispose()
    {
        NetworkManager.OnUserConnect -= OnUserConnect;
        networkedNodes.Clear();

        base.Dispose();
    }
}
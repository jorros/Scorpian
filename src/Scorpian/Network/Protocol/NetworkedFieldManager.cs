using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Scorpian.Helper;
using Scorpian.Network.Packets;
using Scorpian.SceneManagement;

namespace Scorpian.Network.Protocol;

public sealed class NetworkedFieldManager
{
    private readonly NetworkManager _networkManager;
    private readonly object _target;
    private readonly IDictionary<int, FieldInfo> _networkedVars;
    private readonly IDictionary<int, FieldInfo> _networkedLists;
    
    public NetworkedFieldManager(NetworkManager networkManager, object target)
    {
        _networkManager = networkManager;
        _target = target;
        _networkedLists = GetNetworkedLists(target.GetType());
        _networkedVars = GetNetworkedFields(target.GetType());
    }

    internal FieldInfo GetList(int field)
    {
        return _networkedLists[field];
    }

    internal FieldInfo GetVar(int field)
    {
        return _networkedVars[field];
    }

    private static Dictionary<int, FieldInfo> GetNetworkedFields(Type type)
    {
        var vars = new Dictionary<int, FieldInfo>();

        foreach (var field in type.GetRuntimeFields().Where(t =>
                     t.FieldType.IsGenericType && t.FieldType.GetGenericTypeDefinition() == typeof(NetworkedVar<>)))
        {
            vars.Add(field.Name.GetDeterministicHashCode(), field);
        }

        return vars;
    }

    private static Dictionary<int, FieldInfo> GetNetworkedLists(Type type)
    {
        var vars = new Dictionary<int, FieldInfo>();

        foreach (var field in type.GetRuntimeFields().Where(t =>
                     t.FieldType.IsGenericType && t.FieldType.GetGenericTypeDefinition() == typeof(NetworkedList<>)))
        {
            vars.Add(field.Name.GetDeterministicHashCode(), field);
        }

        return vars;
    }

    public IDictionary<int, object> GetVariables()
    {
        var dict = new Dictionary<int, object>();
        foreach (var var in _networkedVars)
        {
            dynamic field = var.Value.GetValue(_target);

            dict.Add(var.Key, field.Value);
        }

        return dict;
    }

    public void CommitVariables()
    {
        foreach (var netVar in _networkedVars)
        {
            dynamic field = netVar.Value.GetValue(_target);

            if (field is null || !field.IsDirty)
            {
                continue;
            }

            var change = field.GetProposedVal();
            field.Accept(change);
        }
    }
    
    public IDictionary<int, object> GetLists()
    {
        var dict = new Dictionary<int, object>();
        foreach (var var in _networkedLists)
        {
            dynamic field = var.Value.GetValue(_target);

            dict.Add(var.Key, field);
        }

        return dict;
    }

    public void Update()
    {
        ulong nodeId = 0;
        string scene;

        if (_target is NetworkedNode networkedNode)
        {
            nodeId = networkedNode.NetworkId;
            scene = networkedNode.Scene.GetType().Name;
        }
        else
        {
            scene = _target.GetType().Name;
        }
        
        foreach (var netVar in _networkedVars)
        {
            dynamic field = netVar.Value.GetValue(_target);

            if (field is null || !field.IsDirty)
            {
                continue;
            }

            var change = field.GetProposedVal();
            field.Accept(change);

            var packet = new SyncVarPacket
            {
                Field = netVar.Key,
                NodeId = nodeId,
                Scene = scene,
                Value = change
            };
            
            foreach (var client in _networkManager.ConnectedClients)
            {
                if (field.shouldReceive?.Invoke(client) != false)
                {
                    _networkManager.Send(packet, client);
                }
            }
        }

        foreach (var netList in _networkedLists)
        {
            dynamic field = netList.Value.GetValue(_target);

            if (field is null)
            {
                continue;
            }

            Queue<SyncListPacket> queue = field.packets;
            while (queue.Count > 0)
            {
                var packet = queue.Dequeue();

                var toBeSend = packet with {Field = netList.Key, NodeId = nodeId, Scene = scene};
                
                foreach (var client in _networkManager.ConnectedClients)
                {
                    if (field.shouldReceive?.Invoke(client) != false)
                    {
                        _networkManager.Send(toBeSend, client);
                    }
                }

                field.Commit(packet);
            }
        }
    }
}
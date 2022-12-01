using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Scorpian.Helper;
using Scorpian.Network.Packets;

namespace Scorpian.Network.RPC;

public class RpcManager
{
    private readonly NetworkManager _networkManager;
    private readonly object _target;
    private IDictionary<int, MethodBase> _clientRpcs;
    private IDictionary<int, MethodBase> _serverRpcs;

    public RpcManager(NetworkManager networkManager, object target)
    {
        _networkManager = networkManager;
        _target = target;
        _clientRpcs = RetrieveRpcs(target.GetType(), typeof(ClientRpcAttribute));
        _serverRpcs = RetrieveRpcs(target.GetType(), typeof(ServerRpcAttribute));
    }

    public void Invoke<T>(string name, T args, uint? clientId = null)
    {
        _networkManager.Send(new RemoteCallPacket
        {
            Arguments = args,
            Method = name.GetDeterministicHashCode(),
            Scene = _target.GetType().Name,
            NodeId = 0
        }, clientId);
    }

    public MethodBase Get(int nameHashCode, bool client)
    {
        return client
            ? _clientRpcs[nameHashCode]
            : _serverRpcs[nameHashCode];
    }
    
    private static Dictionary<int, MethodBase> RetrieveRpcs(Type type, Type attribute)
    {
        var rpcs = new Dictionary<int, MethodBase>();

        foreach (var method in type.GetRuntimeMethods()
                     .Where(x => Attribute.IsDefined(x, attribute)))
        {
            if (method.GetParameters().Length > 2)
            {
                throw new EngineException($"Malformed RPC method: {method.Name} on {type.Name}");
            }

            rpcs.Add(method.Name.GetDeterministicHashCode(), method);
        }

        return rpcs;
    }
}
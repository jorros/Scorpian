using System.Collections.Generic;
using LiteNetwork;

namespace Scorpian.Network;

public class LiteConnectionManager : IConnectionManager<LiteConnection>
{
    private readonly Dictionary<uint, LiteConnection> _connections;
    private uint _lastId = 1;

    public LiteConnectionManager()
    {
        _connections = new Dictionary<uint, LiteConnection>();
    }

    public uint Add(LiteConnection connection)
    {
        _connections.Add(_lastId, connection);
        _lastId++;

        return _lastId - 1;
    }

    public void Remove(uint id)
    {
        _connections.Remove(id);
    }

    public LiteConnection Get(uint id)
    {
        return _connections[id];
    }

    public IEnumerable<uint> IDs => _connections.Keys;
}
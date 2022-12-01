using System.Collections.Generic;

namespace Scorpian.Network;

public interface IConnectionManager<T>
{
    uint Add(T connection);

    void Remove(uint id);

    T Get(uint id);

    IEnumerable<uint> IDs { get; }
}
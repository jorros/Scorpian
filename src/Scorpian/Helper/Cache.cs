using System;
using System.Collections.Generic;

namespace Scorpian.Helper;

internal class Cache<K, V>
{
    private readonly Dictionary<K, V> _data = new();

    public V Get(K key, Func<V> value)
    {
        if (_data.ContainsKey(key))
        {
            return _data[key];
        }
        
        _data[key] = value();

        return _data[key];
    }

    public void Clear()
    {
        _data.Clear();
    }
}
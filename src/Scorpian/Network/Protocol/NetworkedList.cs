using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scorpian.Network.Packets;

namespace Scorpian.Network.Protocol;

public class NetworkedList<T> : IList<T>
{
    internal readonly Func<ushort, bool> shouldReceive;
    private readonly List<T> _list;
    internal readonly Queue<SyncListPacket> packets;

    public event EventHandler<ListChangedEventArgs<T>> OnChange;

    public NetworkedList(Func<ushort, bool> shouldReceive = null) : this(new List<T>(), shouldReceive)
    {
    }
    
    public NetworkedList(IEnumerable<T> list, Func<ushort, bool> shouldReceive = null)
    {
        this.shouldReceive = shouldReceive;
        packets = new Queue<SyncListPacket>();
        _list = list.ToList();
    }

    public void Commit(SyncListPacket packet)
    {
        switch (packet.Action)
        {
            case NetworkedListAction.Add:
                _list.Add((T) packet.Value);
                OnChange?.Invoke(this,
                    new ListChangedEventArgs<T>
                    {
                        Action = NetworkedListAction.Add, 
                        Index = _list.Count - 1, 
                        Value = (T) packet.Value
                    });
                break;

            case NetworkedListAction.Clear:
                _list.Clear();
                OnChange?.Invoke(this,
                    new ListChangedEventArgs<T>
                    {
                        Action = NetworkedListAction.Clear
                    });
                break;

            case NetworkedListAction.Insert:
                _list.Insert(packet.Index, (T) packet.Value);
                OnChange?.Invoke(this,
                    new ListChangedEventArgs<T>
                    {
                        Action = NetworkedListAction.Insert, 
                        Index = packet.Index, 
                        Value = (T) packet.Value
                    });
                break;

            case NetworkedListAction.Remove:
                _list.Remove((T) packet.Value);
                OnChange?.Invoke(this,
                    new ListChangedEventArgs<T>
                    {
                        Action = NetworkedListAction.Remove,
                        Value = (T) packet.Value
                    });
                break;

            case NetworkedListAction.Set:
                _list[packet.Index] = (T) packet.Value;
                OnChange?.Invoke(this,
                    new ListChangedEventArgs<T>
                    {
                        Action = NetworkedListAction.Set, 
                        Index = packet.Index, 
                        Value = (T) packet.Value
                    });
                break;

            case NetworkedListAction.RemoveAt:
                _list.RemoveAt(packet.Index);
                OnChange?.Invoke(this,
                    new ListChangedEventArgs<T>
                    {
                        Action = NetworkedListAction.RemoveAt, 
                        Index = packet.Index, 
                        Value = (T) packet.Value
                    });
                break;

            case NetworkedListAction.Sync:
                _list.Clear();
                _list.AddRange(packet.List.Cast<T>());
                OnChange?.Invoke(this,
                    new ListChangedEventArgs<T>
                    {
                        Action = NetworkedListAction.Sync
                    });
                break;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T item)
    {
        packets.Enqueue(new SyncListPacket
        {
            Action = NetworkedListAction.Add,
            Value = item
        });
    }

    public void Clear()
    {
        packets.Enqueue(new SyncListPacket
        {
            Action = NetworkedListAction.Clear
        });
    }

    public bool Contains(T item)
    {
        return _list.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        packets.Enqueue(new SyncListPacket
        {
            Action = NetworkedListAction.Remove,
            Value = item
        });

        return true;
    }

    public int Count => _list.Count;
    public bool IsReadOnly { get; }

    public int IndexOf(T item)
    {
        return _list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        packets.Enqueue(new SyncListPacket
        {
            Action = NetworkedListAction.Insert,
            Index = index,
            Value = item
        });
    }

    public void RemoveAt(int index)
    {
        packets.Enqueue(new SyncListPacket
        {
            Action = NetworkedListAction.RemoveAt,
            Index = index
        });
    }

    public T this[int index]
    {
        get => _list[index];
        set =>
            packets.Enqueue(new SyncListPacket
            {
                Action = NetworkedListAction.Set,
                Value = value
            });
    }
}
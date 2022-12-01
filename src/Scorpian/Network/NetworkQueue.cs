using System;
using System.Collections.Concurrent;

namespace Scorpian.Network;

public class NetworkQueue
{
    private readonly ConcurrentQueue<EventArgs> _queue;

    public NetworkQueue()
    {
        _queue = new ConcurrentQueue<EventArgs>();
    }

    public void Enqueue(EventArgs evt)
    {
        _queue.Enqueue(evt);
    }

    public EventArgs Dequeue()
    {
        _queue.TryDequeue(out var evt);

        return evt;
    }

    public bool HasAny => !_queue.IsEmpty;
}
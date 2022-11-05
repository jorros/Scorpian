using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Scorpian.SceneManagement;

public class EventManager
{
    private readonly Dictionary<string, Dictionary<int, Action<object[]>>> _eventDictionary = new();
    
    internal void RegisterAll(object instance)
    {
        var type = instance.GetType();
        foreach (var method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public |
                                               BindingFlags.NonPublic))
        {
            var attribute = method.GetCustomAttribute(typeof(EventAttribute));

            if (attribute is null)
            {
                continue;
            }

            var eventAttribute = (EventAttribute) attribute;

            void RunMethod(object[] x)
            {
                method.Invoke(instance, x);
            }

            if (_eventDictionary.TryGetValue(eventAttribute.Name, out var events))
            {
                if (!events.TryAdd(instance.GetHashCode(), RunMethod))
                {
                    events[instance.GetHashCode()] = RunMethod;
                }
            }
            else
            {
                events = new Dictionary<int, Action<object[]>>
                {
                    [instance.GetHashCode()] = RunMethod
                };
                _eventDictionary.Add(eventAttribute.Name, events);
            }
        }
    }

    internal void RemoveAll(object instance)
    {
        var type = instance.GetType();
        foreach (var method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public |
                                               BindingFlags.NonPublic))
        {
            var attribute = method.GetCustomAttribute(typeof(EventAttribute));

            if (attribute is null)
            {
                continue;
            }

            var eventAttribute = (EventAttribute) attribute;

            if (_eventDictionary.TryGetValue(eventAttribute.Name, out var events))
            {
                events.Remove(instance.GetHashCode());
            }
        }
    }
    
    public void Trigger(string eventName, params object[] message)
    {
        if (!_eventDictionary.TryGetValue(eventName, out var events))
        {
            return;
        }

        foreach (var evt in events)
        {
            evt.Value(message.Any() ? message : null);
        }
    }
}
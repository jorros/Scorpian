using System;

namespace Scorpian.SceneManagement;

[AttributeUsage(AttributeTargets.Method)]
public class EventAttribute : Attribute
{
    public string Name { get; }

    public EventAttribute(string name)
    {
        Name = name;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scorpian.SceneManagement;

public abstract class Node : Component
{
    protected ulong Identifier { get; private set; }
    public Scene Scene { get; private set; }
    internal IEnumerable<Component> Components { get; } = new List<Component>();

    protected void AttachComponent(Component component)
    {
        ((List<Component>) Components).Add(component);
        component.Init(this, ServiceProvider);
        component.OnInit();
    }

    protected T FindComponent<T>() where T : Component
    {
        return Components.FirstOrDefault(x => x.GetType() == typeof(T)) as T;
    }

    internal void Create(ulong identifier, IServiceProvider serviceProvider, Scene scene)
    {
        Identifier = identifier;
        Scene = scene;
        
        Init(this, serviceProvider);
    }
}
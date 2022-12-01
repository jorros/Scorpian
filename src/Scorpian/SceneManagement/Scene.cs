using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scorpian.Asset;
using Scorpian.Graphics;
using static Scorpian.SDL.SDL;

namespace Scorpian.SceneManagement;

public abstract class Scene : IDisposable
{
    public IServiceProvider ServiceProvider { get; private set; }
    public SceneManager SceneManager { get; private set; }
    public UserDataManager UserDataManager { get; private set; }
    public EventManager EventManager { get; private set; }
    public Camera Camera { get; private set; }
    public Dictionary<ulong, Node> Nodes { get; } = new();
    public ILogger Logger { get; private set; }

    private ulong _nodeIdCounter;
    
    public virtual Color BackgroundColor { get; } = Color.Firebrick;

    protected T CreateNode<T>(Action<T> configure = null) where T : Node
    {
        var node = (T)CreateNode(typeof(T), obj => configure?.Invoke(obj as T));
        
        node.OnInit();

        return node;
    }
    
    public T FindNode<T>() where T : Node
    {
        return Nodes.FirstOrDefault(x => x.Value is T).Value as T;
    }

    protected Node CreateNode(Type type, Action<Node> configure = null)
    {
        if (!type.IsAssignableTo(typeof(Node)))
        {
            return null;
        }
        
        var node = (Node)Activator.CreateInstance(type);
        configure?.Invoke(node);
        node?.Create(_nodeIdCounter, ServiceProvider, this);

        Nodes.Add(_nodeIdCounter, node);

        _nodeIdCounter++;
        
        return node;
    }
    
    internal void Render(RenderContext context)
    {
        var current = SDL_GetTicks64();

        foreach (var node in Nodes.Values)
        {
            var dT = (current - node.lastRender) / 1000f;
            node.OnRender(context, dT);
            node.lastRender = current;
            
            foreach (var component in node.Components)
            {
                dT = (current - component.lastRender) / 1000f;
                
                component.OnRender(context, dT);
                component.lastRender = current;
            }
        }
        
        OnRender(context);
    }

    internal virtual async Task Update()
    {
        await OnUpdate();

        foreach (var node in Nodes.Values)
        {
            foreach (var component in node.Components)
            {
                await component.Update();
            }

            await node.Update();
        }
    }

    internal async Task Tick()
    {
        await OnTick();

        foreach (var node in Nodes.Values)
        {
            foreach (var component in node.Components)
            {
                await component.Tick();
            }
            
            await node.Tick();
        }
    }

    internal virtual void Load(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        SceneManager = serviceProvider.GetRequiredService<SceneManager>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        Logger = loggerFactory.CreateLogger(GetType());
        UserDataManager = serviceProvider.GetRequiredService<UserDataManager>();
        EventManager = serviceProvider.GetRequiredService<EventManager>();

        var assetManager = serviceProvider.GetService<AssetManager>();
        
        EventManager.RegisterAll(this);

        if (assetManager is not null)
        {
            Camera = serviceProvider.GetRequiredService<RenderContext>().Camera;
            OnLoad(assetManager);
        }
    }

    protected abstract void OnLoad(AssetManager assetManager);

    protected virtual Task OnTick()
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnUpdate()
    {
        return Task.CompletedTask;
    }

    protected virtual void OnRender(RenderContext context)
    {
    }

    protected virtual Task OnLeave()
    {
        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
        EventManager.RemoveAll(this);
        
        foreach (var node in Nodes.Values)
        {
            node.Dispose();
        }

        Nodes.Clear();

        OnLeave();
    }
}
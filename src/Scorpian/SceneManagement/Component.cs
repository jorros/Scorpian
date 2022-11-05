using System;
using Microsoft.Extensions.DependencyInjection;
using Scorpian.Asset;
using Scorpian.Graphics;

namespace Scorpian.SceneManagement;

public abstract class Component : IDisposable
{
    protected IServiceProvider ServiceProvider { get; private set; }
    protected AssetManager AssetManager { get; private set; }
    protected SceneManager SceneManager { get; private set; }
    protected UserDataManager UserDataManager { get; private set; }
    protected Camera Camera { get; private set; }
    protected EventManager EventManager { get; private set; }
    protected Node Parent { get; private set; }

    internal ulong lastRender;

    public virtual void OnCleanUp()
    {
    }
    
    public virtual void OnInit()
    {
    }
    
    public virtual void OnUpdate()
    {
    }

    internal virtual void Update()
    {
        OnUpdate();
    }

    public virtual void OnTick()
    {
    }

    internal virtual void Tick()
    {
        OnTick();
    }
    
    public virtual void OnRender(RenderContext context, float dT)
    {
    }

    internal void Init(Node parent, IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Parent = parent;

        Camera = ServiceProvider.GetService<RenderContext>()?.Camera;
        UserDataManager = ServiceProvider.GetRequiredService<UserDataManager>();
        SceneManager = ServiceProvider.GetRequiredService<SceneManager>();
        AssetManager = ServiceProvider.GetService<AssetManager>();
        EventManager = serviceProvider.GetRequiredService<EventManager>();
        EventManager.RegisterAll(this);
    }

    public virtual void Dispose()
    {
        EventManager.RemoveAll(this);
        OnCleanUp();
    }
}
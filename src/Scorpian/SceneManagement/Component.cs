using System;
using System.Threading.Tasks;
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

    public virtual Task OnCleanUp()
    {
        return Task.CompletedTask;
    }
    
    public virtual Task OnInit()
    {
        return Task.CompletedTask;
    }
    
    public virtual Task OnUpdate()
    {
        return Task.CompletedTask;
    }

    internal virtual async Task Update()
    {
        await OnUpdate();
    }

    public virtual Task OnTick()
    {
        return Task.CompletedTask;
    }

    internal virtual async Task Tick()
    {
        await OnTick();
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
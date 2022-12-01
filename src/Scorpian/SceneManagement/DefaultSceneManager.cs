using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Scorpian.Graphics;

namespace Scorpian.SceneManagement;

public class DefaultSceneManager : SceneManager
{
    private readonly IServiceProvider _serviceProvider;
    protected Scene currentScene;
    private readonly RenderContext _renderContext;
    private CancellationTokenSource _cancellationTokenSource;
    
    protected readonly Dictionary<string, Scene> loadedScenes = new();

    public DefaultSceneManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        _renderContext = serviceProvider.GetService<RenderContext>();
    }

    public override T Load<T>()
    {
        var scene = Activator.CreateInstance(typeof(T), true) as Scene;
        scene?.Load(_serviceProvider);

        loadedScenes.Add(typeof(T).Name, scene);

        return (T)scene;
    }

    internal override Scene GetLoadedScene(string name)
    {
        return loadedScenes.ContainsKey(name) ? loadedScenes[name] : null;
    }

    public override void Switch(string scene, bool unloadCurrent = true)
    {
        if (string.IsNullOrEmpty(scene) || !loadedScenes.ContainsKey(scene))
        {
            throw new EngineException($"Switching scene failed. {scene} is not loaded.");
        }

        if (unloadCurrent)
        {
            currentScene?.Dispose();
        }

        currentScene = loadedScenes[scene];
    }

    public override bool IsLoaded(string scene)
    {
        return loadedScenes.ContainsKey(scene);
    }

    public override void Quit()
    {
        _cancellationTokenSource?.Cancel();
    }

    public override Scene GetCurrentScene()
    {
        return currentScene;
    }

    internal override void SetCancellationToken(CancellationTokenSource source)
    {
        _cancellationTokenSource = source;
    }

    internal override async Task Tick()
    {
        await currentScene.Tick();
    }

    internal override async Task Update()
    {
        await currentScene.Update();
    }

    internal override void Render(TimeSpan elapsedTime)
    {
        _renderContext.Begin(currentScene.BackgroundColor);
        currentScene.Render(_renderContext);
        _renderContext.End();
    }
}
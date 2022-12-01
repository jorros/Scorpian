using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scorpian.SceneManagement;

public abstract class SceneManager
{
    public abstract T Load<T>() where T : Scene;
    public abstract void Switch(string scene, bool unloadCurrent = true);
    public abstract bool IsLoaded(string scene);
    internal abstract Scene GetLoadedScene(string name);
    public abstract void Quit();
    public abstract Scene GetCurrentScene();
    internal abstract void SetCancellationToken(CancellationTokenSource source);
    internal abstract Task Tick();
    internal abstract Task Update();
    internal abstract void Render(TimeSpan elapsedTime);
}
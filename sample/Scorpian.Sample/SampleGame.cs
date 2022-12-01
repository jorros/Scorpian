using Microsoft.Extensions.DependencyInjection;
using Scorpian.SceneManagement;

namespace Scorpian.Sample;

public class SampleGame : Engine
{
    protected override void Init(IServiceCollection services, List<Type> networkedNodes)
    {
    }

    protected override void Load(IServiceProvider serviceProvider)
    {
        var sceneManager = serviceProvider.GetRequiredService<SceneManager>();
        sceneManager.Load<SampleScene>();
        sceneManager.Switch(nameof(SampleScene));
    }
}
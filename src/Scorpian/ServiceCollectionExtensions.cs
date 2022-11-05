using Microsoft.Extensions.DependencyInjection;
using Scorpian.SceneManagement;

namespace Scorpian;

public static class ServiceCollectionExtensions
{
    public static void AddScene<T>(this IServiceCollection services) where T : Scene
    {
        services.AddTransient<T>();
    }
}
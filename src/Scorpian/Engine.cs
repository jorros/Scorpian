using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scorpian.Asset;
using Scorpian.Asset.AssetLoaders;
using Scorpian.Asset.Markup;
using Scorpian.Asset.SpriteSheetParsers;
using Scorpian.Graphics;
using Scorpian.Helper;
using Scorpian.InputManagement;
using Scorpian.Network;
using Scorpian.Network.Packets;
using Scorpian.SceneManagement;
using Scorpian.SceneManagement.PacketHandlers;
using static Scorpian.SDL.SDL;

namespace Scorpian;

public abstract class Engine
{
    private ServiceProvider _serviceProvider;
    private EngineSettings _settings;
    protected abstract void Init(IServiceCollection services, List<Type> networkedNodes);

    protected abstract Task Load(IServiceProvider serviceProvider);

    public IServiceProvider ServiceProvider => _serviceProvider;
    
    protected void AddNetworkPacketsFrom(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(INetworkPacket)) && !t.IsInterface))
        {
            _settings.NetworkPackets.Add(type.FullName.GetDeterministicHashCode16(), type);
        }
    }

    protected void SetAuthentication(Func<string, IServiceProvider, LoginResponsePacket> auth)
    {
        _settings.Authentication = auth;
    }

    private static Engine _engine;
    private static EngineSettings _engineSettings;

    // public unsafe void Run(EngineSettings settings, string[] argv)
    // {
    //     _engine = this;
    //     _engineSettings = settings;
    //
    //     delegate* unmanaged[Cdecl]<int, IntPtr, int> unmanagedPtr = (delegate* unmanaged[Cdecl]<int, IntPtr, int>)&RunUIKit;
    //     SDL_UIKitRunApp(argv.Length, IntPtr.Zero, unmanagedPtr);
    // }
    //
    // [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    // private static int RunUIKit(int argc, IntPtr argv)
    // {
    //     _engine.Run(_engineSettings).Wait();
    //
    //     return 0;
    // }

    public async Task Run(EngineSettings settings, IntPtr? viewHandler = null)
    {
        try
        {


            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.AddConsole(opt => opt.LogToStandardErrorThreshold = LogLevel.Error);
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            _settings = settings;
            services.AddSingleton(_ => settings);

            services.AddSingleton<UserDataManager>();
            services.AddSingleton<EventManager>();

            if (!settings.Headless)
            {
                services.AddSingleton<GraphicsManager>();
                services.AddSingleton<AssetManager>();
                services.AddSingleton<RenderContext>();
                services.AddSingleton<FontMarkupReader>();

                services.AddSingleton<IAssetLoader, SpriteLoader>();
                services.AddSingleton<IAssetLoader, FontLoader>();

                services.AddSingleton<LibgdxSpriteSheetParser>();
            }

            if (settings.NetworkEnabled)
            {
                services.AddSingleton<NetworkManager>();
                services.AddSingleton<PacketManager>();
                services.AddSingleton<NetworkQueue>();
                services.AddSingleton<LiteConnectionManager>();

                services.AddSingleton<SceneManager, NetworkedSceneManager>();
                services.AddSingleton<ScenePacketManager>();

                AddNetworkPacketsFrom(typeof(Engine).Assembly);
                services.RegisterAllTypes<IPacketHandler>(new[] {typeof(Engine).Assembly});
            }
            else
            {
                services.AddSingleton<SceneManager, DefaultSceneManager>();
            }

            Init(services, settings.NetworkedNodes);

            _serviceProvider = services.BuildServiceProvider();

            if (settings.NetworkEnabled)
            {
                var networkManager = _serviceProvider.GetRequiredService<NetworkManager>();
                await networkManager.Start();
            }

            var graphicsManager = _serviceProvider.GetService<GraphicsManager>();
            var sceneManager = _serviceProvider.GetRequiredService<SceneManager>();
            var assetManager = _serviceProvider.GetService<AssetManager>();
            var userDataManager = _serviceProvider.GetRequiredService<UserDataManager>();
            var renderContext = _serviceProvider.GetService<RenderContext>();
            var logger = _serviceProvider.GetRequiredService<ILogger<Engine>>();

            var running = new CancellationTokenSource();
            sceneManager.SetCancellationToken(running);

            userDataManager.Load();

            if (!settings.Headless)
            {
                graphicsManager?.Init(viewHandler);
                renderContext?.Init();

                var highRes = graphicsManager?.IsHighRes();

                var assetLoaders = _serviceProvider.GetServices<IAssetLoader>();
                assetManager?.Init(assetLoaders, highRes ?? false);
            }

            await Load(_serviceProvider);

            StartUpdate(logger, running.Token);
            StartTick(settings, logger, running.Token);

            if (settings.Headless)
            {
                while (!running.IsCancellationRequested)
                {
                    settings.HeadlessLoopAction?.Invoke();
                    Thread.Sleep(100);
                }

                var networkManager = _serviceProvider.GetRequiredService<NetworkManager>();
                await networkManager.Stop();

                return;
            }

            StartRender(graphicsManager, sceneManager, running);

            graphicsManager?.Quit();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }

    private void StartTick(EngineSettings settings, ILogger logger, CancellationToken token)
    {
        var cap = 1000 / (double) settings.TicksPerSecond;
        var sceneManager = _serviceProvider.GetRequiredService<SceneManager>();

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                var start = SDL_GetPerformanceCounter();

                try
                {
                    await sceneManager.Tick().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    logger.LogError("Failed to process tick: {Error}", e.Message);
                }

                var end = SDL_GetPerformanceCounter();
                var elapsedMs = (end - start) / (float) SDL_GetPerformanceFrequency() * 1000.0f;
                await Task.Delay(Math.Max((int) (cap - elapsedMs), 0), token);
            }
        }, token);
    }

    private void StartUpdate(ILogger logger, CancellationToken token)
    {
        var stopwatch = new Stopwatch();
        var sceneManager = _serviceProvider.GetRequiredService<SceneManager>();
        var networkManager = _serviceProvider.GetRequiredService<NetworkManager>();

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    stopwatch.Start();

                    Input.UpdateMouseState();
                    networkManager.RunQueue();

                    await sceneManager.Update();

                    await Task.Delay((int) Math.Floor(Math.Max(1000 / 30.0 - stopwatch.ElapsedMilliseconds, 0)), token);

                    stopwatch.Stop();
                    stopwatch.Reset();
                }
                catch (Exception e)
                {
                    logger.LogError("Failed to process update: {Error}", e.Message);
                }
            }
        }, token);
    }

    private void StartRender(GraphicsManager graphicsManager, SceneManager sceneManager,
        CancellationTokenSource cancellationTokenSource)
    {
        var stopwatch = new Stopwatch();

        var highDpi = graphicsManager.IsHighDpiMode();

        while (!cancellationTokenSource.IsCancellationRequested)
        {
            stopwatch.Start();
            var start = SDL_GetPerformanceCounter();

            while (SDL_PollEvent(out var e) == 1)
            {
                switch (e.type)
                {
                    case SDL_EventType.SDL_QUIT:
                        cancellationTokenSource.Cancel();
                        break;

                    case SDL_EventType.SDL_KEYUP:
                    case SDL_EventType.SDL_KEYDOWN:
                        Input.RaiseEvent(e.key);
                        break;

                    case SDL_EventType.SDL_MOUSEMOTION:
                        Input.CaptureMouseMotion(e.motion, highDpi);
                        break;

                    case SDL_EventType.SDL_MOUSEWHEEL:
                        Input.CaptureMouseWheel(e.wheel);
                        break;

                    case SDL_EventType.SDL_MOUSEBUTTONUP:
                    case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                        Input.CaptureMouseButton(e.button, highDpi);
                        break;

                    case SDL_EventType.SDL_TEXTINPUT:
                        var ch = new byte[Marshal.SizeOf<char>()];
                        unsafe
                        {
                            Marshal.Copy((IntPtr) e.text.text, ch, 0, Marshal.SizeOf<char>());
                        }

                        Input.RaiseTextInput(Encoding.UTF8.GetChars(ch)[0]);

                        break;
                }
            }
            sceneManager.Render(stopwatch.Elapsed);

            graphicsManager.Flush();

            var end = SDL_GetPerformanceCounter();
            var elapsed = (end - start) / (float) SDL_GetPerformanceFrequency();

            stopwatch.Stop();
            graphicsManager.FPS = (int) (1.0f / elapsed);
            Thread.Sleep((int) Math.Floor(Math.Max(4.0 - stopwatch.ElapsedMilliseconds, 0)));
            stopwatch.Reset();
        }
    }
}
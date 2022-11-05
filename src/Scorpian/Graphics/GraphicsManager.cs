using System;
using Microsoft.Extensions.Logging;
using Scorpian.Helper;
using static Scorpian.SDL.SDL;
using static Scorpian.SDL.SDL_image;
using static Scorpian.SDL.SDL_ttf;

namespace Scorpian.Graphics;

public class GraphicsManager
{
    private readonly UserDataManager _userDataManager;
    private readonly EngineSettings _settings;
    private readonly ILogger<GraphicsManager> _logger;

    public GraphicsManager(UserDataManager userDataManager, EngineSettings settings, ILogger<GraphicsManager> logger)
    {
        _userDataManager = userDataManager;
        _settings = settings;
        _logger = logger;
    }

    internal IntPtr Window { get; private set; }
    internal IntPtr Renderer { get; private set; }

    internal int FPS { get; set; }

    internal void Init(IntPtr? handle = null)
    {
        SDL_GetVersion(out var version);
        _logger.LogDebug("Starting (using SDL {Version})", $"{version.major}.{version.minor}.{version.patch}");
        if (SDL_Init(SDL_INIT_VIDEO) < 0)
        {
            _logger.LogCritical("Could not initialise SDL: {Error}", SDL_GetError());
            throw new EngineException($"Could not initialise SDL: {SDL_GetError()}");
        }

        if (handle is not null)
        {
            _logger.LogDebug("Use existing window");
            Window = SDL_CreateWindowFrom(handle.Value);
        }
        else
        {
            _logger.LogDebug("Create new window");
            var windowWidth = _userDataManager.Get("windowWidth", 1366);
            var windowHeight = _userDataManager.Get("windowHeight", 768);

            Window = SDL_CreateWindow(_settings.DisplayName, SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED,
                windowWidth, windowHeight,
                SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI);
        }

        if (Window == IntPtr.Zero)
        {
            _logger.LogCritical("Could not create window: {Error}", SDL_GetError());
            throw new EngineException($"Could not create window: {SDL_GetError()}");
        }

        Renderer = SDL_CreateRenderer(Window,
            -1,
            SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE);

        if (Renderer == IntPtr.Zero)
        {
            _logger.LogCritical("There was an issue creating the renderer: {Error}", SDL_GetError());
            throw new EngineException($"There was an issue creating the renderer: {SDL_GetError()}");
        }

        if (IMG_Init(IMG_InitFlags.IMG_INIT_PNG) == 0)
        {
            _logger.LogCritical("There was an issue initialising SDL_Image: {Error}", IMG_GetError());
            throw new EngineException($"There was an issue initialising SDL_Image: {IMG_GetError()}");
        }

        if (TTF_Init() == -1)
        {
            _logger.LogCritical("There was an issue initialising SDL_TTF: {Error}", TTF_GetError());
            throw new EngineException($"There was an issue initialising SDL_TTF: {TTF_GetError()}");
        }
    }

    internal bool IsHighDpiMode()
    {
        ErrorHandling.Handle(_logger, SDL_GetRendererOutputSize(Renderer, out var rw, out var rh));
        SDL_GetWindowSize(Window, out var ww, out var wh);

        return rw != ww;
    }

    internal bool IsHighRes()
    {
        ErrorHandling.Handle(_logger, SDL_GetRendererOutputSize(Renderer, out var rw, out var rh));

        return rw * rw > 3686400;
    }

    internal void Quit()
    {
        TTF_Quit();
        IMG_Quit();
        SDL_DestroyRenderer(Renderer);
        SDL_DestroyWindow(Window);
        SDL_Quit();
    }

    internal void Flush()
    {
        SDL_RenderPresent(Renderer);
    }
}
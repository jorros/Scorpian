using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.Logging;
using Scorpian.Asset;
using Scorpian.Asset.Font;
using Scorpian.Helper;
using Scorpian.Maths;
using static Scorpian.SDL.SDL;

namespace Scorpian.Graphics;

public class RenderContext
{
    private readonly GraphicsManager _graphicsManager;
    private readonly ILogger<RenderContext> _logger;
    public Camera Camera { get; private set; }
    private ulong _currentTick;
    private readonly Dictionary<Action, ulong> _timedActions;
    public int FPS => _graphicsManager.FPS;
    public Size DrawSize { get; private set; }

    public RenderContext(GraphicsManager graphicsManager, ILogger<RenderContext> logger)
    {
        _graphicsManager = graphicsManager;
        _logger = logger;
        _timedActions = new Dictionary<Action, ulong>();
    }

    internal void Init()
    {
        SDL_RenderGetViewport(_graphicsManager.Renderer, out var rect);
        Camera = new Camera(this, rect);

        DrawSize = new Size(rect.w, rect.h);
    }

    internal void Begin(Color clearColor, ScaleQuality scaleQuality = ScaleQuality.Nearest)
    {
        ErrorHandling.Handle(_logger,
            SDL_SetRenderDrawColor(_graphicsManager.Renderer, clearColor.R, clearColor.G, clearColor.B, 255));
        ErrorHandling.Handle(_logger, SDL_RenderClear(_graphicsManager.Renderer));

        SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, $"{(int) scaleQuality}");

        _currentTick = SDL_GetTicks64();
    }

    internal void End()
    {
    }

    public void InvokeIn(uint ms, Action action)
    {
        if (!_timedActions.ContainsKey(action))
        {
            _timedActions[action] = _currentTick + ms;
            return;
        }

        if (_timedActions[action] <= _currentTick)
        {
            action.Invoke();
            _timedActions.Remove(action);
        }
    }

    public void RunEvery(uint ms, Action action)
    {
        if (_currentTick % ms == 0)
        {
            action.Invoke();
        }
    }

    public void SetClipping(RectangleF? rectangle)
    {
        SetClipping(rectangle.HasValue
            ? new Rectangle((int) rectangle.Value.X, (int) rectangle.Value.Y, (int) rectangle.Value.Width,
                (int) rectangle.Value.Height)
            : null);
    }

    public void SetClipping(Rectangle? rectangle)
    {
        if (rectangle is null)
        {
            SDL_RenderSetClipRect(_graphicsManager.Renderer, IntPtr.Zero);

            return;
        }

        var rect = new SDL_Rect
        {
            x = rectangle.Value.Left,
            y = rectangle.Value.Top,
            h = rectangle.Value.Height,
            w = rectangle.Value.Width
        };

        SDL_RenderGetViewport(_graphicsManager.Renderer, out var viewport);

        if (viewport.x == rect.x && viewport.y == rect.y && viewport.w == rect.w && viewport.h == rect.h)
        {
            SDL_RenderSetClipRect(_graphicsManager.Renderer, IntPtr.Zero);

            return;
        }

        SDL_RenderSetClipRect(_graphicsManager.Renderer, ref rect);
    }

    public Rectangle GetClipping()
    {
        SDL_RenderGetClipRect(_graphicsManager.Renderer, out var rect);

        return new Rectangle
        {
            X = rect.x,
            Y = rect.y,
            Height = rect.h,
            Width = rect.w
        };
    }

    public RenderTargetSprite CreateRenderTarget(Size size)
    {
        var texture = SDL_CreateTexture(_graphicsManager.Renderer, SDL_PIXELFORMAT_ARGB8888,
            (int) SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, size.Width, size.Height);

        return new RenderTargetSprite(_graphicsManager, texture, size);
    }

    public void Draw(Sprite sprite, PointF position)
    {
        var dest = new RectangleF(position.X, position.Y, sprite.Size.Width, sprite.Size.Height);
        Draw(sprite, null, dest, 0, Color.White);
    }

    public void Draw(Sprite sprite, RectangleF dest, double angle, Color color, byte alpha, int index = -1,
        bool inWorld = true)
    {
        Draw(sprite, null, dest, angle, color, alpha, index, inWorld);
    }

    public void Draw(Sprite sprite, Rectangle? src, RectangleF dest, double angle, Color color, byte alpha = 255,
        int index = -1,
        bool inWorld = true)
    {
        if (inWorld)
        {
            var position = Camera.WorldToScreen(dest.Location.ToVector2());

            dest = new RectangleF(position.ToPointF(), Camera.GetSize(dest.Size));
        }

        sprite.Render(_graphicsManager, src, dest, angle, color, alpha, index);
    }

    public void DrawText(Font font, PointF position, string text, FontSettings settings, bool inWorld = true)
    {
        if (inWorld)
        {
            position = Camera.WorldToScreen(position.ToVector2()).ToPointF();
        }

        font.Render(new Point((int) position.X, (int) position.Y), text, settings);
    }

    public void DrawLine(PointF from, PointF to, Color color)
    {
        var f = Camera.WorldToScreen(from);
        var t = Camera.WorldToScreen(to);

        SDL_SetRenderDrawColor(_graphicsManager.Renderer, color.R, color.G, color.B, color.A);
        SDL_RenderDrawLineF(_graphicsManager.Renderer, f.X, f.Y, t.X, t.Y);
    }

    public void DrawRectangle(RectangleF rect, Color color, bool fill)
    {
        var target = rect.ToSdl();

        SDL_SetRenderDrawColor(_graphicsManager.Renderer, color.R, color.G, color.B, color.A);

        if (fill)
        {
            SDL_RenderFillRectF(_graphicsManager.Renderer, ref target);

            return;
        }

        SDL_RenderDrawRectF(_graphicsManager.Renderer, ref target);
    }

    public Sprite MergeSprites(IEnumerable<Sprite> sprites)
    {
        if (sprites is null)
        {
            return null;
        }

        var input = sprites.ToArray();
        var spriteSize = input.First().Size;

        var renderTarget = CreateRenderTarget(spriteSize);
        renderTarget.BeginDraw();

        renderTarget.Clear();

        foreach (var sprite in input)
        {
            sprite.Render(_graphicsManager, null, new RectangleF(0, 0, spriteSize.Width, spriteSize.Height), 0,
                Color.White, 255, -1);
        }

        renderTarget.EndDraw();

        return renderTarget;
    }
}
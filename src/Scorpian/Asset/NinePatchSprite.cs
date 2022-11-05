using System;
using System.Drawing;
using Scorpian.Graphics;
using Scorpian.Helper;
using static Scorpian.SDL.SDL;

namespace Scorpian.Asset;

internal class NinePatchSprite : Sprite
{
    private SDL_Rect _topLeft;
    private SDL_Rect _top;
    private SDL_Rect _topRight;
    private SDL_Rect _right;
    private SDL_Rect _bottomRight;
    private SDL_Rect _bottom;
    private SDL_Rect _bottomLeft;
    private SDL_Rect _left;
    private SDL_Rect _center;

    private readonly SpritesheetFrame _frame;

    public NinePatchSprite(IntPtr texture, SpritesheetFrame frame) : base(texture,
        frame.OriginalSize)
    {
        _frame = frame;

        var split = frame.Split!.Value;
        var size = frame.OriginalSize;

        _topLeft = SdlHelper.Create(frame.Position.X, frame.Position.Y, split.Left, split.Top);
        _top = SdlHelper.Create(frame.Position.X + split.Left, frame.Position.Y, size.Width - (split.Right + split.Left), split.Top);
        _topRight = SdlHelper.Create(frame.Position.X + size.Width - split.Right, frame.Position.Y, split.Right, split.Top);
        _right = SdlHelper.Create(frame.Position.X + size.Width - split.Right, frame.Position.Y + split.Top, split.Right, size.Height - (split.Bottom + split.Top));
        _bottomRight = SdlHelper.Create(frame.Position.X + size.Width - split.Right, frame.Position.Y + size.Height - split.Bottom, split.Right, split.Bottom);
        _bottom = SdlHelper.Create(frame.Position.X + split.Left, frame.Position.Y + size.Height - split.Bottom, size.Width - (split.Right + split.Left), split.Bottom);
        _bottomLeft = SdlHelper.Create(frame.Position.X, frame.Position.Y + size.Height - split.Bottom, split.Left, split.Bottom);
        _left = SdlHelper.Create(frame.Position.X, frame.Position.Y + split.Top, split.Left, size.Height - (split.Bottom + split.Top));
        _center = SdlHelper.Create(frame.Position.X + split.Left, frame.Position.Y + split.Top, size.Width - (split.Right + split.Left), size.Height - (split.Bottom + split.Top));
    }

    internal override void Render(GraphicsManager context, Rectangle? src, RectangleF? dest, double angle, Color color, byte alpha, int index)
    {
        SDL_SetTextureColorMod(Texture, color.R, color.G, color.B);
        SDL_SetTextureAlphaMod(Texture, alpha);

        if (dest is null)
        {
            SDL_RenderGetLogicalSize(context.Renderer, out var w, out var h);
            if (w == 0 && h == 0)
            {
                SDL_RenderGetViewport(context.Renderer, out var viewport);
                dest = new RectangleF(viewport.x, viewport.y, viewport.w, viewport.h);
            }
            else
            {
                dest = new RectangleF(0, 0, w, h);
            }
        }

        var centerWidth = dest.Value.Width - (_frame.Split!.Value.Left + _frame.Split.Value.Right);
        var centerHeight = dest.Value.Height - (_frame.Split.Value.Top + _frame.Split.Value.Bottom);

        SDL_SetRenderDrawColor(context.Renderer, 255, 0, 0, 100);

        // Draw top left
        var target = new SDL_FRect
        {
            x = dest.Value.X,
            y = dest.Value.Y,
            w = _topLeft.w,
            h = _topLeft.h
        };
        SDL_RenderCopyExF(context.Renderer, Texture, ref _topLeft, ref target, 0, IntPtr.Zero,
            SDL_RendererFlip.SDL_FLIP_NONE);
        // SDL_RenderDrawRect(context.Renderer, ref target);

        // Draw top
        target = new SDL_FRect
        {
            x = dest.Value.X + _topLeft.w,
            y = dest.Value.Y,
            w = centerWidth,
            h = _top.h
        };
        SDL_RenderCopyExF(context.Renderer, Texture, ref _top, ref target, 0, IntPtr.Zero,
            SDL_RendererFlip.SDL_FLIP_NONE);
        // SDL_RenderDrawRect(context.Renderer, ref target);

        // Draw top right
        target = new SDL_FRect
        {
            x = dest.Value.X + _topLeft.w + centerWidth,
            y = dest.Value.Y,
            w = _topRight.w,
            h = _topRight.h
        };
        SDL_RenderCopyExF(context.Renderer, Texture, ref _topRight, ref target, 0, IntPtr.Zero,
            SDL_RendererFlip.SDL_FLIP_NONE);
        // SDL_RenderDrawRect(context.Renderer, ref target);

        // Draw left
        target = new SDL_FRect
        {
            x = dest.Value.X,
            y = dest.Value.Y + _topLeft.h,
            w = _left.w,
            h = centerHeight
        };
        SDL_RenderCopyExF(context.Renderer, Texture, ref _left, ref target, 0, IntPtr.Zero,
            SDL_RendererFlip.SDL_FLIP_NONE);
        // SDL_RenderDrawRect(context.Renderer, ref target);
        
        // // Draw center
        target = new SDL_FRect
        {
            x = dest.Value.X + _topLeft.w,
            y = dest.Value.Y + _topLeft.h,
            w = centerWidth,
            h = centerHeight
        };
        SDL_RenderCopyExF(context.Renderer, Texture, ref _center, ref target, 0, IntPtr.Zero,
            SDL_RendererFlip.SDL_FLIP_NONE);
        // SDL_RenderDrawRect(context.Renderer, ref target);
        
        // Draw right
        target = new SDL_FRect
        {
            x = dest.Value.X + _left.w + centerWidth,
            y = dest.Value.Y + _topLeft.h,
            w = _right.w,
            h = centerHeight
        };
        SDL_RenderCopyExF(context.Renderer, Texture, ref _right, ref target, 0, IntPtr.Zero,
            SDL_RendererFlip.SDL_FLIP_NONE);
        // SDL_RenderDrawRect(context.Renderer, ref target);
        
        // Draw bottom left
        target = new SDL_FRect
        {
            x = dest.Value.X,
            y = dest.Value.Y + _topLeft.h + centerHeight,
            w = _bottomLeft.w,
            h = _bottomLeft.h
        };
        SDL_RenderCopyExF(context.Renderer, Texture, ref _bottomLeft, ref target, 0, IntPtr.Zero,
            SDL_RendererFlip.SDL_FLIP_NONE);
        // SDL_RenderDrawRectF(context.Renderer, ref target);
        
        // Draw bottom
        target = new SDL_FRect
        {
            x = dest.Value.X + _bottomLeft.w,
            y = dest.Value.Y + _topLeft.h + centerHeight,
            w = centerWidth,
            h = _bottom.h
        };
        SDL_RenderCopyExF(context.Renderer, Texture, ref _bottom, ref target, 0, IntPtr.Zero,
            SDL_RendererFlip.SDL_FLIP_NONE);
        // SDL_RenderDrawRectF(context.Renderer, ref target);
        
        // Draw bottom right
        target = new SDL_FRect
        {
            x = dest.Value.X + _bottomLeft.w + centerWidth,
            y = dest.Value.Y + _topLeft.h + centerHeight,
            w = _bottomRight.w,
            h = _bottomRight.h
        };
        SDL_RenderCopyExF(context.Renderer, Texture, ref _bottomRight, ref target, 0, IntPtr.Zero,
            SDL_RendererFlip.SDL_FLIP_NONE);
        // SDL_RenderDrawRectF(context.Renderer, ref target);
    }
}
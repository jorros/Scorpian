using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Scorpian.Graphics;
using Scorpian.Helper;
using static Scorpian.SDL.SDL;

namespace Scorpian.Asset;

public class TextureSprite : Sprite
{
    private readonly SpritesheetFrame _frame;

    internal TextureSprite(IntPtr texture, Size size) : base(texture, size)
    {
    }

    internal TextureSprite(IntPtr texture, SpritesheetFrame frame) : base(texture, frame.OriginalSize)
    {
        _frame = frame;
    }

    internal override void Render(GraphicsManager context, Rectangle? src, RectangleF? dest, double angle, Color color, byte alpha, int index)
    {
        var target = IntPtr.Zero;
        SDL_FRect? targetRect = null;

        if (dest is not null)
        {
            targetRect = new SDL_FRect
            {
                x = dest.Value.X,
                y = dest.Value.Y,
                h = dest.Value.Height,
                w = dest.Value.Width
            };
        }

        var source = IntPtr.Zero;

        if (_frame is not null)
        {
            var srcRect = new SDL_Rect
            {
                w = _frame.Size.Width,
                h = _frame.Size.Height,
                x = _frame.Position.X,
                y = _frame.Position.Y
            };

            if (src is not null)
            {
                var maxW = srcRect.x + srcRect.w;
                var maxH = srcRect.y + srcRect.h;
                var actW = srcRect.x + src.Value.X + src.Value.Width;
                var actH = srcRect.y + src.Value.Y + src.Value.Height;

                srcRect = new SDL_Rect
                {
                    x = srcRect.x + src.Value.X,
                    y = srcRect.y + src.Value.Y,
                    w = actW > maxW ? maxW - (srcRect.x + src.Value.X) : src.Value.Width,
                    h = actH > maxH ? maxH - (srcRect.y + src.Value.Y) : src.Value.Height
                };
            }

            var offX = _frame.Offset.X;
            var offY = _frame.Offset.Y;

            var w = _frame.Size.Width;
            var h = _frame.Size.Height;

            var otherOffX = Size.Width - offX - w;
            var otherOffY = Size.Height - offY - h;

            if (targetRect is not null)
            {
                targetRect = new SDL_FRect
                {
                    x = targetRect.Value.x + offX,
                    y = targetRect.Value.y + otherOffY,
                    w = (int) (dest.Value.Width / (double) Size.Width * w),
                    h = (int) (dest.Value.Height / (double) Size.Height * h)
                };
            }

            // Center = new OffsetVector(w / 2, h / 2);

            source = Marshal.AllocHGlobal(Marshal.SizeOf(srcRect));
            Marshal.StructureToPtr(srcRect, source, false);
        }
        else if(src is not null)
        {
            var srcRect = src.Value.ToSdl();
            
            source = Marshal.AllocHGlobal(Marshal.SizeOf(srcRect));
            Marshal.StructureToPtr(srcRect, source, false);
        }

        // var debugRect = dest.ToSdl();
        // SDL_SetRenderDrawColor(context.Renderer, 255, 0, 0, 255);
        // SDL_RenderDrawRect(context.Renderer, ref debugRect);
        //
        // SDL_RenderDrawRect(context.Renderer, ref target);

        // var centerSdl = Center.ToSdl();

        if (targetRect is not null)
        {
            target = Marshal.AllocHGlobal(Marshal.SizeOf(targetRect));
            Marshal.StructureToPtr(targetRect.Value, target, false);
        }

        SDL_SetTextureColorMod(Texture, color.R, color.G, color.B);
        SDL_SetTextureAlphaMod(Texture, alpha);

        SDL_RenderCopyExF(context.Renderer, Texture, source, target, angle, IntPtr.Zero, 
            SDL_RendererFlip.SDL_FLIP_NONE);

        if (source != IntPtr.Zero)
        {
            Marshal.DestroyStructure<SDL_Rect>(source);
            Marshal.FreeHGlobal(source);
        }

        if (target != IntPtr.Zero)
        {
            Marshal.DestroyStructure<SDL_FRect>(target);
            Marshal.FreeHGlobal(target);
        }
    }
}
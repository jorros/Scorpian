using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Scorpian.Graphics;
using static Scorpian.SDL.SDL;

namespace Scorpian.Asset;

public class AnimatedSprite : Sprite
{
    private Dictionary<int, SpritesheetFrame> _frames;

    internal AnimatedSprite(IntPtr texture, SpritesheetFrame frame) : base(texture, frame.OriginalSize)
    {
        _frames = new Dictionary<int, SpritesheetFrame>();
    }

    internal void AddFrame(SpritesheetFrame frame)
    {
        _frames.Add(frame.Index, frame);
    }

    public int FramesCount => _frames.Count;

    internal override void Render(GraphicsManager context, Rectangle? src, RectangleF? dest, double angle, Color color, byte alpha, int index)
    {
        var srcRect = new SDL_Rect
        {
            x = _frames[index].Position.X,
            y = _frames[index].Position.Y,
            w = _frames[index].Size.Width,
            h = _frames[index].Size.Height
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
        
        var offX = _frames[index].Offset.X;
        var offY = _frames[index].Offset.Y;

        var w = _frames[index].Size.Width;
        var h = _frames[index].Size.Height;
        
        var otherOffY = Size.Height - offY - h;

        var target = IntPtr.Zero;

        if (dest is not null)
        {
            var targetRect = new SDL_FRect
            {
                x = dest.Value.X + offX,
                y = dest.Value.Y + otherOffY,
                w = (int) (dest.Value.Width / (double) Size.Width * w),
                h = (int) (dest.Value.Height / (double) Size.Height * h)
            };
            
            target = Marshal.AllocHGlobal(Marshal.SizeOf(targetRect));
            Marshal.StructureToPtr(targetRect, target, false);
        }

        SDL_SetTextureColorMod(Texture, color.R, color.G, color.B);
        SDL_SetTextureAlphaMod(Texture, alpha);

        SDL_RenderCopyExF(context.Renderer, Texture, ref srcRect, target, angle, IntPtr.Zero, 
            SDL_RendererFlip.SDL_FLIP_NONE);
        
        if (target != IntPtr.Zero)
        {
            Marshal.DestroyStructure<SDL_FRect>(target);
            Marshal.FreeHGlobal(target);
        }
    }
}
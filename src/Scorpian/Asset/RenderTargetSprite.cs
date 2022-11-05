using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Scorpian.Graphics;
using static Scorpian.SDL.SDL;

namespace Scorpian.Asset;

public class RenderTargetSprite : Sprite
{
    private readonly GraphicsManager _graphicsManager;

    internal override void Render(GraphicsManager context, Rectangle? src, RectangleF? dest, double angle, Color color, byte alpha, int index)
    {
        var target = IntPtr.Zero;

        if (dest is not null)
        {
            var targetRect = new SDL_FRect
            {
                x = dest.Value.X,
                y = dest.Value.Y,
                h = dest.Value.Height,
                w = dest.Value.Width
            };
            
            target = Marshal.AllocHGlobal(Marshal.SizeOf(targetRect));
            Marshal.StructureToPtr(targetRect, target, false);
        }

        SDL_SetTextureColorMod(Texture, color.R, color.G, color.B);
        SDL_SetTextureAlphaMod(Texture, alpha);

        SDL_RenderCopyExF(context.Renderer, Texture, IntPtr.Zero, target, angle, IntPtr.Zero, 
            SDL_RendererFlip.SDL_FLIP_NONE);

        if (target != IntPtr.Zero)
        {
            Marshal.DestroyStructure<SDL_FRect>(target);
            Marshal.FreeHGlobal(target);
        }
    }

    public void BeginDraw()
    {
        SDL_SetRenderTarget(_graphicsManager.Renderer, Texture);
    }

    public void EndDraw()
    {
        SDL_SetRenderTarget(_graphicsManager.Renderer, IntPtr.Zero);
    }

    public void Clear()
    {
        SDL_SetTextureBlendMode(Texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
        
        SDL_SetRenderDrawColor(_graphicsManager.Renderer, 0, 0, 0, 0);
        SDL_RenderClear(_graphicsManager.Renderer);
    }
    
    internal RenderTargetSprite(GraphicsManager graphicsManager, IntPtr texture, Size size) : base(texture, size)
    {
        _graphicsManager = graphicsManager;
    }
}
using System;
using System.Drawing;
using Scorpian.Graphics;
using static Scorpian.SDL.SDL;

namespace Scorpian.Asset;

public abstract class Sprite : IAsset, IDisposable
{
    public Size Size { get; }

    internal IntPtr Texture { get; }

    protected Sprite(IntPtr texture, Size size)
    {
        Texture = texture;
        Size = size;
    }
    
    internal abstract void Render(GraphicsManager context, Rectangle? src, RectangleF? dest, double angle, Color color, byte alpha, int index);
    
    public void Dispose()
    {
        SDL_DestroyTexture(Texture);
    }
}
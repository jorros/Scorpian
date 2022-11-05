using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Scorpian.Asset.Markup;

namespace Scorpian.Asset.Font;

public class NewLineRenderer : IFontRenderer
{
    public Type Type => typeof(NewLineBlock);
    public IEnumerable<(IntPtr texture, SDL.SDL.SDL_Rect target)> Render(Font font, IBlock block, Point position, ref Point cursor)
    {
        cursor.X = position.X;
        cursor.Y += ((NewLineBlock)block).LineHeight;

        return Enumerable.Empty<(IntPtr texture, SDL.SDL.SDL_Rect target)>();
    }

    public void CalculateSize(string text, Font font, IBlock block, ref Point cursor)
    {
        cursor.X = 0;
        cursor.Y += ((NewLineBlock)block).LineHeight;
    }

    public void Clear()
    {
        
    }
}
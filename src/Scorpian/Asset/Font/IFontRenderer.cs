using System;
using System.Collections.Generic;
using System.Drawing;
using Scorpian.Asset.Markup;
using static Scorpian.SDL.SDL;

namespace Scorpian.Asset.Font;

public interface IFontRenderer
{
    Type Type { get; }

    IEnumerable<(IntPtr texture, SDL_Rect target)> Render(Font font, IBlock block, Point position, ref Point cursor);

    void CalculateSize(string text, Font font, IBlock block, ref Point cursor);

    void Clear();
}
using System;
using System.Drawing;
using Scorpian.Graphics;
using Scorpian.UI.Style;

namespace Scorpian.UI;

public class HorizontalDivider : UIElement
{
    public string Type { get; set; }

    public new int Width
    {
        get => base.Width;
        set => base.Width = value;
    }

    protected override void OnInit(RenderContext renderContext, Stylesheet stylesheet)
    {
        var style = stylesheet.GetHorizontalDivider(Type);

        if (Height == 0)
        {
            Height = style.Height;
        }
    }

    protected override void OnRender(RenderContext renderContext, Stylesheet stylesheet, bool inWorld)
    {
        var style = stylesheet.GetHorizontalDivider(Type);
        var position = GetPosition();

        if (!Show)
        {
            return;
        }

        var width = Math.Max(Width, style.MinWidth);
        var target = new RectangleF(stylesheet.Scale(position.X), stylesheet.Scale(position.Y), stylesheet.Scale(width),
            stylesheet.Scale(Height));
        
        renderContext.Draw(style.Background, target, 0, Color.White, 255, -1, inWorld);
    }
}
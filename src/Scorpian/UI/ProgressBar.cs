using System;
using System.Drawing;
using Scorpian.Graphics;
using Scorpian.UI.Style;

namespace Scorpian.UI;

public class ProgressBar : UIElement
{
    public string Type { get; set; }

    public byte Progress { get; set; }

    public void SetWidth(int width)
    {
        Width = width;
    }

    protected override void OnInit(RenderContext renderContext, Stylesheet stylesheet)
    {
        var style = stylesheet.GetProgressBar(Type);

        if (Height == 0)
        {
            Height = style.Height;
        }

        if (Width == 0)
        {
            Width = style.Width;
        }
    }

    protected override void OnRender(RenderContext renderContext, Stylesheet stylesheet, bool inWorld)
    {
        var style = stylesheet.GetProgressBar(Type);

        if (!Show)
        {
            return;
        }

        var width = stylesheet.Scale(Width);
        var height = stylesheet.Scale(Height);

        var position = stylesheet.Scale(GetPosition());

        var target = new RectangleF(position.X, position.Y, width, height);
        if (style.Background is not null)
        {
            renderContext.Draw(style.Background, target, 0, Color.White, 255, -1, inWorld);
        }

        Rectangle src;

        if (style.Orientation == Orientation.Horizontal)
        {
            target = new RectangleF(position.X, position.Y, (int) Math.Round(width / 100.0 * Progress), height);
            src = new Rectangle(0, 0, (int) target.Width, (int) target.Height);
        }
        else
        {
            var calc = (int) Math.Round(height / 100.0 * Progress);
            target = new RectangleF(position.X, position.Y + height - calc, width, calc);
            src = new Rectangle(0, height - calc, width, calc);
        }

        renderContext.Draw(style.Fill, src, target, 0, style.FillColor, 255, -1, inWorld);
        
        target = new RectangleF(position.X, position.Y, width, height);
        if (style.Foreground is not null)
        {
            renderContext.Draw(style.Foreground, target, 0, Color.White, 255, -1, inWorld);
        }
    }
}
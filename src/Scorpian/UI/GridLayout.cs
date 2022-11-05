using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Scorpian.Asset;
using Scorpian.Graphics;
using Scorpian.Maths;
using Scorpian.UI.Style;

namespace Scorpian.UI;

public class GridLayout : UIElement, Container
{
    public List<UIElement> Elements { get; } = new();
    public Sprite Background { get; set; }
    public Point Padding { get; set; }
    public Point Margin { get; set; } = Point.Empty;
    public Size GridSize { get; set; }
    
    public void Attach(UIElement element)
    {
        element.Parent = this;
        Elements.Add(element);
    }

    public void Clear()
    {
        Elements.Clear();
    }

    public void Remove(UIElement element)
    {
        Elements.Remove(element);
    }

    public void Remove(Func<UIElement, bool> predicate)
    {
        var element = Elements.FirstOrDefault(predicate);
        if (element is not null)
        {
            Elements.Remove(element);
        }
    }

    private PointF GetPoint(float x, float y)
    {
        var cellWidth = (float)Width / GridSize.Width;
        var cellHeight = (float)Height / GridSize.Height;

        return new PointF(x * cellWidth, y * cellHeight);
    }

    private PointF GetPoint(int pos)
    {
        var y = pos / (float)GridSize.Width;
        var x = pos % GridSize.Width;

        return GetPoint(x, y);
    }

    protected override void OnInit(RenderContext renderContext, Stylesheet stylesheet)
    {
    }

    protected override void OnRender(RenderContext renderContext, Stylesheet stylesheet, bool inWorld)
    {
        if (!Show)
        {
            return;
        }
        
        if (Background is not null)
        {
            var scaledPos = stylesheet.Scale(GetPosition()).Add(stylesheet.Scale(Margin));
            var rect = new RectangleF(scaledPos.X, scaledPos.Y, stylesheet.Scale(Width), stylesheet.Scale(Height));
            renderContext.Draw(Background, rect, 0, Color.White, 255, -1, inWorld);
        }
        
        var currentPos = new PointF(Padding.X, Padding.Y).Add(Margin);
        
        for (var i = 0; i < Elements.Count; i++)
        {
            Elements[i].Position = currentPos.Add(GetPoint(i));
            Elements[i].Render(renderContext, stylesheet, inWorld);
        }
    }
}
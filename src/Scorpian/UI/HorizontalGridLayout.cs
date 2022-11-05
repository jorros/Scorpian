using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Scorpian.Asset;
using Scorpian.Graphics;
using Scorpian.Maths;
using Scorpian.UI.Style;

namespace Scorpian.UI;

public class HorizontalGridLayout : UIElement, Container
{
    public List<UIElement> Elements { get; } = new();
    public Sprite Background { get; set; }
    
    public int MinWidth { get; set; }
    public Box Padding { get; set; }
    public int SpaceBetween { get; set; }
    public Point Margin { get; set; } = Point.Empty;

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

    protected override void OnInit(RenderContext renderContext, Stylesheet stylesheet)
    {
    }

    protected override void OnRender(RenderContext renderContext, Stylesheet stylesheet, bool inWorld)
    {
        if (!Show)
        {
            return;
        }
        
        var scaledHeight = stylesheet.Scale(Height);
        
        var span = CollectionsMarshal.AsSpan(Elements);
        
        var elementSum = 0;
        foreach (var element in span)
        {
            elementSum += element.Width;
        }

        Width = Padding.Left + Padding.Right + elementSum + Math.Max(Elements.Count - 1, 0) * SpaceBetween;
        Width = Width > MinWidth ? Width : MinWidth;
        
        if (Background is not null)
        {
            var scaledPos = stylesheet.Scale(GetPosition()).Add(stylesheet.Scale(Margin));
            var rect = new RectangleF(scaledPos.X, scaledPos.Y, stylesheet.Scale(Width), scaledHeight);
            renderContext.Draw(Background, rect, 0, Color.White, 255, -1, inWorld);
        }
        
        var currentPos = new PointF(Padding.Left, Padding.Top).Add(Margin);

        foreach (var element in span)
        {
            element.Position = currentPos;

            element.Render(renderContext, stylesheet, inWorld);

            currentPos = currentPos.Add(new PointF(SpaceBetween + element.Width, 0));
        }
    }
}
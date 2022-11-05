using System.Drawing;
using Scorpian.Asset;
using Scorpian.Graphics;
using Scorpian.UI.Style;

namespace Scorpian.UI;

public class Image : UIElement
{
    private Sprite _sprite;
    private int? _width;
    private int? _height;

    public new int Width
    {
        get => base.Width;
        set
        {
            _width = value;
            base.Width = value;
        }
    }

    public new int Height
    {
        get => base.Height;
        set
        {
            _height = value;
            base.Height = value;
        }
    }
    
    public Color Color { get; set; } = Color.White;

    public Sprite Sprite
    {
        get => _sprite;
        set
        {
            _sprite = value;
            if (_width is null)
            {
                base.Width = value.Size.Width;
            }

            if (_height is null)
            {
                base.Height = value.Size.Height;
            }
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
        
        var position = stylesheet.Scale(GetPosition());
        var bounds = new RectangleF(position.X, position.Y, stylesheet.Scale(Width), stylesheet.Scale(Height));

        renderContext.Draw(Sprite, bounds, 0, Color, 255, -1, inWorld);
    }
}
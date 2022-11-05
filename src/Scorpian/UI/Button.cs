using System;
using System.Drawing;
using Scorpian.Graphics;
using Scorpian.InputManagement;
using Scorpian.Maths;
using Scorpian.UI.Style;

namespace Scorpian.UI;

public class Button : UIElement
{
    public string Type { get; set; }
    public Content Content { get; set; }

    public delegate void ClickedEventHandler(object sender, MouseButtonEventArgs e);

    public event ClickedEventHandler OnClick;

    private RectangleF? _bounds;
    private bool _isPressed;

    public Button()
    {
        Input.OnMouseButton += (_, args) =>
        {
            if (!Show || !Enabled)
            {
                return;
            }

            if (args.Type == MouseEventType.ButtonUp)
            {
                _isPressed = false;
            }

            if (_bounds is null)
            {
                return;
            }

            if (_bounds.Value.Contains(new Point(args.X, args.Y)))
            {
                switch (args.Type)
                {
                    case MouseEventType.ButtonDown:
                        _isPressed = true;
                        break;
                    case MouseEventType.ButtonUp:
                        OnClick?.Invoke(this, args);
                        break;
                }

                return;
            }

            _isPressed = false;
        };
    }

    private bool IsInButton(PointF position)
    {
        if (_bounds is null)
        {
            return false;
        }

        return position.X >= _bounds.Value.X && position.X <= _bounds.Value.X + _bounds.Value.Width &&
               position.Y >= _bounds.Value.Y && position.Y <= _bounds.Value.Y + _bounds.Value.Height;
    }

    protected override void OnInit(RenderContext renderContext, Stylesheet stylesheet)
    {
    }

    protected override void OnRender(RenderContext renderContext, Stylesheet stylesheet, bool inWorld)
    {
        var style = stylesheet.GetButton(Type);

        var content = Content.Value;
        var paddedTextWidth = style.Padding.X * 2 + content.Width;
        var paddedTextHeight = style.Padding.Y * 2 + content.Height;

        Width = style.MinWidth is not null ? Math.Max(style.MinWidth.Value, paddedTextWidth) : style.FixedWidth;
        Height = style.MinHeight is not null ? Math.Max(style.MinHeight.Value, paddedTextHeight) : style.FixedHeight;

        var position = GetPosition();

        _bounds = new RectangleF(stylesheet.Scale(position.X), stylesheet.Scale(position.Y), stylesheet.Scale(Width),
            stylesheet.Scale(Height));
        
        if (!Show)
        {
            return;
        }

        var tint = style.Tint ?? Color.White;

        if (style.HoveredTint is not null && Enabled && IsInButton(Input.MousePosition))
        {
            tint = style.HoveredTint.Value;
        }

        if (style.PressedTint is not null && Enabled && _isPressed)
        {
            tint = style.PressedTint.Value;
        }

        if (!Enabled)
        {
            tint = Color.DarkGray;
        }

        if (style.Button is not null)
        {
            renderContext.Draw(style.Button, _bounds.Value, 0, tint, 255, -1, inWorld);
        }

        switch (Content.Value)
        {
            case Label {Type:null} label when style.DefaultLabelStyle is not null:
                label.Type = style.DefaultLabelStyle;
                break;
            case Image img:
                img.Color = tint;
                break;
        }

        content.Position = new PointF(Width / 2f, Height / 2f).Add(position).Add(style.ContentPosition);
        content.Render(renderContext, stylesheet, inWorld);
    }
}
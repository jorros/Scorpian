using System;
using System.Drawing;
using Scorpian.Graphics;
using Scorpian.InputManagement;
using Scorpian.UI.Style;

namespace Scorpian.UI;

public class RadioButton : UIElement
{
    public string Type { get; set; }

    public object Value { get; set; }
    
    public RadioGroup RadioGroup { get; set; } 

    public Content Content
    {
        get => _content;
        set
        {
            if (value is not null)
            {
                value.Value.Parent = this;
            }

            _content = value;
        }
    }

    public bool IsSelected { get; set; }

    private RectangleF? _bounds;
    private bool _isPressed;
    private Content _content;

    public RadioButton()
    {
        Input.OnMouseButton += InputOnOnMouseButton;
    }

    private void InputOnOnMouseButton(object sender, MouseButtonEventArgs e)
    {
        if (!Show || !Enabled)
        {
            return;
        }

        if (e.Type == MouseEventType.ButtonUp)
        {
            _isPressed = false;
        }

        if (_bounds is null || RadioGroup is null)
        {
            return;
        }

        if (_bounds.Value.Contains(new Point(e.X, e.Y)))
        {
            switch (e.Type)
            {
                case MouseEventType.ButtonDown:
                    _isPressed = true;
                    break;
                case MouseEventType.ButtonUp:
                    RadioGroup.Select(this);
                    break;
            }

            return;
        }

        _isPressed = false;
    }

    protected override void OnInit(RenderContext renderContext, Stylesheet stylesheet)
    {
    }

    protected override void OnRender(RenderContext renderContext, Stylesheet stylesheet, bool inWorld)
    {
        var style = stylesheet.GetRadioButton(Type);

        if (Content is not null)
        {
            Width = Math.Max(style.MinWidth, Content.Value.Width + style.Padding.Left + style.Padding.Right);
            Height = Math.Max(style.MinHeight, Content.Value.Height + style.Padding.Top + style.Padding.Bottom);
        }
        else
        {
            Width = style.MinWidth;
            Height = style.MinHeight;
        }

        var position = GetPosition();
        _bounds = new RectangleF(stylesheet.Scale(position.X), stylesheet.Scale(position.Y), stylesheet.Scale(Width),
            stylesheet.Scale(Height));

        if (!Show)
        {
            return;
        }

        var tint = Color.White;

        if (style.HoveredTint is not null && Enabled && _bounds.Value.Contains(Input.MousePosition))
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

        var sprite = style.UncheckedButton;
        if (IsSelected)
        {
            sprite = style.CheckedButton;

            if (style.SelectedTint is not null)
            {
                tint = style.SelectedTint.Value;
            }
        }

        if (sprite is not null)
        {
            renderContext.Draw(sprite, _bounds.Value, 0, tint, 255, -1, inWorld);
        }

        if (Content is not null)
        {
            if (Content.Value is Image img)
            {
                img.Color = tint;
            }
            
            Content.Value.Position = new PointF(style.Padding.Left, style.Padding.Top);
            Content.Value.Render(renderContext, stylesheet, inWorld);
        }
    }
}
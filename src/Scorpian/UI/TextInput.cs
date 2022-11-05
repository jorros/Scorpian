using System.Drawing;
using Scorpian.Graphics;
using Scorpian.Helper;
using Scorpian.InputManagement;
using Scorpian.Maths;
using Scorpian.UI.Style;

namespace Scorpian.UI;

public class TextInput : UIElement
{
    public string Type { get; set; }

    public string Text { get; set; } = string.Empty;

    private bool _focus;
    private RectangleF _bounds;
    private int _caret;

    private DelayedAction _backspaceAction;
    private DelayedAction _deleteAction;
    private DelayedAction _leftCursor;
    private DelayedAction _rightCursor;

    private const int InputStartScrolling = 30;
    private const int KeyboardInitialDelay = 300;

    public TextInput()
    {
        Input.OnKeyboard += InputOnOnKeyboard;
        Input.OnMouseButton += InputOnOnMouseButton;
        Input.OnTextInput += InputOnOnTextInput;
    }

    private void InputOnOnMouseButton(object sender, MouseButtonEventArgs e)
    {
        if (Show == false)
        {
            return;
        }
        
        if (Enabled == false)
        {
            return;
        }
        
        if (e.Button != MouseButton.Left)
        {
            _focus = false;
            return;
        }

        var point = new Point(e.X, e.Y);
        if (_bounds.Contains(point))
        {
            _focus = true;
            _caret = Text.Length;
            return;
        }

        _focus = false;
    }

    private void InputOnOnTextInput(object sender, TextInputEventArgs e)
    {
        if (!_focus)
        {
            return;
        }

        Text = Text.Insert(_caret, e.Character.ToString());
        _caret++;
    }

    private void LeftCursor()
    {
        _caret = _caret > 0 ? _caret - 1 : _caret;
    }

    private void RightCursor()
    {
        _caret = _caret < Text.Length ? _caret + 1 : _caret;
    }

    private void Backspace()
    {
        if (_caret == 0)
        {
            return;
        }

        Text = Text.Remove(_caret - 1, 1);
        _caret = _caret > 0 ? _caret - 1 : _caret;
    }

    private void Delete()
    {
        if (_caret < Text.Length)
        {
            Text = Text.Remove(_caret, 1);
        }
    }

    private void InputOnOnKeyboard(object sender, KeyboardEventArgs e)
    {
        if (!Show || !Enabled)
        {
            return;
        }
        
        if (!_focus)
        {
            return;
        }

        switch (e.Key)
        {
            case KeyboardKey.Left:
                if (e.Type == KeyboardEventType.KeyDown)
                {
                    _leftCursor.TryInvoke(KeyboardInitialDelay);
                    break;
                }

                _leftCursor.InvokeIfNotInvoked();
                _leftCursor.Reset();

                break;

            case KeyboardKey.Right:
                if (e.Type == KeyboardEventType.KeyDown)
                {
                    _rightCursor.TryInvoke(KeyboardInitialDelay);
                    break;
                }

                _rightCursor.InvokeIfNotInvoked();
                _rightCursor.Reset();
                break;

            case KeyboardKey.Backspace:
                if (e.Type == KeyboardEventType.KeyDown)
                {
                    _backspaceAction.TryInvoke(KeyboardInitialDelay);
                    break;
                }

                _backspaceAction.InvokeIfNotInvoked();
                _backspaceAction.Reset();
                break;

            case KeyboardKey.Delete:
                if (e.Type == KeyboardEventType.KeyDown)
                {
                    _deleteAction.TryInvoke(KeyboardInitialDelay);
                    break;
                }

                _deleteAction.InvokeIfNotInvoked();
                _deleteAction.Reset();
                break;
        }
    }

    public void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    protected override void OnInit(RenderContext renderContext, Stylesheet stylesheet)
    {
        _backspaceAction ??= new DelayedAction(renderContext, Backspace);
        _deleteAction ??= new DelayedAction(renderContext, Delete);
        _leftCursor ??= new DelayedAction(renderContext, LeftCursor);
        _rightCursor ??= new DelayedAction(renderContext, RightCursor);
        
        var style = stylesheet.GetTextInput(Type);
        
        if (Width == 0)
        {
            Width = style.Width;
        }

        if (Height == 0)
        {
            Height = style.Height;
        }
    }

    protected override void OnRender(RenderContext renderContext, Stylesheet stylesheet, bool inWorld)
    {
        var style = stylesheet.GetTextInput(Type);

        if (!Show)
        {
            return;
        }

        var position = stylesheet.Scale(GetPosition());
        _bounds = new RectangleF(position.X, position.Y, stylesheet.Scale(Width), stylesheet.Scale(Height));

        var tint = Color.White;

        if (!Enabled)
        {
            tint = Color.DarkGray;
        }

        renderContext.Draw(style.Background, _bounds, 0, tint, 255, -1, inWorld);

        var padding = stylesheet.Scale(style.Padding);
        var textSettings = style.Text.ToFontSettings();
        textSettings = textSettings with {Size = stylesheet.Scale(textSettings.Size)};

        var clippingRect = new RectangleF(position.X + padding.X, position.Y + padding.Y,
            _bounds.Width - padding.X * 2, _bounds.Height - padding.Y * 2);

        renderContext.SetClipping(clippingRect);

        var startPos = position.Add(padding);
        var textWidth = style.Text.Font.CalculateSize(Text[.._caret], textSettings).Width;
        var textHeight = style.Text.Font.GetHeight(textSettings);
        var textCorrection = 0f;

        if (startPos.X + textWidth > (clippingRect.Right - stylesheet.Scale(InputStartScrolling)))
        {
            textCorrection = clippingRect.Right - (startPos.X + textWidth) - stylesheet.Scale(InputStartScrolling);
        }

        renderContext.DrawText(style.Text.Font,
            new PointF(position.X + padding.X + textCorrection, position.Y + padding.Y), Text, textSettings,
            inWorld);
        
        renderContext.SetClipping(null);
        
        if (_focus && Enabled)
        {
            renderContext.DrawLine(new PointF(startPos.X + textWidth + textCorrection, startPos.Y),
                new PointF(startPos.X + textWidth + textCorrection, startPos.Y + textHeight), style.Text.Color);
        }
    }
}
using System.Drawing;
using System.Text;
using Scorpian.Asset;
using Scorpian.Asset.Font;
using Scorpian.Graphics;
using Scorpian.Maths;
using Scorpian.UI.Style;

namespace Scorpian.UI;

public class Label : UIElement
{
    private Font _font;
    public string Type { get; set; }
    public string Font { get; set; }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;

            if (_calculatedText is not null)
            {
                UpdateText();
            }
        }
    }

    public int? Size { get; set; }
    public Color? Color { get; set; }
    public Alignment? TextAlign { get; set; }
    public FontStyle? Style { get; set; }
    public int? Outline { get; set; }
    public Color? OutlineColor { get; set; }
    public Point Margin { get; set; }
    public int? MaxWidth { get; set; }
    private string _calculatedText;
    private LabelStyle _style;
    private FontSettings _fontSettings;
    private string _text = string.Empty;

    protected override void OnInit(RenderContext renderContext, Stylesheet stylesheet)
    {
        _style = stylesheet.GetLabel(Type);
        _font = Font is null ? _style.Font : stylesheet.GetFont(Font);
        
        _fontSettings = new FontSettings
        {
            Alignment = TextAlign ?? _style.Alignment,
            Color = Color ?? _style.Color,
            Size = stylesheet.Scale(Size ?? _style.Size),
            Style = Style ?? _style.Style,
            Outline = Outline ?? _style.Outline,
            OutlineColor = OutlineColor ?? _style.OutlineColor
        };

        UpdateText();
    }

    private void UpdateText()
    {
        var maxWidth = MaxWidth ?? _style.MaxWidth;
        if (maxWidth is not null)
        {
            var words = Text.Split(' ');
            var sb = new StringBuilder();
            var currentWidth = 0;

            foreach (var word in words)
            {
                var length = _font.CalculateSize($" {word}", _fontSettings).Width;
                if (currentWidth + length >= maxWidth)
                {
                    sb.Append('\n');
                    currentWidth = _font.CalculateSize(word, _fontSettings).Width;
                }
                else
                {
                    sb.Append(' ');
                    currentWidth += length;
                }

                sb.Append($"{word}");
            }

            sb = sb.Remove(0, 1);

            _calculatedText = sb.ToString();
            
            return;
        }

        _calculatedText = Text;
    }

    protected override void OnRender(RenderContext renderContext, Stylesheet stylesheet, bool inWorld)
    {
        var size = _font.CalculateSize(_calculatedText, _fontSettings);
        Width = size.Width;
        Height = size.Height;
        
        if (!Show)
        {
            return;
        }
        
        renderContext.DrawText(_font, stylesheet.Scale(GetPosition().Add(Margin)), _calculatedText, _fontSettings, inWorld);
    }
}
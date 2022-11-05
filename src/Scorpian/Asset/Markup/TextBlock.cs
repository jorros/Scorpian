using System.Drawing;

namespace Scorpian.Asset.Markup;

public record TextBlock : IBlock
{
    public int Size { get; set; }
    public int Outline { get; set; }
    public Color OutlineColor { get; set; }
    public FontStyle Style { get; set; }
    public Color Color { get; set; }
    public string Text { get; set; }
}
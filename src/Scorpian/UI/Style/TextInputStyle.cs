using System.Drawing;
using Scorpian.Asset;

namespace Scorpian.UI.Style;

public record TextInputStyle
{
    public LabelStyle Text { get; set; }
    
    public Sprite Background { get; set; }
    
    public Point Padding { get; set; }
    
    public int Width { get; set; }
    
    public int Height { get; set; }
}
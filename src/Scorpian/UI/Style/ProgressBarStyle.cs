using System.Drawing;
using Scorpian.Asset;

namespace Scorpian.UI.Style;

public class ProgressBarStyle
{
    public Sprite Background { get; set; }
    
    public Sprite Foreground { get; set; }
    
    public Sprite Fill { get; set; }
    
    public Color FillColor { get; set; } = Color.White;

    public Orientation Orientation { get; set; } = Orientation.Horizontal;
    
    public int Height { get; set; }
    
    public int Width { get; set; }
}
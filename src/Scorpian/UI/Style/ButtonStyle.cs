using System.Drawing;
using Scorpian.Asset;

namespace Scorpian.UI.Style;

public record ButtonStyle
{
    public Sprite Button { get; set; }

    public int? MinWidth { get; set; }
    
    public int? MinHeight { get; set; }
    
    public int FixedWidth { get; set; }
    public int FixedHeight { get; set; }
    
    public Point ContentPosition { get; set; } = Point.Empty;
    
    public string DefaultLabelStyle { get; set; }
    
    public Point Padding { get; set; } = Point.Empty;
    
    public Color? Tint { get; set; }
    
    public Color? PressedTint { get; set; }
    
    public Color? HoveredTint { get; set; }
}
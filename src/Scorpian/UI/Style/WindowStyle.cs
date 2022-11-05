using System.Drawing;
using Scorpian.Asset;
using Scorpian.Maths;

namespace Scorpian.UI.Style;

public record WindowStyle
{
    public int Width { get; set; }
    
    public int Height { get; set; }
    
    public Sprite Background { get; set; }

    public Point Padding { get; set; }
    
    public bool HasActionBar { get; set; }
    
    public Box ActionBarPadding { get; set; }
    
    public int ActionBarSpaceBetween { get; set; }
    
    public int ActionBarHeight { get; set; }
    
    public Alignment ActionBarAlign { get; set; }
    
    public bool HasTitle { get; set; }
    
    public Box TitlePadding { get; set; }
    
    public int TitleSpaceBetween { get; set; }
    
    public int TitleHeight { get; set; }
    
    public string TitleLabelStyle { get; set; }
    
    public Alignment TitleAlign { get; set; }
    
    public bool IsDraggable { get; set; }
}
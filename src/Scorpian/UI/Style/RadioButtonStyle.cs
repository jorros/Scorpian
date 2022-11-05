using System.Drawing;
using Scorpian.Asset;
using Scorpian.Maths;

namespace Scorpian.UI.Style;

public class RadioButtonStyle
{
    public Sprite? UncheckedButton { get; set; }
    
    public Sprite? CheckedButton { get; set; }
    
    public int MinWidth { get; set; }
    
    public int MinHeight { get; set; }
    
    public Box Padding { get; set; }
    
    public Color? SelectedTint { get; set; }
    
    public Color? PressedTint { get; set; }
    
    public Color? HoveredTint { get; set; }
}
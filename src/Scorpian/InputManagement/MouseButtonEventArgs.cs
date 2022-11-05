namespace Scorpian.InputManagement;

public class MouseButtonEventArgs
{
    public MouseEventType Type { get; set; }
    
    public int Clicks { get; set; }
    
    public int X { get; set; }
    
    public int Y { get; set; }
    
    public MouseButton Button { get; set; }
}

public enum MouseEventType
{
    ButtonDown,
    ButtonUp
}
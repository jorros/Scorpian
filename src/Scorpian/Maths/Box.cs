using System.Drawing;

namespace Scorpian.Maths;

public struct Box
{
    public int Left { get; set; }
    
    public int Right { get; set; }
    
    public int Top { get; set; }
    
    public int Bottom { get; set; }

    public Box(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public Box(Rectangle rectangle)
    {
        Left = rectangle.X;
        Top = rectangle.Y;
        Right = rectangle.Width;
        Bottom = rectangle.Height;
    }

    public static Box Empty { get; } = new Box(0, 0, 0, 0);
}
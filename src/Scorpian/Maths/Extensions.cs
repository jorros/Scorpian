using System.Drawing;
using System.Numerics;
using Scorpian.HexMap;

namespace Scorpian.Maths;

public static class Extensions
{
    public static Point Add(this Point a, Point b)
    {
        return new Point(a.X + b.X, a.Y + b.Y);
    }

    public static Point Subtract(this Point a, Point b)
    {
        return new Point(a.X - b.X, a.Y - b.Y);
    }
    
    public static PointF Add(this PointF a, PointF b)
    {
        return new PointF(a.X + b.X, a.Y + b.Y);
    }

    public static PointF Subtract(this PointF a, PointF b)
    {
        return new PointF(a.X - b.X, a.Y - b.Y);
    }

    public static Hex ToCube(this Point a)
    {
        var q = a.X - (a.Y - (a.X & 1)) / 2;
        var r = a.Y;
        var s = -q - r;

        return new Hex(q, r, s);
    }

    public static Point ToPoint(this Vector2 vector2)
    {
        var pt = new Point(
            (int) (vector2.X + 0.5f), (int) (vector2.Y + 0.5f));

        return pt;
    }

    public static PointF ToPointF(this Vector2 vector2)
    {
        var pt = new PointF(vector2.X, vector2.Y);

        return pt;
    }

    public static Vector2 ToVector(this Point point)
    {
        return new Vector2(point.X, point.Y);
    }

    public static Vector2 ToVector(this Size size)
    {
        return new Vector2(size.Width, size.Height);
    }

    public static SizeF ToSize(this Vector2 vector)
    {
        return new SizeF(vector.X, vector.Y);
    }
}
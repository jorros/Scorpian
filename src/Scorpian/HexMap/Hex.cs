using System;
using System.Drawing;

namespace Scorpian.HexMap;

public readonly struct Hex : IEquatable<Hex>
{
    public int Q { get; init; }
    public int R { get; init; }
    public int S { get; init; }

    public static Hex FromAngle(double angle) => angle switch
    {
        > 330 => new Hex(1, 0, -1),
        > 270 => new Hex(1, -1, 0),
        > 210 => new Hex(0, -1, 1),
        > 150 => new Hex(-1, 0, 1),
        > 90 => new Hex(-1, +1, 0),
        > 30 => new Hex(0, +1, -1),
        _ => new Hex(1, 0, -1)
    };

    public Hex(int q, int r, int s)
    {
        Q = q;
        R = r;
        S = s;
    }
    
    public double DistanceTo(Hex to)
    {
        var vector = to - this;

        return (Math.Abs(vector.Q) + Math.Abs(vector.R) + Math.Abs(vector.S)) / 2;
    }
    
    public static Hex operator +(Hex a) => a;
    public static Hex operator -(Hex a) => new(-a.Q, -a.R, -a.S);

    public static Hex operator +(Hex a, Hex b)
        => new(a.Q + b.Q, a.R + b.R, a.S + b.S);

    public static Hex operator -(Hex a, Hex b)
        => a + (-b);

    public static Hex operator *(Hex a, Hex b)
        => new Hex(a.Q * b.Q, a.R * b.R, a.S * b.S);

    public static Hex operator /(Hex a, Hex b)
    {
        if (b.Q == 0 || b.R == 0 || b.S == 0)
        {
            throw new DivideByZeroException();
        }

        return new Hex(a.Q / b.Q, a.R / b.R, a.S / b.S);
    }
    
    public static bool operator ==(Hex obj1, Hex obj2) => obj1.Equals(obj2);
    public static bool operator !=(Hex obj1, Hex obj2) => !(obj1 == obj2);

    public Point ToPoint()
    {
        return new Point(Q + (R - (R & 1)) / 2, R);
    }

    public bool Equals(Hex other)
    {
        return Q == other.Q && R == other.R && S == other.S;
    }

    public override bool Equals(object obj)
    {
        return obj is Hex other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Q, R, S);
    }

    public override string ToString()
    {
        return $"{Q}:{R}:{S}";
    }
}
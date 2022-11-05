using System;
using System.Drawing;

namespace Scorpian.HexMap;

public readonly struct HexF : IEquatable<HexF>
{
    public float Q { get; init; }
    public float R { get; init; }
    public float S { get; init; }

    public static HexF FromAngle(double angle) => angle switch
    {
        > 330 => new HexF(1, 0, -1),
        > 270 => new HexF(1, -1, 0),
        > 210 => new HexF(0, -1, 1),
        > 150 => new HexF(-1, 0, 1),
        > 90 => new HexF(-1, +1, 0),
        > 30 => new HexF(0, +1, -1),
        _ => new HexF(1, 0, -1)
    };

    public HexF(float q, float r, float s)
    {
        Q = q;
        R = r;
        S = s;
    }

    public double DistanceTo(HexF to)
    {
        var vector = to - this;

        return (Math.Abs(vector.Q) + Math.Abs(vector.R) + Math.Abs(vector.S)) / 2f;
    }

    public static HexF operator +(HexF a) => a;
    public static HexF operator -(HexF a) => new(-a.Q, -a.R, -a.S);

    public static HexF operator +(HexF a, HexF b)
        => new(a.Q + b.Q, a.R + b.R, a.S + b.S);

    public static HexF operator -(HexF a, HexF b)
        => a + (-b);

    public static HexF operator *(HexF a, HexF b)
        => new HexF(a.Q * b.Q, a.R * b.R, a.S * b.S);

    public static HexF operator /(HexF a, HexF b)
    {
        if (b.Q == 0f || b.R == 0f || b.S == 0f)
        {
            throw new DivideByZeroException();
        }

        return new HexF(a.Q / b.Q, a.R / b.R, a.S / b.S);
    }

    public static bool operator ==(HexF obj1, HexF obj2) => obj1.Equals(obj2);
    public static bool operator !=(HexF obj1, HexF obj2) => !(obj1 == obj2);

    public PointF ToPointF()
    {
        return new PointF(Q + (R - ((int) R & 1)) / 2f, R);
    }

    public Hex ToHex()
    {
        var q = (int) Math.Round(Q);
        var r = (int) Math.Round(R);
        var s = (int) Math.Round(S);
        var q_diff = Math.Abs(q - Q);
        var r_diff = Math.Abs(r - R);
        var s_diff = Math.Abs(s - S);
        if (q_diff > r_diff && q_diff > s_diff)
        {
            q = -r - s;
        }
        else if (r_diff > s_diff)
        {
            r = -q - s;
        }
        else
        {
            s = -q - r;
        }

        return new Hex(q, r, s);
    }

    public bool Equals(HexF other)
    {
        return Q == other.Q && R == other.R && S == other.S;
    }

    public override bool Equals(object obj)
    {
        return obj is HexF other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Q, R, S);
    }
}
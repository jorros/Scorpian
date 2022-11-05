using System;
using System.Numerics;

namespace Scorpian.Maths;

public class BoundingFrustum : IEquatable<BoundingFrustum>
{
    private Matrix4x4 _matrix;
    private readonly Vector3[] _corners = new Vector3[CornerCount];
    private readonly Plane[] _planes = new Plane[PlaneCount];

    private const int PlaneCount = 6;

    public const int CornerCount = 8;

    public BoundingFrustum(Matrix4x4 value)
    {
        _matrix = value;
        CreatePlanes();
        CreateCorners();
    }

    public Matrix4x4 Matrix
    {
        get => _matrix;
        set
        {
            _matrix = value;
            CreatePlanes(); // FIXME: The odds are the planes will be used a lot more often than the matrix
            CreateCorners(); // is updated, so this should help performance. I hope ;)
        }
    }

    public Plane Near => _planes[0];
    public Plane Far => _planes[1];
    public Plane Left => _planes[2];
    public Plane Right => _planes[3];
    public Plane Top => _planes[4];
    public Plane Bottom => _planes[5];

    public static bool operator ==(BoundingFrustum a, BoundingFrustum b)
    {
        if (Equals(a, null))
            return Equals(b, null);

        if (Equals(b, null))
            return false;

        return a._matrix == b._matrix;
    }

    public static bool operator !=(BoundingFrustum a, BoundingFrustum b)
    {
        return !(a == b);
    }

    public bool Equals(BoundingFrustum other)
    {
        return (this == other);
    }

    public override bool Equals(object obj)
    {
        var f = obj as BoundingFrustum;
        return !Equals(f, null) && (this == f);
    }
    
    public Vector3[] GetCorners()
    {
        return (Vector3[])_corners.Clone();
    }
		
    public void GetCorners(Vector3[] corners)
    {
        if (corners == null) throw new ArgumentNullException(nameof(corners));
        if (corners.Length < CornerCount) throw new ArgumentOutOfRangeException(nameof(corners));

        _corners.CopyTo(corners, 0);
    }
    
    public override int GetHashCode()
    {
        return _matrix.GetHashCode();
    }
    
    private void CreatePlanes()
    {            
        _planes[0] = new Plane(-_matrix.M13, -_matrix.M23, -_matrix.M33, -_matrix.M43);
        _planes[1] = new Plane(_matrix.M13 - _matrix.M14, _matrix.M23 - _matrix.M24, _matrix.M33 - _matrix.M34, _matrix.M43 - _matrix.M44);
        _planes[2] = new Plane(-_matrix.M14 - _matrix.M11, -_matrix.M24 - _matrix.M21, -_matrix.M34 - _matrix.M31, -_matrix.M44 - _matrix.M41);
        _planes[3] = new Plane(_matrix.M11 - _matrix.M14, _matrix.M21 - _matrix.M24, _matrix.M31 - _matrix.M34, _matrix.M41 - _matrix.M44);
        _planes[4] = new Plane(_matrix.M12 - _matrix.M14, _matrix.M22 - _matrix.M24, _matrix.M32 - _matrix.M34, _matrix.M42 - _matrix.M44);
        _planes[5] = new Plane(-_matrix.M14 - _matrix.M12, -_matrix.M24 - _matrix.M22, -_matrix.M34 - _matrix.M32, -_matrix.M44 - _matrix.M42);
            
        NormalizePlane(ref _planes[0]);
        NormalizePlane(ref _planes[1]);
        NormalizePlane(ref _planes[2]);
        NormalizePlane(ref _planes[3]);
        NormalizePlane(ref _planes[4]);
        NormalizePlane(ref _planes[5]);
    }
    
    private static void NormalizePlane(ref Plane p)
    {
        var factor = 1f / p.Normal.Length();
        p.Normal.X *= factor;
        p.Normal.Y *= factor;
        p.Normal.Z *= factor;
        p.D *= factor;
    }
    
    private void CreateCorners()
    {
        IntersectionPoint(ref _planes[0], ref _planes[2], ref _planes[4], out _corners[0]);
        IntersectionPoint(ref _planes[0], ref _planes[3], ref _planes[4], out _corners[1]);
        IntersectionPoint(ref _planes[0], ref _planes[3], ref _planes[5], out _corners[2]);
        IntersectionPoint(ref _planes[0], ref _planes[2], ref _planes[5], out _corners[3]);
        IntersectionPoint(ref _planes[1], ref _planes[2], ref _planes[4], out _corners[4]);
        IntersectionPoint(ref _planes[1], ref _planes[3], ref _planes[4], out _corners[5]);
        IntersectionPoint(ref _planes[1], ref _planes[3], ref _planes[5], out _corners[6]);
        IntersectionPoint(ref _planes[1], ref _planes[2], ref _planes[5], out _corners[7]);
    }
    
    private static void IntersectionPoint(ref Plane a, ref Plane b, ref Plane c, out Vector3 result)
    {
        // Formula used
        //                d1 ( N2 * N3 ) + d2 ( N3 * N1 ) + d3 ( N1 * N2 )
        //P =   -------------------------------------------------------------------------
        //                             N1 . ( N2 * N3 )
        //
        // Note: N refers to the normal, d refers to the displacement. '.' means dot product. '*' means cross product
        
        var cross = Vector3.Cross(b.Normal, c.Normal);
        
        var f = Vector3.Dot(a.Normal, cross);
        f *= -1.0f;
            
        cross = Vector3.Cross(b.Normal, c.Normal);
        var v1 = Vector3.Multiply(cross, a.D);
        
        
        cross = Vector3.Cross(c.Normal, a.Normal);
        var v2 = Vector3.Multiply(cross, b.D);


        cross = Vector3.Cross(a.Normal, b.Normal);
        var v3 = Vector3.Multiply(cross, c.D);
        //v3 = (c.D * (Vector3.Cross(a.Normal, b.Normal)));
            
        result.X = (v1.X + v2.X + v3.X) / f;
        result.Y = (v1.Y + v2.Y + v3.Y) / f;
        result.Z = (v1.Z + v2.Z + v3.Z) / f;
    }
}
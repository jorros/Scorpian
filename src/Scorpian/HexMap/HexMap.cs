using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Scorpian.Graphics;
using Scorpian.Maths;

namespace Scorpian.HexMap;

public class HexMap<T> : IEnumerable<Hex> where T : class
{
    private readonly int _width;
    private readonly int _height;
    private readonly Size _tileSize;
    private readonly PointF _position;
    private readonly Func<Hex, T> _construct;
    private readonly List<HexMapLayer<T>> _layers;
    private readonly Dictionary<Hex, T> _tiles;

    private readonly Matrix3x2 _matrix = new((float) Math.Sqrt(3.0), 0.0f,
        (float) Math.Sqrt(3.0) / 2.0f,
        3.0f / 2.0f, 0, 0);

    private readonly Matrix3x2 _inverted = new((float) Math.Sqrt(3.0) / 3f, 0f,
        -1f / 3f, 2f / 3f, 0, 0);

    public SizeF Size { get; }

    public HexMap(int width, int height, Size tileSize, PointF position, Func<Hex, T> construct)
    {
        _width = width;
        _height = height;
        _tileSize = tileSize;
        _position = position;
        _construct = construct;
        _layers = new List<HexMapLayer<T>>();
        _tiles = new Dictionary<Hex, T>();
        
        Size = CalculateSize();

        Clear();
    }

    public void Clear()
    {
        _tiles.Clear();
        for (var r = 0; r < _height; r++)
        {
            for (var x = 0; x < _width; x++)
            {
                var q = x - (r >> 1);

                var position = new Hex(q, r, -q - r);

                _tiles.Add(position, _construct.Invoke(position));
            }
        }
    }

    public T GetData(Hex position)
    {
        return _tiles.ContainsKey(position) ? _tiles[position] : null;
    }

    public bool Contains(Hex hex) => _tiles.ContainsKey(hex);

    public bool HasNeighbour(Hex position, Func<T, bool> predicate, int range = 1)
    {
        var min = -range;

        for (var q = min; q <= range; q++)
        {
            for (var r = min; r <= range; r++)
            {
                for (var s = min; s <= range; s++)
                {
                    // Sum of cube coordinates should equal 0
                    if (q + r + s != 0)
                    {
                        continue;
                    }

                    var pos = new Hex(q, r, s) + position;
                    if (pos == position)
                    {
                        continue;
                    }

                    var tile = GetData(pos);
                    if (tile is null)
                    {
                        continue;
                    }

                    if (predicate.Invoke(tile))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public IReadOnlyList<T> GetNeighbours(Hex start, Func<T, bool> predicate = null, int range = 1)
    {
        var list = new List<T>();

        var min = -range;

        for (var q = min; q <= range; q++)
        {
            for (var r = min; r <= range; r++)
            {
                for (var s = min; s <= range; s++)
                {
                    // Sum of cube coordinates should equal 0
                    if (q + r + s != 0)
                    {
                        continue;
                    }

                    var pos = new Hex(q, r, s) + start;

                    if (pos == start)
                    {
                        continue;
                    }

                    var tile = GetData(pos);
                    if (tile == null)
                    {
                        continue;
                    }

                    if (predicate == null || predicate.Invoke(tile))
                    {
                        list.Add(tile);
                    }
                }
            }
        }

        return list;
    }

    public HexMapLayer<T> AddLayer()
    {
        var layer = new HexMapLayer<T>(_tiles.Keys, this);
        _layers.Add(layer);

        return layer;
    }

    public PointF HexToWorld(Hex position)
    {
        var result = Vector2.Transform(new Vector2(position.Q, position.R), _matrix);
        result *= new Vector2(_tileSize.Width, _tileSize.Height);
        
        return new PointF(result.X + _tileSize.Width / 2f + _position.X, result.Y + _tileSize.Height / 2f + _position.Y);
    }

    public Hex WorldToHex(PointF position)
    {
        position = position.Subtract(_position);
        var pt = new Vector2((position.X - _tileSize.Width / 2f) / _tileSize.Width,
            (position.Y - _tileSize.Height / 2f) / _tileSize.Height);
        
        var result = Vector2.Transform(pt, _inverted);
        var hex = new HexF(result.X, result.Y, -result.X - result.Y);
        
        var q = (int) Math.Round(hex.Q);
        var r = (int) Math.Round(hex.R);
        var s = (int) Math.Round(hex.S);

        var qDiff = Math.Abs(q - hex.Q);
        var rDiff = Math.Abs(r - hex.R);
        var sDiff = Math.Abs(s - hex.S);
        if (qDiff > rDiff && qDiff > sDiff)
        {
            q = -r - s;
        }
        else if (rDiff > sDiff)
        {
            r = -q - s;
        }
        else
        {
            s = -q - r;
        }
        
        return new Hex(q, r, s);
    }

    private PointF HexCornerOffset(int corner)
    {
        var angle = 2.0 * Math.PI *
            (0.5 + corner) / 6;
        return new PointF((float) (_tileSize.Width * Math.Cos(angle)), (float) (_tileSize.Height * Math.Sin(angle)));
    }

    private static RectangleF PointGetBounds(IEnumerable<PointF> points)
    {
        var left = float.MaxValue;
        var top = float.MaxValue;
        var right = float.MinValue;
        var bottom = float.MinValue;
        foreach (var p in points)
        {
            left = Math.Min(p.X, left);
            right = Math.Max(p.X, right);
            top = Math.Min(p.Y, top);
            bottom = Math.Max(p.Y, bottom);
        }

        return new RectangleF(left, top, Math.Abs(left - right), Math.Abs(top - bottom));
    }

    private SizeF CalculateSize()
    {
        var corners = new List<PointF>();
        for (var corner = 0; corner < 6; corner++)
        {
            corners.Add(HexCornerOffset(corner));
        }

        var cornerBounds = PointGetBounds(corners);
        
        return new SizeF(cornerBounds.Width * _width, cornerBounds.Height * _height * 0.75f);
    }

    private List<PointF> PolygonCorners(Hex h)
    {
        var corners = new List<PointF>();
        var center = HexToWorld(h);
        for (var i = 0; i < 6; i++)
        {
            var offset = HexCornerOffset(i);
            corners.Add(new PointF(center.X + offset.X,
                center.Y + offset.Y));
        }

        return corners;
    }

    public void Render(RenderContext renderContext)
    {
        foreach (var layer in _layers)
        {
            layer.Render(renderContext);
        }
        
        // renderContext.DrawLine(new PointF(0, 0), new PointF(Size.Width, 0), Color.Red);
        // renderContext.DrawLine(new PointF(Size.Width, 0), new PointF(Size.Width, Size.Height), Color.Red);
        // renderContext.DrawLine(new PointF(Size.Width, Size.Height), new PointF(0, Size.Height), Color.Red);
        // renderContext.DrawLine(new PointF(0, Size.Height), new PointF(0, 0), Color.Red);
        //
        // for (var r = 0; r < _height; r++)
        // {
        //     for (var x = 0; x < _width; x++)
        //     {
        //         var q = x - (r >> 1);
        //
        //         var hex = new Hex(q, r, -q - r);
        //         RenderDebug(renderContext, hex, Color.Indigo);
        //     }
        // }
    }

    private void RenderDebug(RenderContext renderContext, Hex hex, Color color)
    {
        var corners = PolygonCorners(hex);

        for (var i = 1; i < 6; i++)
        {
            renderContext.DrawLine(new Point((int) corners[i - 1].X, (int) corners[i - 1].Y),
                new Point((int) corners[i].X, (int) corners[i].Y), color);
        }
    }

    public IEnumerator<Hex> GetEnumerator()
    {
        return _tiles.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
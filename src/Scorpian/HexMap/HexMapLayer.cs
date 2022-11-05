using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using Scorpian.Asset;
using Scorpian.Graphics;
using static Scorpian.SDL.SDL;

namespace Scorpian.HexMap;

public class HexMapLayer<T> where T : class
{
    private readonly IEnumerable<Hex> _fields;
    private readonly HexMap<T> _parent;
    private readonly ConcurrentDictionary<Hex, Sprite> _tiles;
    private readonly Dictionary<Hex, int> _frames;

    private ulong _lastUpdate;

    internal HexMapLayer(IEnumerable<Hex> fields, HexMap<T> parent)
    {
        _fields = fields;
        _parent = parent;
        _tiles = new ConcurrentDictionary<Hex, Sprite>();
        _frames = new Dictionary<Hex, int>();
    }

    public void SetSprite(Hex position, Sprite tile)
    {
        _tiles[position] = tile;
    }

    public Sprite GetSprite(Hex position)
    {
        return _tiles[position];
    }

    public void Clear()
    {
        _tiles.Clear();
        _frames.Clear();

        foreach (var hex in _fields)
        {
            _tiles.TryAdd(hex, null);
        }
    }

    public void Render(RenderContext renderContext)
    {
        var current = SDL_GetTicks64();

        foreach (var hex in _tiles.Keys)
        {
            if (!_tiles.TryGetValue(hex, out var sprite))
            {
                continue;
            }
            
            if (sprite is null)
            {
                continue;
            }

            RenderTile(sprite, current, renderContext, hex);
        }
    }

    private void RenderTile(Sprite sprite, ulong current, RenderContext renderContext, Hex hex)
    {
        var position = (_parent.HexToWorld(hex) - sprite.Size / 2);

        if (sprite is AnimatedSprite animated)
        {
            var dT = (current - _lastUpdate) / 1000.0f;
            var framesToUpdate = (int) Math.Floor(dT / (1.0f / 24));

            if (!_frames.ContainsKey(hex))
            {
                _frames[hex] = 0;
            }

            if (framesToUpdate > 0)
            {
                _frames[hex] += framesToUpdate;
                _frames[hex] %= animated.FramesCount;
                _lastUpdate = current;
            }

            renderContext.Draw(sprite, new RectangleF(position, sprite.Size), 0, Color.White, 255, _frames[hex]);
            return;
        }

        renderContext.Draw(sprite, position);
    }
}
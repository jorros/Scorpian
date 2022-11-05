using System;
using System.Collections.Generic;
using System.Drawing;
using Scorpian.Asset.Markup;
using Scorpian.Graphics;
using Scorpian.Helper;
using static Scorpian.SDL.SDL;
using static Scorpian.SDL.SDL_ttf;

namespace Scorpian.Asset.Font;

public class TextBlockRenderer : IFontRenderer
{
    private readonly GraphicsManager _graphicsManager;
    public Type Type => typeof(TextBlock);

    private readonly Dictionary<(Point position, bool isOutline), (string text, IntPtr texture)> _textureCache = new();

    public TextBlockRenderer(GraphicsManager graphicsManager)
    {
        _graphicsManager = graphicsManager;
    }

    public IEnumerable<(IntPtr texture, SDL_Rect target)> Render(Font font, IBlock block, Point position,
        ref Point cursor)
    {
        var textBlock = (TextBlock) block;
        var cachedOptions = CachedTextOptions.FromTextBlock(textBlock);

        var textures = new List<(IntPtr texture, SDL_Rect target)>();

        var width = RenderTexture(cachedOptions.Outline > 0, cursor);

        if (cachedOptions.Outline > 0)
        {
            cachedOptions = cachedOptions with {Outline = 0};
            RenderTexture(false, cursor);
        }

        cursor = cursor with {X = cursor.X + width};

        int RenderTexture(bool isOutline, Point pos)
        {
            var fnt = font.LoadFont(cachedOptions);

            var (texture, surface) = GenerateTexture(_graphicsManager, fnt, textBlock, pos, isOutline);

            SDL_QueryTexture(texture, out _, out _, out var w, out var h);

            var target = new SDL_Rect
            {
                x = pos.X,
                y = pos.Y,
                w = w,
                h = h
            };

            textures.Add((texture, target));

            if (surface != IntPtr.Zero)
            {
                SDL_FreeSurface(surface);
            }

            return w;
        }

        return textures;
    }

    public void CalculateSize(string text, Font font, IBlock block, ref Point cursor)
    {
        var textBlock = (TextBlock) block;
        var cachedOptions = CachedTextOptions.FromTextBlock(textBlock);

        var fnt = font.LoadFont(cachedOptions);
        TTF_SizeUTF8(fnt, textBlock.Text, out var w, out var h);
        cursor = new Point {X = cursor.X + w, Y = Math.Max(h, cursor.Y)};
    }

    public void Clear()
    {
        foreach (var texture in _textureCache.Values)
        {
            SDL_DestroyTexture(texture.texture);
        }
    }

    private (IntPtr texture, IntPtr surface) GenerateTexture(GraphicsManager context, IntPtr font,
        TextBlock text, Point position, bool isOutline)
    {
        var texture = IntPtr.Zero;
        var surface = IntPtr.Zero;

        if (_textureCache.ContainsKey((position, isOutline)))
        {
            var cached = _textureCache[(position, isOutline)];
            if (cached.text == text.Text)
            {
                texture = cached.texture;
            }
            else
            {
                SDL_DestroyTexture(cached.texture);
            }
        }

        if (texture == IntPtr.Zero)
        {
            surface = TTF_RenderUTF8_Blended(font, text.Text,
                isOutline ? text.OutlineColor.ToSdl() : text.Color.ToSdl());

            texture = SDL_CreateTextureFromSurface(context.Renderer, surface);

            _textureCache[(position, isOutline)] = (text.Text, texture);
        }

        return (texture, surface);
    }
}
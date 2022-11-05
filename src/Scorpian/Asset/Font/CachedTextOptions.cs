using System.Drawing;
using Scorpian.Asset.Markup;

namespace Scorpian.Asset.Font;

public record CachedTextOptions(int Size, int Outline, Color OutlineColor, FontStyle Style)
{
    public static CachedTextOptions FromTextBlock(TextBlock block)
    {
        return new CachedTextOptions(block.Size, block.Outline, block.OutlineColor, block.Style);
    }
};
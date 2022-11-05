using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Scorpian.Asset.Markup;
using Scorpian.Graphics;
using Scorpian.Helper;
using SharpCompress.Archives;
using static Scorpian.SDL.SDL;

namespace Scorpian.Asset.AssetLoaders;

internal class FontLoader : IAssetLoader
{
    private readonly FontMarkupReader _fontMarkupReader;
    private readonly GraphicsManager _graphicsManager;

    public FontLoader(FontMarkupReader fontMarkupReader, GraphicsManager graphicsManager)
    {
        _fontMarkupReader = fontMarkupReader;
        _graphicsManager = graphicsManager;
    }
    
    public IEnumerable<string> Extensions => new[] {".otf", ".ttf"};
    public IReadOnlyList<(string key, IAsset asset)> Load(IArchiveEntry entry, IArchive archive)
    {
        using var entryStream = entry.OpenEntryStream();
        using var memory = new MemoryStream();
        entryStream.CopyTo(memory);
        var data = memory.ToArray();
        
        var size = Marshal.SizeOf(typeof(byte)) * data.Length;
        var pnt = Marshal.AllocHGlobal(size);
        Marshal.Copy(data, 0, pnt, size);

        var rw = SDL_RWFromMem(pnt, size);

        var fonts = new (string key, IAsset asset)[]
        {
            (entry.GetAssetKey(), new Font.Font(rw, _fontMarkupReader, _graphicsManager))
        };

        return fonts;
    }
}
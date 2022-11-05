using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scorpian.Asset.AssetLoaders;
using Scorpian.Helper;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers.Tar;

namespace Scorpian.Asset;

public class AssetManager
{
    private IEnumerable<IAssetLoader> _assetLoaders;
    private Dictionary<string, AssetBundle> _assetBundles;
    private bool _highRes;

    public bool HighRes => _highRes;

    internal void Init(IEnumerable<IAssetLoader> assetLoaders, bool highRes)
    {
        _assetLoaders = assetLoaders;
        _assetBundles = new Dictionary<string, AssetBundle>();
        _highRes = highRes;
    }

    public AssetBundle Load(string name)
    {
        var bundle = new Dictionary<string, IAsset>();

        using var stream = File.Open(Path.Combine("Content", $"{name}.pack"), FileMode.Open);
        using var archive = ArchiveFactory.Open(stream);

        var allowedExtensions = GetAllowedExtensions();

        foreach (var entry in archive.Entries.Where(entry =>
                     !entry.IsDirectory && allowedExtensions.Contains(Path.GetExtension(entry.Key).ToLowerInvariant())))
        {
            var ext = Path.GetExtension(entry.Key).ToLowerInvariant();

            switch (_highRes)
            {
                case true when archive.Entries.Any(p =>
                    p.Key.Equals($"{entry.GetAssetKey()}@2x{ext}", StringComparison.InvariantCultureIgnoreCase)) && !IsHighRes(entry):
                case false when IsHighRes(entry):
                    continue;
            }

            var loader = _assetLoaders.First(x => x.Extensions.Contains(ext));

            var assets = loader.Load(entry, archive);

            if (assets is null || !assets.Any())
            {
                continue;
            }

            foreach (var asset in assets)
            {
                bundle.TryAdd(asset.key, asset.asset);
            }
        }

        var assetBundle = new AssetBundle(bundle);
        _assetBundles[name] = assetBundle;

        return assetBundle;
    }

    private static bool IsHighRes(IArchiveEntry entry)
    {
        return entry.Key.Contains("@2x.");
    }

    private IEnumerable<string> GetAllowedExtensions()
    {
        return _assetLoaders.SelectMany(x => x.Extensions);
    }

    public T Get<T>(string name) where T : class, IAsset
    {
        var split = name.Split(':');

        if (!_assetBundles.ContainsKey(split[0]))
        {
            throw new EngineException($"Tried to access not loaded asset bundle {name}.");
        }

        var assetBundle = _assetBundles[split[0]];

        return assetBundle.Get<T>(split[1]);
    }

    public static void Pack(string src, string output)
    {
        using var archive = ArchiveFactory.Create(ArchiveType.Zip);

        archive.AddAllFromDirectory(src);
        archive.SaveTo(output, new TarWriterOptions(CompressionType.Deflate, true));
    }
}
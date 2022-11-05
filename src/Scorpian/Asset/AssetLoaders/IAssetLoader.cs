using System.Collections.Generic;
using SharpCompress.Archives;

namespace Scorpian.Asset.AssetLoaders;

internal interface IAssetLoader
{
    IEnumerable<string> Extensions { get; }

    IReadOnlyList<(string key, IAsset asset)> Load(IArchiveEntry entry, IArchive archive);
}
using System;
using System.Collections.Generic;

namespace Scorpian.Asset;

public class AssetBundle : IDisposable
{
    private readonly Dictionary<string, IAsset> _assets;

    internal AssetBundle(Dictionary<string, IAsset> assets)
    {
        _assets = assets;
    }

    public T Get<T>(string name) where T : class, IAsset
    {
        return _assets[name] as T;
    }

    public void Dispose()
    {
        foreach (var asset in _assets.Values)
        {
            switch (asset)
            {
                case TextureSprite sprite:
                    sprite.Dispose();
                    break;
                
                case Font.Font font:
                    font.Dispose();
                    break;
                
                default:
                    Console.WriteLine("Unknown asset in bundle. Could not remove from memory.");
                    break;
            }
        }
        
        _assets.Clear();
    }
}
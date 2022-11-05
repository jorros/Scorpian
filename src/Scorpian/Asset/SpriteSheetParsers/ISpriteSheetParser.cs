using System.IO;

namespace Scorpian.Asset.SpriteSheetParsers;

internal interface ISpriteSheetParser
{
    SpritesheetDescriptor Read(Stream stream);
}
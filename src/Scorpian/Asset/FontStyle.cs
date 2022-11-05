using System;

namespace Scorpian.Asset;

[Flags]
public enum FontStyle
{
    Normal = 0x00,
    Bold = 0x01,
    Italic = 0x02,
    Underline = 0x04,
    Strikethrough = 0x08
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Scorpian.Helper;

namespace Scorpian.Asset.Markup;

internal class FontMarkupReader
{
    private readonly Cache<(string, TextBlock), IReadOnlyList<IBlock>> _cache = new();

    private static readonly XmlReaderSettings Settings = new()
    {
        IgnoreWhitespace = true,
        CheckCharacters = false,
        ConformanceLevel = ConformanceLevel.Fragment,
    };

    public IEnumerable<IBlock> Read(string content, TextBlock @default = null)
    {
        IReadOnlyList<IBlock> CalculateTextBlocks()
        {
            content = content.ReplaceLineEndings("<br/>");
            using var stringReader = new StringReader(content);
            using var reader = XmlReader.Create(stringReader, Settings);

            var blocks = new List<IBlock>();
            IBlock current = @default ?? new TextBlock();
            blocks.Add(current);

            var currentElement = string.Empty;

            while (reader.MoveToNextAttribute() || reader.Read())
            {
                TextBlock previousBlock;
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        previousBlock = GetPrevious(blocks);
                        
                        if (reader.Name == "br")
                        {
                            current = new NewLineBlock {LineHeight = (int) Math.Round(previousBlock.Size * 1.1)};
                            blocks.Add(current);
                            break;
                        }

                        current = previousBlock with {Text = string.Empty};
                        blocks.Add(current);

                        currentElement = reader.Name;
                        break;

                    case XmlNodeType.Text:
                        previousBlock = GetPrevious(blocks);

                        current = previousBlock with {Text = reader.Value};
                        blocks.Add(current);
                        break;

                    case XmlNodeType.Attribute:
                        if (current is TextBlock tb)
                        {
                            Process(currentElement.ToLowerInvariant(), reader.Name.ToLowerInvariant(), reader.Value,
                                tb);
                        }
                        break;

                    case XmlNodeType.EndElement:
                        previousBlock = GetPrevious(blocks);
                        
                        current = previousBlock with {Text = string.Empty};
                        blocks.Add(current);
                        break;
                }
            }

            return blocks.Where(x =>
            {
                if (x is TextBlock t)
                {
                    return !string.IsNullOrEmpty(t.Text);
                }

                return true;
            }).ToArray();
        }

        return _cache.Get((content, @default), CalculateTextBlocks);
    }

    private TextBlock GetPrevious(IReadOnlyList<IBlock> blocks)
    {
        TextBlock previousBlock = null;
        for (var i = blocks.Count - 1; i >= 0; i--)
        {
            if (blocks[i] is TextBlock tb)
            {
                previousBlock = tb;
            }
        }

        return previousBlock;
    }

    private static void Process(string type, string attribute, string value, TextBlock textBlock)
    {
        switch (type)
        {
            case "text":
                ProcessText(attribute, value, textBlock);
                break;

            case "outline":
                ProcessOutline(attribute, value, textBlock);
                break;
        }
    }

    private static void ProcessText(string attribute, string value, TextBlock textBlock)
    {
        switch (attribute)
        {
            case "color":
                textBlock.Color = ColorHelper.HtmlToColor(value);
                break;

            case "size":
                if (int.TryParse(value, out var size))
                {
                    textBlock.Size = size;
                }

                break;

            case "style":
                if (Enum.TryParse(value, true, out FontStyle fontStyle))
                {
                    textBlock.Style = fontStyle;
                }

                break;
        }
    }

    private static void ProcessOutline(string attribute, string value, TextBlock textBlock)
    {
        switch (attribute)
        {
            case "color":
                textBlock.OutlineColor = ColorHelper.HtmlToColor(value);
                break;

            case "size":
                if (int.TryParse(value, out var size))
                {
                    textBlock.Outline = size;
                }

                break;
        }
    }
}
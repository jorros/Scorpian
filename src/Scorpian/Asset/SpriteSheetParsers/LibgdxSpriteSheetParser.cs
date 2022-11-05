using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Scorpian.Maths;

namespace Scorpian.Asset.SpriteSheetParsers;

internal class LibgdxSpriteSheetParser : ISpriteSheetParser
{
    public SpritesheetDescriptor Read(Stream stream)
    {
        var frames = new List<SpritesheetFrame>();
        var desc = new SpritesheetDescriptor
        {
            Frames = frames
        };

        using var streamReader = new StreamReader(stream);

        var name = streamReader.ReadLine();

        SpritesheetFrame currentFrame = null;

        while (!streamReader.EndOfStream)
        {
            var line = streamReader.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.StartsWith("  "))
            {
                var data = Parse(line);
                AssignField(data, currentFrame);

                continue;
            }

            if (line.Contains(":"))
            {
                continue;
            }

            currentFrame = new SpritesheetFrame
            {
                Name = line
            };

            frames.Add(currentFrame);
        }

        return desc;
    }

    private void AssignField((string field, object value) data, SpritesheetFrame frame)
    {
        if (frame is null)
        {
            return;
        }

        switch (data.field)
        {
            case "xy":
                frame.Position = (Point) data.value;
                break;

            case "size":
                frame.Size = new Size((Point) data.value);
                break;

            case "split":
                frame.Split = new Box((Rectangle) data.value);
                break;

            case "orig":
                frame.OriginalSize = new Size((Point) data.value);
                break;

            case "offset":
                frame.Offset = (Point) data.value;
                break;

            case "rotate":
                frame.Rotated = (bool) data.value;
                break;

            case "index":
                frame.Index = (int) data.value;
                break;
        }
    }

    private (string field, object value) Parse(string line)
    {
        var trimmed = line.Trim();
        var kvp = trimmed.Split(':');

        var data = kvp[1].Trim();

        object val = data switch
        {
            "false" => false,
            "true" => true,
            _ => null
        };

        if (val != null)
        {
            return (kvp[0], val);
        }

        if (TryParseRectangle(kvp[1], out var rectangle))
        {
            return (kvp[0], rectangle);
        }

        if (TryParsePoint(kvp[1], out var point))
        {
            return (kvp[0], point);
        }

        if (int.TryParse(kvp[1], out var integer))
        {
            return (kvp[0], integer);
        }

        return (kvp[0], null);
    }

    private static bool TryParseRectangle(string line, out Rectangle rectangle)
    {
        var fields = line.Split(',');

        if (fields.Length != 4)
        {
            rectangle = Rectangle.Empty;
            return false;
        }

        rectangle = new Rectangle(int.Parse(fields[0]), int.Parse(fields[2]), int.Parse(fields[1]),
            int.Parse(fields[3]));
        return true;
    }

    private static bool TryParsePoint(string line, out Point point)
    {
        var fields = line.Split(',');

        if (fields.Length != 2)
        {
            point = Point.Empty;
            return false;
        }

        point = new Point(int.Parse(fields[0]), int.Parse(fields[1]));
        return true;
    }
}
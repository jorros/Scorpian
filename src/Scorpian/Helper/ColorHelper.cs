// Taken from https://stackoverflow.com/a/61871235

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;

namespace Scorpian.Helper;

public static class ColorHelper
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "ColorTranslator throws System.Exception")]
    public static Color HtmlToColor(string htmlColor)
    {
        var htmlLowerCase = htmlColor.ToLower().Trim();

        try
        {
            if (htmlLowerCase.StartsWith("rgb"))
            {
                return ArgbToColor(htmlLowerCase);
            }

            if (htmlLowerCase.StartsWith("#"))
            {
                return HexToColor(htmlLowerCase);
            }

            // Fallback to ColorTranslator for named colors, e.g. "Black", "White" etc.
            return ColorTranslator.FromHtml(htmlLowerCase);
        }
        catch
        {
            // ColorTranslator throws System.Exception, don't really care what the actual error is.
        }

        return Color.Black;
    }

    private static Color HexToColor(string htmlLowerCase)
    {
        var len = htmlLowerCase.Length;

        switch (len)
        {
            // #RGB
            case 4:
            {
                var r = Convert.ToInt32(htmlLowerCase.Substring(1, 1), 16);
                var g = Convert.ToInt32(htmlLowerCase.Substring(2, 1), 16);
                var b = Convert.ToInt32(htmlLowerCase.Substring(3, 1), 16);

                return Color.FromArgb(r + (r * 16), g + (g * 16), b + (b * 16));
            }
            // #RGBA
            case 5:
            {
                var r = Convert.ToInt32(htmlLowerCase.Substring(1, 1), 16);
                var g = Convert.ToInt32(htmlLowerCase.Substring(2, 1), 16);
                var b = Convert.ToInt32(htmlLowerCase.Substring(3, 1), 16);
                var a = Convert.ToInt32(htmlLowerCase.Substring(4, 1), 16);

                return Color.FromArgb(a + (a * 16), r + (r * 16), g + (g * 16), b + (b * 16));
            }
            // #RRGGBB
            case 7:
                return Color.FromArgb(
                    Convert.ToInt32(htmlLowerCase.Substring(1, 2), 16),
                    Convert.ToInt32(htmlLowerCase.Substring(3, 2), 16),
                    Convert.ToInt32(htmlLowerCase.Substring(5, 2), 16));
            // #RRGGBBAA
            case 9:
                return Color.FromArgb(
                    Convert.ToInt32(htmlLowerCase.Substring(7, 2), 16),
                    Convert.ToInt32(htmlLowerCase.Substring(1, 2), 16),
                    Convert.ToInt32(htmlLowerCase.Substring(3, 2), 16),
                    Convert.ToInt32(htmlLowerCase.Substring(5, 2), 16));
            default:
                return Color.Black;
        }
    }

    private static Color ArgbToColor(string htmlLowerCase)
    {
        var left = htmlLowerCase.IndexOf('(');
        var right = htmlLowerCase.IndexOf(')');

        if (left < 0 || right < 0)
        {
            return Color.Black;
        }

        var noBrackets = htmlLowerCase.Substring(left + 1, right - left - 1);

        var parts = noBrackets.Split(',');

        var r = int.Parse(parts[0], CultureInfo.InvariantCulture);
        var g = int.Parse(parts[1], CultureInfo.InvariantCulture);
        var b = int.Parse(parts[2], CultureInfo.InvariantCulture);

        switch (parts.Length)
        {
            case 3:
                return Color.FromArgb(r, g, b);
            case 4:
            {
                var a = float.Parse(parts[3], CultureInfo.InvariantCulture);

                return Color.FromArgb((int)(a * 255), r, g, b);
            }
            default:
                return Color.Black;
        }
    }
}
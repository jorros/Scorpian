using System.Collections.Generic;

namespace Scorpian.Helper;

public static class ResolutionHelper
{
    internal static IEnumerable<(int width, int height)> SupportedResolutions => new[]
    {
        (1366, 768),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160)
    };
}
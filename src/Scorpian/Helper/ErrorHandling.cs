using Microsoft.Extensions.Logging;
using static Scorpian.SDL.SDL;

namespace Scorpian.Helper;

public static class ErrorHandling
{
    public static void Handle(ILogger logger, int code)
    {
        if (code != 0)
        {
            logger.LogError("{Error}", SDL_GetError());
        }
    }
}
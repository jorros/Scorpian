using System;

namespace Scorpian.Network;

[Flags]
public enum NetworkMode
{
    Client = 1,
    Server = 2
}
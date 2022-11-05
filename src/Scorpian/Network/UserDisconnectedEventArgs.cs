using System;

namespace Scorpian.Network;

public class UserDisconnectedEventArgs : EventArgs
{
    public ushort ClientId { get; set; }
}
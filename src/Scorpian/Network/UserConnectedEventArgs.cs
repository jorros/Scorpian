using System;

namespace Scorpian.Network;

public class UserConnectedEventArgs : EventArgs
{
    public ushort ClientId { get; set; }
}
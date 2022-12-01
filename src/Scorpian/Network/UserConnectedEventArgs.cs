using System;

namespace Scorpian.Network;

public class UserConnectedEventArgs : EventArgs
{
    public uint ClientId { get; set; }

    public UserConnectedEventArgs(uint clientId)
    {
        ClientId = clientId;
    }
}
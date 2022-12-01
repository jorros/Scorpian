using System;

namespace Scorpian.Network;

public class UserDisconnectedEventArgs : EventArgs
{
    public UserDisconnectedEventArgs(uint clientId)
    {
        ClientId = clientId;
    }
    
    public uint ClientId { get; set; }
}
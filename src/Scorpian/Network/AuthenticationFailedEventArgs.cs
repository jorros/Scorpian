using System;

namespace Scorpian.Network;

public class AuthenticationFailedEventArgs : EventArgs
{
    public string Reason { get; set; }

    public AuthenticationFailedEventArgs(string reason)
    {
        Reason = reason;
    }
}
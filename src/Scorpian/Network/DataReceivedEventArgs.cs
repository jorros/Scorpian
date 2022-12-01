using System;

namespace Scorpian.Network;

public class DataReceivedEventArgs : EventArgs
{
    public object Data { get; set; }
    
    public uint ClientId { get; set; }
}
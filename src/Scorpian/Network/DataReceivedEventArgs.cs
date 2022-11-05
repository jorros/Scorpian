using System;

namespace Scorpian.Network;

public class DataReceivedEventArgs : EventArgs
{
    public object Data { get; set; }
    
    public ushort SenderId { get; set; }
}
using System;

namespace Scorpian.InputManagement;

public class KeyboardEventArgs : EventArgs
{
    public KeyboardEventType Type { get; set; }
    
    public bool Repeated { get; set; }
    
    public KeyboardKey Key { get; set; }
}

public enum KeyboardEventType
{
    KeyDown,
    KeyUp
}
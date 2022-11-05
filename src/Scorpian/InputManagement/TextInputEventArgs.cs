using System;

namespace Scorpian.InputManagement;

public class TextInputEventArgs : EventArgs
{
    public char Character { get; set; }
}
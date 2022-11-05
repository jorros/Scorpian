namespace Scorpian.Network.Protocol;

public class VarChangedEventArgs<T>
{
    public T NewValue { get; set; }
    
    public T OldValue { get; set; }
}
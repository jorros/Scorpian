using System;

namespace Scorpian.Helper;

public class Dirty<T>
{
    private T _value;
    private bool _dirty;
    
    private Dirty(T value)
    {
        _value = value;
    }

    public static implicit operator Dirty<T>(T value)
    {
        return new Dirty<T>(value);
    }
    
    public event EventHandler OnValueChanged;

    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            _dirty = true;
            OnValueChanged?.Invoke(this, null);
        }
    }

    public bool IsDirty
    {
        get
        {
            var isDirty = _dirty;
            _dirty = false;
            return isDirty;
        }
    }
}
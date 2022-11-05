using System;
using Scorpian.Graphics;

namespace Scorpian.Helper;

public class DelayedAction
{
    private readonly RenderContext _renderContext;
    private readonly Action _action;

    private bool _isInvoked;

    public DelayedAction(RenderContext renderContext, Action action)
    {
        _renderContext = renderContext;
        _action = action;
    }

    public void TryInvoke(uint initial)
    {
        if (_isInvoked)
        {
            _action.Invoke();
            return;
        }
        
        _renderContext.InvokeIn(initial, InitialInvoke);
    }

    public void Reset()
    {
        _isInvoked = false;
    }

    public void InvokeIfNotInvoked()
    {
        if (!_isInvoked)
        {
            _action.Invoke();
        }
    }

    private void InitialInvoke()
    {
        _isInvoked = true;
    }
}
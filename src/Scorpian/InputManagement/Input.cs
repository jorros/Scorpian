using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using static Scorpian.SDL.SDL;

namespace Scorpian.InputManagement;

public static class Input
{
    public static event EventHandler<KeyboardEventArgs> OnKeyboard;
    public static event EventHandler<TextInputEventArgs> OnTextInput;
    public static event EventHandler<MouseMoveEventArgs> OnMouseMove;
    public static event EventHandler<MouseButtonEventArgs> OnMouseButton;
    public static event EventHandler<MouseWheelEventArgs> OnMouseWheel;

    public static Point MousePosition { get; private set; }

    private static ConcurrentDictionary<MouseButton, State> MouseState { get; } = new();

    private static IEnumerable<(MouseButton, uint)> _mButtons = new[]
    {
        (MouseButton.Left, SDL_BUTTON_LMASK), (MouseButton.Middle, SDL_BUTTON_MMASK),
        (MouseButton.Right, SDL_BUTTON_RMASK), (MouseButton.X1, SDL_BUTTON_X1MASK), (MouseButton.X2, SDL_BUTTON_X2MASK)
    };

    private enum State
    {
        Active,
        Down,
        Up
    }

    internal static void UpdateMouseState()
    {
        var state = SDL_GetMouseState(IntPtr.Zero, IntPtr.Zero);

        foreach (var mButton in _mButtons)
        {
            if ((state & mButton.Item2) != 0)
            {
                if (MouseState.ContainsKey(mButton.Item1))
                {
                    MouseState[mButton.Item1] = State.Active;
                }
                else
                {
                    MouseState[mButton.Item1] = State.Down;
                }
            }
            else
            {
                if (!MouseState.ContainsKey(mButton.Item1))
                {
                    continue;
                }

                if (MouseState[mButton.Item1] != State.Up)
                {
                    MouseState[mButton.Item1] = State.Up;
                }
                else
                {
                    MouseState.Remove(mButton.Item1, out _);
                }
            }
        }
    }

    private static int CalculateWithDpi(int val, bool highDpi)
    {
        return highDpi ? val * 2 : val;
    }

    internal static void RaiseEvent(SDL_KeyboardEvent key)
    {
        OnKeyboard?.Invoke(null, new KeyboardEventArgs
        {
            Type = key.type == SDL_EventType.SDL_KEYUP ? KeyboardEventType.KeyUp : KeyboardEventType.KeyDown,
            Repeated = key.repeat != 0,
            Key = key.keysym.scancode.ToKey()
        });
    }

    internal static void RaiseTextInput(char input)
    {
        OnTextInput?.Invoke(null, new TextInputEventArgs
        {
            Character = input
        });
    }

    internal static void CaptureMouseMotion(SDL_MouseMotionEvent motion, bool highDpi)
    {
        MousePosition = new Point(CalculateWithDpi(motion.x, highDpi), CalculateWithDpi(motion.y, highDpi));
        OnMouseMove?.Invoke(null, new MouseMoveEventArgs
        {
            X = CalculateWithDpi(motion.x, highDpi),
            Y = CalculateWithDpi(motion.y, highDpi),
            DeltaX = motion.xrel,
            DeltaY = motion.yrel
        });
    }

    internal static void CaptureMouseWheel(SDL_MouseWheelEvent wheel)
    {
        OnMouseWheel?.Invoke(null, new MouseWheelEventArgs
        {
            X = wheel.x,
            Y = wheel.y,
            PreciseX = wheel.preciseX,
            PreciseY = wheel.preciseY
        });
    }

    internal static void CaptureMouseButton(SDL_MouseButtonEvent button, bool highDpi)
    {
        OnMouseButton?.Invoke(null, new MouseButtonEventArgs
        {
            Clicks = button.clicks,
            Type = button.type == SDL_EventType.SDL_MOUSEBUTTONUP ? MouseEventType.ButtonUp : MouseEventType.ButtonDown,
            X = CalculateWithDpi(button.x, highDpi),
            Y = CalculateWithDpi(button.y, highDpi),
            Button = (MouseButton) button.button
        });
    }

    public static bool IsKeyDown(KeyboardKey key)
    {
        var ptr = SDL_GetKeyboardState(out var num);
        var keys = new byte[num];
        Marshal.Copy(ptr, keys, 0, num);

        return keys[(int) key] == 1;
    }

    public static bool IsButtonDown(MouseButton button)
    {
        if (MouseState.ContainsKey(button))
        {
            return MouseState[button] == State.Down;
        }

        return false;
    }
    
    public static bool IsButton(MouseButton button)
    {
        if (MouseState.ContainsKey(button))
        {
            return MouseState[button] == State.Active;
        }

        return false;
    }
    
    public static bool IsButtonUp(MouseButton button)
    {
        if (MouseState.ContainsKey(button))
        {
            return MouseState[button] == State.Up;
        }

        return false;
    }
}